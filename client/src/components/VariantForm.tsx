import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { api } from '../api';
import { useLoad } from '../ui';
import { IconCheck, IconPlus, IconX } from '../ui';
import type { ProductVariant, ProductListItem } from '../types';

interface VariantFormProps {
    product: Pick<ProductListItem, 'productId' | 'productName'>;
    initial?: ProductVariant | null;
    onDone: (productId: string) => void;
    onCancel: () => void;
}

interface AttributeRow {
    name: string;
    value: string;
}

interface Errors {
    sku?: string;
    price?: string;
    stock?: string;
    attributes?: string;
    combination?: string;
    api?: string;
}

function parseAttributes(json: string): AttributeRow[] {
    try {
        const parsed = JSON.parse(json || '{}') as Record<string, unknown>;
        return Object.entries(parsed).map(([name, value]) => ({
            name,
            value: typeof value === 'string' ? value : JSON.stringify(value),
        }));
    } catch {
        return [];
    }
}

function comboKey(attributes: Record<string, string>): string {
    return Object.entries(attributes)
        .filter(([, value]) => Boolean(value))
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([name, value]) => `${name}=${value}`)
        .join('|');
}

type AttributeDimension = { name: string; values: string[] };

export function VariantForm({ product, initial, onDone, onCancel }: VariantFormProps) {
    const isEdit = Boolean(initial);
    const variants = useLoad(() => api.variants(product.productId), [product.productId]);

    const dimensions = useMemo<AttributeDimension[]>(() => {
        const byName = new Map<string, Set<string>>();
        const source = [...(variants.data ?? [])];
        if (initial) source.unshift(initial);
        for (const variant of source) {
            for (const { name, value } of parseAttributes(variant.attributesJson)) {
                if (!name) continue;
                if (!byName.has(name)) byName.set(name, new Set());
                if (value) byName.get(name)!.add(value);
            }
        }
        return [...byName.entries()]
            .sort(([a], [b]) => a.localeCompare(b))
            .map(([name, values]) => ({ name, values: [...values].sort() }));
    }, [variants.data, initial]);

    const [sku, setSku] = useState(initial?.sku ?? '');
    const [price, setPrice] = useState(initial ? String(initial.price) : '');
    const [stock, setStock] = useState('');
    const [imageUrl, setImageUrl] = useState(initial?.imageUrl ?? '');
    const [rows, setRows] = useState<AttributeRow[]>(() => {
        if (initial) return parseAttributes(initial.attributesJson);
        return dimensions.map(d => ({ name: d.name, value: '' }));
    });
    const [errors, setErrors] = useState<Errors>({});
    const [submitting, setSubmitting] = useState(false);
    const [saved, setSaved] = useState(false);

    useEffect(() => {
        if (initial) return;
        setRows(current => {
            if (current.some(r => r.name)) return current;
            return dimensions.map(d => ({ name: d.name, value: '' }));
        });
    }, [dimensions, initial]);

    const usedSkus = useMemo(() => {
        const set = new Set<string>();
        for (const v of variants.data ?? []) {
            if (v.variantId === initial?.variantId) continue;
            set.add(v.sku.toLowerCase());
        }
        return set;
    }, [variants.data, initial]);

    const usedCombos = useMemo(() => {
        const set = new Set<string>();
        for (const v of variants.data ?? []) {
            if (v.variantId === initial?.variantId) continue;
            set.add(comboKey(Object.fromEntries(parseAttributes(v.attributesJson).map(r => [r.name, r.value]))));
        }
        return set;
    }, [variants.data, initial]);

    const valuesFor = (name: string) => dimensions.find(d => d.name === name)?.values ?? [];

    const addRow = () => setRows([...rows, { name: '', value: '' }]);
    const updateRow = (index: number, patch: Partial<AttributeRow>) => {
        setRows(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));
    };
    const removeRow = (index: number) => setRows(rows.filter((_, i) => i !== index));

    const validate = (): boolean => {
        const next: Errors = {};
        if (!isEdit) {
            if (!sku.trim()) next.sku = 'SKU is required.';
            else if (!/^[A-Za-z0-9\-_]+$/.test(sku.trim())) next.sku = 'SKU can only contain letters, numbers, hyphens, and underscores.';
            else if (usedSkus.has(sku.trim().toLowerCase())) next.sku = 'This SKU is already in use.';
        }

        const priceNumber = Number(price);
        if (price === '' || Number.isNaN(priceNumber)) next.price = 'Price is required.';
        else if (priceNumber < 0) next.price = 'Price cannot be negative.';
        else if (priceNumber > 1_000_000) next.price = 'Price seems unreasonably high.';

        if (!isEdit) {
            const stockNumber = Number(stock);
            if (stock === '' || Number.isNaN(stockNumber) || !Number.isInteger(stockNumber)) next.stock = 'Stock quantity must be a whole number.';
            else if (stockNumber < 0) next.stock = 'Stock quantity cannot be negative.';
        }

        const names = new Set<string>();
        for (const row of rows) {
            if (row.name.trim()) {
                if (names.has(row.name.trim())) {
                    next.attributes = `Duplicate attribute "${row.name.trim()}".`;
                    break;
                }
                names.add(row.name.trim());
                if (!row.value.trim()) {
                    next.attributes = `Pick a value for "${row.name.trim()}".`;
                    break;
                }
            }
        }

        const attributes = Object.fromEntries(rows.filter(r => r.name.trim() && r.value.trim()).map(r => [r.name.trim(), r.value.trim()]));
        if (usedCombos.has(comboKey(attributes))) {
            next.combination = 'A variant with these exact attributes already exists for this product.';
        }

        setErrors(next);
        return Object.keys(next).length === 0;
    };

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        if (submitting) return;
        if (!validate()) return;

        const attributes = Object.fromEntries(rows.filter(r => r.name.trim() && r.value.trim()).map(r => [r.name.trim(), r.value.trim()]));
        const attributesJson = JSON.stringify(attributes);
        setSubmitting(true);
        setErrors({});
        try {
            if (initial) {
                await api.updateVariant(product.productId, initial.variantId, {
                    price: Number(price),
                    imageUrl: imageUrl.trim() || null,
                    attributesJson,
                });
            } else {
                await api.createVariant(product.productId, {
                    sku: sku.trim(),
                    price: Number(price),
                    imageUrl: imageUrl.trim() || null,
                    attributesJson,
                    initialQuantity: Number(stock),
                });
            }
            setSaved(true);
            onDone(product.productId);
        } catch (err) {
            setErrors({ api: err instanceof Error ? err.message : 'Could not save the variant.' });
        } finally {
            setSubmitting(false);
        }
    };

    const namesId = `attr-names-${product.productId.replace(/-/g, '').slice(0, 8)}`;

    return (
        <form className="card stack" onSubmit={submit}>
            <header className="variant-form-head">
                <div>
                    <p className="eyebrow">{isEdit ? 'Edit variant' : 'Create variant'}</p>
                    <h3>{isEdit ? 'Update a purchasable configuration' : 'Make this product purchasable'}</h3>
                </div>
                <div className="variant-form-product">
                    <span className="field-label">Product</span>
                    <strong>{product.productName}</strong>
                </div>
            </header>

            <section className="variant-section">
                <h4>Variant information</h4>
                <div className="form-row">
                    {isEdit ? (
                        <label className="field">
                            <span className="field-label">SKU</span>
                            <input value={initial?.sku ?? ''} disabled />
                            <span className="muted">SKU is fixed after creation.</span>
                        </label>
                    ) : (
                        <label className="field">
                            <span className="field-label">SKU</span>
                            <input value={sku} onChange={event => setSku(event.target.value)} placeholder="TSHIRT-RED-M" />
                            {errors.sku && <span className="error">{errors.sku}</span>}
                        </label>
                    )}
                    <label className="field">
                        <span className="field-label">Price</span>
                        <input type="number" step="0.01" min="0" value={price} onChange={event => setPrice(event.target.value)} placeholder="123.00" />
                        {errors.price && <span className="error">{errors.price}</span>}
                    </label>
                    {isEdit ? (
                        <label className="field">
                            <span className="field-label">Stock quantity</span>
                            <input value={initial ? String(initial.availableQuantity) : ''} disabled />
                            <span className="muted">Adjust inventory from the store.</span>
                        </label>
                    ) : (
                        <label className="field">
                            <span className="field-label">Stock quantity</span>
                            <input type="number" step="1" min="0" value={stock} onChange={event => setStock(event.target.value)} placeholder="10" />
                            {errors.stock && <span className="error">{errors.stock}</span>}
                        </label>
                    )}
                </div>
            </section>

            <section className="variant-section">
                <div className="variant-section-head">
                    <h4>Variant attributes</h4>
                    <button type="button" className="button button-small button-outline" onClick={addRow}><IconPlus size={14} /> Add attribute</button>
                </div>
                {dimensions.length > 0 && (
                    <p className="muted">Fields are based on the attributes already used on this product: {dimensions.map(d => d.name).join(', ')}.</p>
                )}
                <div className="stack">
                    {rows.map((row, index) => (
                        <div className="form-row attr-row" key={`${row.name}-${index}`}>
                            <label className="field">
                                <span className="field-label">Name</span>
                                <input
                                    list={namesId}
                                    value={row.name}
                                    onChange={event => updateRow(index, { name: event.target.value })}
                                    placeholder="Color"
                                />
                            </label>
                            <label className="field">
                                <span className="field-label">Value</span>
                                <input
                                    list={`${namesId}-values-${index}`}
                                    value={row.value}
                                    onChange={event => updateRow(index, { value: event.target.value })}
                                    placeholder="Red"
                                />
                            </label>
                            <button type="button" className="icon-button" aria-label="Remove attribute" onClick={() => removeRow(index)}><IconX size={14} /></button>
                        </div>
                    ))}
                    {rows.length === 0 && <span className="muted">No attributes. Add one, or leave empty for a single-variant product.</span>}
                </div>
                {errors.attributes && <span className="error">{errors.attributes}</span>}
                {errors.combination && <span className="error">{errors.combination}</span>}
            </section>

            <section className="variant-section">
                <h4>Variant image</h4>
                <div className="form-row">
                    <label className="field">
                        <span className="field-label">Image URL</span>
                        <input value={imageUrl} onChange={event => setImageUrl(event.target.value)} placeholder="https://…" />
                    </label>
                    {imageUrl && (
                        <div className="variant-image-preview">
                            <img src={imageUrl} alt="Variant preview" />
                        </div>
                    )}
                </div>
            </section>

            {initial && <p className="muted">Editing updates price, image, and attributes. SKU and stock are managed elsewhere.</p>}
            {errors.api && <div className="error" role="alert">{errors.api}</div>}
            {variants.error && <div className="error" role="alert">{variants.error}</div>}
            {saved && <p className="success"><IconCheck size={14} /> Variant {isEdit ? 'updated' : 'created'}.</p>}

            <datalist id={namesId}>
                {[...new Set([...dimensions.map(d => d.name), 'Color', 'Size'])].map(name => <option key={name} value={name} />)}
            </datalist>
            {rows.map((row, index) => (
                <datalist key={`values-${index}`} id={`${namesId}-values-${index}`}>
                    {valuesFor(row.name).map(value => <option key={value} value={value} />)}
                </datalist>
            ))}

            <div className="actions">
                <button type="submit" className="button" disabled={submitting}>
                    {submitting ? 'Saving…' : isEdit ? 'Save variant' : 'Save variant'}
                </button>
                <button type="button" className="button button-outline" disabled={submitting} onClick={onCancel}>Cancel</button>
            </div>
        </form>
    );
}