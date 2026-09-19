import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { useLoad, useMutation, Spinner, ErrorNotice, EmptyState, StatusBadge, money, productStatusLabel, IconPlus, IconPencil, IconTrash, IconSend, IconMinus } from '../ui';
import { VariantForm } from '../components/VariantForm';
import type { OrderSummary, ProductListItem, ProductVariant } from '../types';

const emptyProduct = { productId: '', name: '', description: '', categoryId: '' };

type Tab = 'products' | 'orders';

export function VendorPage() {
    const [tab, setTab] = useState<Tab>('orders');
    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Vendor workspace</p>
                    <h1>Manage your store</h1>
                </div>
                <div className="tabs">
                    <button type="button" className={tab === 'orders' ? 'tab active' : 'tab'} onClick={() => setTab('orders')}>Incoming orders</button>
                    <button type="button" className={tab === 'products' ? 'tab active' : 'tab'} onClick={() => setTab('products')}>Products</button>
                </div>
            </header>
            {tab === 'orders' ? <VendorOrders /> : <VendorProducts />}
        </div>
    );
}

function VendorOrders() {
    const { data, loading, error, reload } = useLoad(() => api.vendorOrders(), []);
    const statusUpdate = useMutation((orderId: string, newStatus: number) => api.updateVendorOrderStatus(orderId, newStatus));

    const nextActions: Record<string, number> = { Pending: 1, Confirmed: 2, Shipped: 3 };
    const nextLabel: Record<string, string> = { Pending: 'Confirm', Confirmed: 'Ship', Shipped: 'Deliver' };

    const advance = async (order: OrderSummary) => {
        const next = nextActions[order.status];
        if (next === undefined) return;
        const ok = await statusUpdate.run(order.id, next);
        if (ok) reload();
    };

    if (loading) return <Spinner label="Loading orders…" />;
    if (error) return <ErrorNotice message={error} onRetry={reload} />;
    if (!data || data.items.length === 0) return <EmptyState title="No incoming orders" hint="When anyone orders your products, they'll appear here." />;

    return (
        <div className="list">
            <div className="vendor-products-grid vendor-orders-grid header-row">
                <span>Order</span><span>Date</span><span>Total</span><span>Status</span><span></span>
            </div>
            {data.items.map(order => {
                const next = nextActions[order.status];
                return (
                    <div className="vendor-products-grid vendor-orders-grid row" key={order.id}>
                        <Link to={`/orders/${order.id}`} className="row-link">#{order.id.slice(0, 8).toUpperCase()}</Link>
                        <span className="muted">{new Date(order.createdAt).toLocaleString()}</span>
                        <span>{money(order.totalAmount)}</span>
                        <span><StatusBadge status={order.status} /></span>
                        <span>
                            {next !== undefined && (
                                <button
                                    type="button"
                                    className="button button-small"
                                    disabled={statusUpdate.loading}
                                    onClick={() => void advance(order)}
                                >
                                    {statusUpdate.loading ? 'Saving…' : nextLabel[order.status]}
                                </button>
                            )}
                            {statusUpdate.error && <span className="error">{statusUpdate.error}</span>}
                        </span>
                    </div>
                );
            })}
        </div>
    );
}

