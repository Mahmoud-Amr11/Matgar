import { useState, type FormEvent } from 'react';
import { api } from '../api';
import { useLoad, useMutation, Spinner, ErrorNotice, EmptyState, StatusBadge, money, productStatusLabel, IconCheck, IconPlus, IconPencil, IconTrash, IconX } from '../ui';
import type { Coupon } from '../types';

type Tab = 'orders' | 'products' | 'categories' | 'coupons';

export function AdminPage() {
    const [tab, setTab] = useState<Tab>('orders');
    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Admin console</p>
                    <h1>Manage the store</h1>
                </div>
                <div className="tabs">
                    <button type="button" className={tab === 'orders' ? 'tab active' : 'tab'} onClick={() => setTab('orders')}>Orders</button>
                    <button type="button" className={tab === 'products' ? 'tab active' : 'tab'} onClick={() => setTab('products')}>Products</button>
                    <button type="button" className={tab === 'categories' ? 'tab active' : 'tab'} onClick={() => setTab('categories')}>Categories</button>
                    <button type="button" className={tab === 'coupons' ? 'tab active' : 'tab'} onClick={() => setTab('coupons')}>Coupons</button>
                </div>
            </header>
            {tab === 'orders' && <AdminOrders />}
            {tab === 'products' && <AdminProducts />}
            {tab === 'categories' && <AdminCategories />}
            {tab === 'coupons' && <AdminCoupons />}
        </div>
    );
}

function AdminOrders() {
    const { data, loading, error, reload } = useLoad(() => api.adminOrders(), []);
    if (loading) return <Spinner label="Loading orders…" />;
    if (error) return <ErrorNotice message={error} onRetry={reload} />;
    if (!data || data.items.length === 0) return <EmptyState title="No orders yet" />;
    return (
        <div className="list">
            {data.items.map(order => (
                <div className="row-card" key={order.id}>
                    <div className="row-main">
                        <strong>#{order.id.slice(0, 8).toUpperCase()}</strong>
                        <span className="muted">{new Date(order.createdAt).toLocaleString()}</span>
                        {order.customerId && <span className="muted">Customer {order.customerId.slice(0, 8)}</span>}
                    </div>
                    <div className="row-side">
                        <StatusBadge status={order.status} />
                        <span className="row-price">{money(order.totalAmount)}</span>
                    </div>
                </div>
            ))}
        </div>
    );
}

function AdminProducts() {
    const products = useLoad(() => api.products({ pageSize: 100 }), []);
    const moderate = useMutation((input: { id: string; type: 'approve' | 'suspend'; reason?: string }) =>
        input.type === 'approve' ? api.approveProduct(input.id) : api.suspendProduct(input.id, input.reason ?? 'Suspended by admin'));

    const [suspendReason, setSuspendReason] = useState<Record<string, string>>({});

    if (products.loading) return <Spinner label="Loading products…" />;
    if (products.error) return <ErrorNotice message={products.error} onRetry={products.reload} />;

    const runModeration = async (id: string, type: 'approve' | 'suspend') => {
        const ok = await moderate.run({ id, type, reason: type === 'suspend' ? suspendReason[id] : undefined });
        if (ok) products.reload();
    };

    return (
        <div className="list">
            {products.data?.items.length === 0 && <EmptyState title="No products" />}
            {products.data?.items.map(product => (
                <div className="row-card" key={product.productId}>
                    <div className="row-main">
                        <strong>{product.productName}</strong>
                        <span className="muted">{product.categoryName}</span>
                        <StatusBadge status={productStatusLabel(product.status)} />
                    </div>
                    <div className="row-side wrap">
                        {product.status === 1 && (
                            <button type="button" className="button button-small" disabled={moderate.loading} onClick={() => void runModeration(product.productId, 'approve')}>
                                <IconCheck size={14} /> Approve
                            </button>
                        )}
                        {product.status !== 3 && (
                            <>
                                <input
                                    aria-label="Suspend reason"
                                    className="small-input"
                                    placeholder="Reason…"
                                    value={suspendReason[product.productId] ?? ''}
                                    onChange={event => setSuspendReason({ ...suspendReason, [product.productId]: event.target.value })}
                                />
                                <button type="button" className="button button-small button-danger-on" disabled={moderate.loading} onClick={() => void runModeration(product.productId, 'suspend')}>
                                    Suspend
                                </button>
                            </>
                        )}
                        {product.status === 3 && <span className="muted">Suspended</span>}
                    </div>
                </div>
            ))}
        </div>
    );
}

