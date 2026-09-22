import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ApiError, api } from '../api';
import type { PaymentStart } from '../types';
import { EmptyState, ErrorNotice, IconChevronLeft, IconCreditCard, Spinner, money, useLoad } from '../ui';

export function PaymentPage() {
    const { id = '' } = useParams();
    const { data: order, loading, error } = useLoad(() => api.order(id), [id]);
    const [payment, setPayment] = useState<PaymentStart | null>(null);
    const [busy, setBusy] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);
    const [paid, setPaid] = useState(false);

    if (loading) return <div className="page"><Spinner label="Loading payment…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} /></div>;
    if (!order) return <div className="page"><EmptyState title="Order not found" /></div>;

    const startPayment = async () => {
        setBusy(true);
        setActionError(null);
        try {
            const start = await api.createPayment(order.id);
            setPayment(start);
        } catch (err) {
            const message = err instanceof ApiError ? err.message : 'Could not start payment.';
            if (message === 'Order already paid.') {
                setPaid(true);
            } else {
                setActionError(message);
            }
        } finally {
            setBusy(false);
        }
    };

    return (
        <div className="page narrow">
            <Link to={`/orders/${order.id}`} className="back-link"><IconChevronLeft size={14} /> Back to order</Link>

            {paid ? (
                <div className="card center success">
                    <h1>Payment received</h1>
                    <p className="muted">Your payment for order #{order.id.slice(0, 8).toUpperCase()} was successful.</p>
                    <Link to={`/orders/${order.id}`} className="button">View order</Link>
                </div>
            ) : (
                <div className="card">
                    <div className="payment-box">
                        <IconCreditCard size={22} />
                        <h3>Complete your payment</h3>
                        <p className="muted">Order #{order.id.slice(0, 8).toUpperCase()} · Amount {money(order.totalAmount)}</p>

                        {actionError && <div className="error" role="alert">{actionError}</div>}

                        {payment ? (
                            <>
                                <a href={payment.checkoutUrl} target="_blank" rel="noreferrer" className="button">Continue to payment</a>
                                <p className="muted">You'll be taken to Paymob's secure checkout. Your order updates automatically once the payment is confirmed.</p>
                            </>
                        ) : (
                            <button type="button" className="button" disabled={busy} onClick={() => void startPayment()}>
                                {busy ? 'Starting…' : 'Start payment'}
                            </button>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}