function VendorProducts() {
    const products = useLoad(() => api.products({ pageSize: 100 }), []);
    const categories = useLoad(() => api.categories({ page: 1, pageSize: 100 }), []);
    const [editing, setEditing] = useState<typeof emptyProduct | null>(null);
    const [variantEditing, setVariantEditing] = useState<{ product: ProductListItem; variant: ProductVariant | null } | null>(null);
    const [openProductId, setOpenProductId] = useState<string | null>(null);
    const [variantSignal, setVariantSignal] = useState(0);
    const [note, setNote] = useState<string | null>(null);

    const productMutation = useMutation((payload: { productId: string; name: string; description: string; categoryId: string }) =>
        payload.productId ? api.updateProduct(payload.productId, payload) : api.createProduct(payload).then(() => undefined));

    const startCreate = () => setEditing({ ...emptyProduct });
    const startEdit = async (product: ProductListItem) => {
        setEditing({ productId: product.productId, name: product.productName, description: '', categoryId: product.categoryId });
        try {
            const details = await api.product(product.productId);
            setEditing({ productId: product.productId, name: details.productName, description: details.description ?? '', categoryId: details.categoryId });
        } catch { /* keep the basic info loaded from the list */ }
    };

    const saveProduct = async (event: FormEvent) => {
        event.preventDefault();
        if (!editing) return;
        setNote(null);
        if (!editing.categoryId) {
            setNote('Choose a category for the product.');
            return;
        }
        const ok = await productMutation.run(editing);
        if (ok) { setEditing(null); products.reload(); }
    };

    const removeProduct = async (productId: string) => {
        setNote(null);
        try {
            await api.deleteProduct(productId);
            products.reload();
        } catch (err) {
            setNote(err instanceof Error ? err.message : 'Could not delete product.');
        }
    };

    const submitProduct = async (productId: string) => {
        setNote(null);
        try {
            await api.submitProduct(productId);
            products.reload();
        } catch (err) {
            setNote(err instanceof Error ? err.message : 'Could not submit product.');
        }
    };

    const startVariant = (product: ProductListItem) => setVariantEditing({ product, variant: null });

    const handleVariantSaved = (productId: string) => {
        setVariantEditing(null);
        setOpenProductId(productId);
        setVariantSignal(v => v + 1);
        products.reload();
    };

    const removeVariant = async (productId: string, variant: ProductVariant) => {
        setNote(null);
        try {
            await api.deleteVariant(productId, variant.variantId);
            setVariantSignal(v => v + 1);
            products.reload();
        } catch (err) {
            setNote(err instanceof Error ? err.message : 'Could not delete variant.');
        }
    };

    if (products.loading || categories.loading) return <Spinner label="Loading workspace…" />;
    if (products.error) return <ErrorNotice message={products.error} onRetry={products.reload} />;

    return (
        <div className="stack">
            <div className="toolbar">
                <button type="button" className="button" onClick={startCreate}><IconPlus size={16} /> New product</button>
                {note && <span className="error">{note}</span>}
            </div>

            {editing && (
                <form className="card stack" onSubmit={saveProduct}>
                    <h3>{editing.productId ? 'Edit product' : 'New product'}</h3>
                    <div className="form-row">
                        <label className="field">
                            <span className="field-label">Name</span>
                            <input value={editing.name} onChange={event => setEditing({ ...editing, name: event.target.value })} required minLength={3} maxLength={100} />
                        </label>
                        <label className="field">
                            <span className="field-label">Category</span>
                            <select value={editing.categoryId} onChange={event => setEditing({ ...editing, categoryId: event.target.value })} required>
                                <option value="">Choose category</option>
                                {categories.data?.items.map(category => (
                                    <option key={category.categoryId} value={category.categoryId}>{category.categoryName}</option>
                                ))}
                            </select>
                        </label>
                    </div>
                    <label className="field">
                        <span className="field-label">Description</span>
                        <textarea rows={3} value={editing.description} onChange={event => setEditing({ ...editing, description: event.target.value })} />
                    </label>
                    {productMutation.error && <div className="error">{productMutation.error}</div>}
                    <div className="actions">
                        <button type="submit" className="button" disabled={productMutation.loading}>{productMutation.loading ? 'Saving…' : 'Save'}</button>
                        <button type="button" className="button button-outline" onClick={() => setEditing(null)}>Cancel</button>
                    </div>
                </form>
            )}

            {variantEditing && (
                <VariantForm
                    product={variantEditing.product}
                    initial={variantEditing.variant}
                    onDone={handleVariantSaved}
                    onCancel={() => setVariantEditing(null)}
                />
            )}

            <div className="list">
                {products.data?.items.length === 0 && <EmptyState title="No products yet" hint="Create your first product to start selling." />}
                {products.data?.items.map(product => (
                    <VendorProductRow
                        key={product.productId}
                        product={product}
                        categories={categories.data?.items ?? []}
                        open={openProductId === product.productId}
                        variantSignal={variantSignal}
                        onToggle={() => setOpenProductId(openProductId === product.productId ? null : product.productId)}
                        onEdit={() => startEdit(product)}
                        onAddVariant={() => startVariant(product)}
                        onDelete={() => void removeProduct(product.productId)}
                        onSubmit={() => void submitProduct(product.productId)}
                        onRemoveVariant={(variant) => void removeVariant(product.productId, variant)}
                        onEditVariant={(variant) => setVariantEditing({ product, variant })}
                    />
                ))}
            </div>
        </div>
    );
}