function AdminCategories() {
    const { data, loading, error, reload } = useLoad(() => api.categories({ page: 1, pageSize: 100 }), []);
    const [newName, setNewName] = useState('');
    const [editingId, setEditingId] = useState('');
    const [editName, setEditName] = useState('');
    const [busy, setBusy] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);

    const create = async (event: FormEvent) => {
        event.preventDefault();
        setBusy(true);
        setActionError(null);
        try { await api.createCategory(newName.trim()); setNewName(''); reload(); } finally { setBusy(false); }
    };

    const saveEdit = async () => {
        setBusy(true);
        setActionError(null);
        try { await api.updateCategory(editingId, editName.trim()); setEditingId(''); reload(); } finally { setBusy(false); }
    };

    const remove = async (id: string) => {
        setBusy(true);
        setActionError(null);
        try { await api.deleteCategory(id); reload(); } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Something went wrong.');
        } finally { setBusy(false); }
    };

    if (loading) return <Spinner label="Loading categories…" />;
    if (error) return <ErrorNotice message={error} onRetry={reload} />;

    return (
        <div className="stack">
            {actionError && <div className="error" role="alert">{actionError}</div>}
            <form className="card form-row" onSubmit={create}>
                <input aria-label="New category name" className="flex-1" placeholder="New category name" value={newName} onChange={event => setNewName(event.target.value)} required />
                <button type="submit" className="button" disabled={busy}><IconPlus size={16} /> Add</button>
            </form>
            <div className="chip-list">
                {data?.items.map(category => (
                    <span className="chip" key={category.categoryId}>
                        {editingId === category.categoryId ? (
                            <>
                                <input aria-label="Edit category name" value={editName} onChange={event => setEditName(event.target.value)} />
                                <button className="icon-button" aria-label="Save category" disabled={busy} onClick={() => void saveEdit()}><IconCheck size={14} /></button>
                                <button className="icon-button" aria-label="Cancel edit" onClick={() => setEditingId('')}><IconX size={14} /></button>
                            </>
                        ) : (
                            <>
                                <strong>{category.categoryName}</strong>
                                <button
                                    className="icon-button"
                                    aria-label={`Edit ${category.categoryName}`}
                                    onClick={() => { setEditingId(category.categoryId); setEditName(category.categoryName); }}
                                >
                                    <IconPencil size={14} />
                                </button>
                                <button className="icon-button danger" aria-label={`Delete ${category.categoryName}`} onClick={() => void remove(category.categoryId)}><IconTrash size={14} /></button>
                            </>
                        )}
                    </span>
                ))}
            </div>
        </div>
    );
}

const emptyCoupon = { code: '', discountType: '0', discountValue: '', minOrderAmount: '', maxUsageCount: '1', expiryDate: '' };

function AdminCoupons() {
    const { data, loading, error, reload } = useLoad(() => api.coupons(), []);
    const createMutation = useMutation(() => api.createCoupon({
        code: form.code,
        discountType: Number(form.discountType),
        discountValue: Number(form.discountValue),
        minOrderAmount: form.minOrderAmount === '' ? null : Number(form.minOrderAmount),
        maxUsageCount: Number(form.maxUsageCount),
        expiryDate: new Date(form.expiryDate).toISOString(),
    }));
    const [form, setForm] = useState(emptyCoupon);

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        const ok = await createMutation.run();
        if (ok && form.code) { setForm(emptyCoupon); reload(); }
    };

    if (loading) return <Spinner label="Loading coupons…" />;
    if (error) return <ErrorNotice message={error} onRetry={reload} />;

    return (
        <div className="stack">
            <form className="card stack" onSubmit={submit}>
                <h3>New coupon</h3>
                <div className="form-row">
                    <label className="field">
                        <span className="field-label">Code</span>
                        <input value={form.code} onChange={event => setForm({ ...form, code: event.target.value })} required placeholder="TAKE10" />
                    </label>
                    <label className="field">
                        <span className="field-label">Type</span>
                        <select value={form.discountType} onChange={event => setForm({ ...form, discountType: event.target.value })}>
                            <option value="0">Percentage</option>
                            <option value="1">Fixed amount</option>
                        </select>
                    </label>
                    <label className="field">
                        <span className="field-label">Value</span>
                        <input type="number" step="0.01" min="0" value={form.discountValue} onChange={event => setForm({ ...form, discountValue: event.target.value })} required />
                    </label>
                </div>
                <div className="form-row">
                    <label className="field">
                        <span className="field-label">Min order (optional)</span>
                        <input type="number" step="0.01" min="0" value={form.minOrderAmount} onChange={event => setForm({ ...form, minOrderAmount: event.target.value })} placeholder="Leave empty for none" />
                    </label>
                    <label className="field">
                        <span className="field-label">Max uses</span>
                        <input type="number" step="1" min="1" value={form.maxUsageCount} onChange={event => setForm({ ...form, maxUsageCount: event.target.value })} required />
                    </label>
                    <label className="field">
                        <span className="field-label">Expires</span>
                        <input type="date" value={form.expiryDate} onChange={event => setForm({ ...form, expiryDate: event.target.value })} required />
                    </label>
                </div>
                {createMutation.error && <div className="error">{createMutation.error}</div>}
                <button type="submit" className="button" disabled={createMutation.loading}><IconPlus size={16} /> Create coupon</button>
            </form>

            <div className="coupons-grid">
                {data?.length === 0 && <EmptyState title="No coupons yet" />}
                {data?.map(coupon => <CouponCard key={coupon.id} coupon={coupon} />)}
            </div>
        </div>
    );
}

function CouponCard({ coupon }: { coupon: Coupon }) {
    const active = coupon.isActive && new Date(coupon.expiryDate) > new Date();
    return (
        <div className="coupon-card card">
            <div className="coupon-code">{coupon.code}</div>
            <div className="coupon-detail">
                <span>{coupon.discountType === 0 ? `${coupon.discountValue}% off` : money(coupon.discountValue) + ' off'}</span>
                <span className="muted">Min order {coupon.minOrderAmount ? money(coupon.minOrderAmount) : 'None'}</span>
                <span className="muted">{coupon.usedCount}/{coupon.maxUsageCount} used · expires {new Date(coupon.expiryDate).toLocaleDateString()}</span>
                <StatusBadge status={active ? 'Active' : 'Expired'} />
            </div>
        </div>
    );
}