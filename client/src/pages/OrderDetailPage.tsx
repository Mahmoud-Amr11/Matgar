import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { api } from '../api';
import { isAdmin, isVendor, useAuth } from '../auth';
import { EmptyState, ErrorNotice, IconChevronLeft, Spinner, StatusBadge, date, money, useLoad, useMutation } from '../ui';

const nextActions: Record<string, number> = { Pending: 1, Confirmed: 2, Shipped: 3 };
const nextLabels: Record<string, string> = { Pending: 'Confirm order', Confirmed: 'Mark shipped', Shipped: 'Mark delivered' };

export function OrderDetailPage() {
    const { id = '' } = useParams();
    const { user } = useAuth();
    const navigate = useNavigate();
    const { data, loading, error, reload } = useLoad(() => api.order(id), [id]);
    const [cancelling, setCancelling] = useState(false);
    const [cancelError, setCancelError] = useState<string | null>(null);
    const statusUpdate = useMutation((newStatus: number) => api.updateVendorOrderStatus(id, newStatus));

    if (loading) return <div className="page"><Spinner label="Loading order…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} onRetry={reload} /></div>;
    if (!data) return <div className="page"><EmptyState title="Order not found" /></div>;

    const owner = Boolean(user?.id && data.customerId && user.id.toLowerCase() === data.customerId.toLowerCase());
    const vendorViewer = isVendor(user) && !owner;
    const adminViewer = isAdmin(user);
    const vendorNext = vendorViewer ? nextActions[data.status] : undefined;

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

    const advance = async () => {
        if (vendorNext === undefined) return;
        const ok = await statusUpdate.run(vendorNext);
        if (ok) reload();
    };

    return (
        <div className="page">
            <Link to={vendorViewer ? '/vendor' : '/orders'} className="back-link">
                <IconChevronLeft size={14} /> {vendorViewer ? 'Back to workspace' : 'Back to orders'}
            </Link>
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">{vendorViewer ? 'Incoming order' : 'Order'}</p>
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
                        {vendorViewer && data.customerId && (
                            <><span>Customer</span><span>#{data.customerId.slice(0, 8).toUpperCase()}</span></>
                        )}
                        {!vendorViewer && <span>Shipping address</span>}
                        {!vendorViewer && <span>{data.shippingAddressSnapshot || '—'}</span>}
                    </div>

                    {owner && data.status === 'Pending' && (
                        <>
                            {cancelError && <div className="error" role="alert">{cancelError}</div>}
                            <div className="actions">
                                <button type="button" className="button button-danger" disabled={cancelling} onClick={() => void cancel()}>
                                    {cancelling ? 'Cancelling…' : 'Cancel order'}
                                </button>
                                <button type="button" className="button button-outline" onClick={() => navigate(`/orders/${data.id}/pay`)}>Pay now</button>
                            </div>
                        </>
                    )}

                    {vendorViewer && (
                        <>
                            {vendorNext === undefined && <p className="muted">This order is {data.status.toLowerCase()} — no action available.</p>}
                            {vendorNext !== undefined && (
                                <>
                                    <p className="muted">Update the fulfilment status for the items you sell.</p>
                                    {statusUpdate.error && <div className="error" role="alert">{statusUpdate.error}</div>}
                                    <button type="button" className="button" disabled={statusUpdate.loading} onClick={() => void advance()}>
                                        {statusUpdate.loading ? 'Saving…' : nextLabels[data.status]}
                                    </button>
                                </>
                            )}
                        </>
                    )}

                    {adminViewer && (
                        <p className="muted">You are viewing this order as an administrator. <Link to="/admin">Open the admin console</Link>.</p>
                    )}
                </div>
            </div>
        </div>
    );
}
