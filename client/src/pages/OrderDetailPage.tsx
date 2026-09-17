import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { api } from '../api';
import { useAuth } from '../auth';
import { EmptyState, ErrorNotice, IconChevronLeft, Spinner, StatusBadge, date, money, useLoad } from '../ui';

export function OrderDetailPage() {
    const { id = '' } = useParams();
    const { user } = useAuth();
    const navigate = useNavigate();
    const { data, loading, error, reload } = useLoad(() => api.order(id), [id]);
    const [cancelling, setCancelling] = useState(false);
    const [cancelError, setCancelError] = useState<string | null>(null);

    if (loading) return <div className="page"><Spinner label="Loading order…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} onRetry={reload} /></div>;
    if (!data) return <div className="page"><EmptyState title="Order not found" /></div>;

    const canCancel = user?.roles.includes('Customer') && data.status === 'Pending' && !cancelling;

    const cancel = async () => {
        setCancelling(true);
        setCancelError(null);
        try {
            await api.cancelOrder(data.id);
            reload();
        } catch (err) {
            setCancelError(err instanceof Error ? err.message : 'Could not cancel order.');
        } finally {
            setCancelling(false);
        }
    };

    return (
        <div className="page">
            <Link to="/orders" className="back-link"><IconChevronLeft size={14} /> Back to orders</Link>
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Order</p>
                    <h1>#{data.id.slice(0, 8).toUpperCase()}</h1>
                </div>
                <StatusBadge status={data.status} />
            </header>

            <div className="order-detail">
                <div className="card">
                    <h3>Items</h3>
                    <div className="detail-items">
                        {data.items.map(item => (
                            <div className="detail-item" key={`${item.productVariantId}-${item.sku}`}>
                                <div>
                                    <strong>{item.sku}</strong>
                                    <span className="muted">Variant {item.productVariantId.slice(0, 8)}</span>
                                </div>
                                <span>{money(item.unitPrice)} × {item.quantity}</span>
                                <strong>{money(item.unitPrice * item.quantity)}</strong>
                            </div>
                        ))}
                    </div>
                    <div className="summary-line total"><span>Subtotal</span><span>{money(data.subTotal)}</span></div>
                    <div className="summary-line"><span>Discount</span><span>{data.discountAmount > 0 ? `– ${money(data.discountAmount)}` : '—'}</span></div>
                    <div className="summary-line total"><span>Total</span><span>{money(data.totalAmount)}</span></div>
                </div>

                <div className="card">
                    <h3>Details</h3>
                    <div className="kv">
                        <span>Placed</span><span>{date(data.createdAt)}</span>
                        <span>Status</span><span><StatusBadge status={data.status} /></span>
                        <span>Shipping address</span><span>{data.shippingAddressSnapshot || '—'}</span>
                    </div>
                    {canCancel && (
                        <>
                            {cancelError && <div className="error" role="alert">{cancelError}</div>}
                            <button type="button" className="button button-danger" disabled={cancelling} onClick={() => void cancel()}>
                                {cancelling ? 'Cancelling…' : 'Cancel order'}
                            </button>
                        </>
                    )}
                    {user?.roles.includes('Customer') && data.status === 'Pending' && (
                        <button type="button" className="button button-outline" onClick={() => navigate(`/orders/${data.id}/pay`)}>Pay now</button>
                    )}
                </div>
            </div>
        </div>
    );
}