function VendorProductRow({ product, categories, open, variantSignal, onToggle, onEdit, onAddVariant, onDelete, onSubmit, onRemoveVariant, onEditVariant }: {
    product: ProductListItem;
    categories: { categoryId: string; categoryName: string }[];
    open: boolean;
    variantSignal: number;
    onToggle: () => void;
    onEdit: () => void;
    onAddVariant: () => void;
    onDelete: () => void;
    onSubmit: () => void;
    onRemoveVariant: (variant: ProductVariant) => void;
    onEditVariant: (variant: ProductVariant) => void;
}) {
    const variants = useLoad(
        open ? () => api.variants(product.productId) : async () => null,
        [product.productId, open, variantSignal]
    );

    return (
        <div className="vendor-product">
            <div className="vendor-product-head">
                <div className="vendor-product-title">
                    <strong>{product.productName}</strong>
                    <span className="muted">{categories.find(c => c.categoryId === product.categoryId)?.categoryName ?? product.categoryName}</span>
                    <StatusBadge status={productStatusLabel(product.status)} />
                </div>
                <div className="vendor-product-actions">
                    {product.status === 0 && <button type="button" className="button button-small" onClick={onSubmit}><IconSend size={14} /> Submit for review</button>}
                    <button type="button" className="button button-small button-outline" onClick={onEdit}><IconPencil size={14} /> Edit</button>
                    <button type="button" className="button button-small button-outline" onClick={onAddVariant}><IconPlus size={14} /> Variant</button>
                    <button type="button" className="icon-button danger" aria-label={`Delete ${product.productName}`} onClick={onDelete}><IconTrash size={16} /></button>
                    <button type="button" className="icon-button" aria-label="Toggle variants" onClick={onToggle}>
                        {open ? <IconMinus size={16} /> : <IconPlus size={16} />}
                    </button>
                </div>
            </div>

            {open && (
                <div className="variant-list">
                    <div className="variant-list-head">
                        <span>SKU</span><span>Price</span><span>Stock</span><span>Attributes</span><span></span>
                    </div>
                    {variants.loading && <Spinner label="Loading variants…" />}
                    {variants.error && <ErrorNotice message={variants.error} onRetry={variants.reload} />}
                    {variants.data?.map(variant => (
                        <div className="variant-row" key={variant.variantId}>
                            <span>{variant.sku}</span>
                            <span>{money(variant.price)}</span>
                            <span>{variant.availableQuantity}</span>
                            <span className="muted">{variant.attributesJson}</span>
                            <span className="variant-actions">
                                <button type="button" className="icon-button" aria-label="Edit variant" onClick={() => onEditVariant(variant)}><IconPencil size={14} /></button>
                                <button type="button" className="icon-button danger" aria-label="Delete variant" onClick={() => onRemoveVariant(variant)}><IconTrash size={14} /></button>
                            </span>
                        </div>
                    ))}
                    {!variants.loading && !variants.error && (variants.data?.length === 0 || !variants.data) && <span className="muted">No variants. Add one to make this product purchasable.</span>}
                </div>
            )}
        </div>
    );
}