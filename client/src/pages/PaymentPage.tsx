import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, isDevMode } from '../api';
import { EmptyState, ErrorNotice, IconChevronLeft, IconCreditCard, Spinner, money, useLoad } from '../ui';

export function PaymentPage() {
    const { id = '' } = useParams();
    const { data: order, loading, error } = useLoad(() => api.order(id), [id]);
    const [payment, setPayment] = useState<{ transactionReference: string; clientSecret: string } | null>(null);
    const [status, setStatus] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);

    useEffect(() => {
        if (!order) return;
        let cancelled = false;
        api.paymentStatus(order.id)
            .then(result => {
                if (cancelled) return;
                setStatus(result.status);
                setPayment({
                    transactionReference: result.transactionReference,
                    clientSecret: `https://payments.example/pay/${result.transactionReference}`,
                });
            })
            .catch(() => { if (!cancelled) setStatus(null); });
        return () => { cancelled = true; };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [order?.id]);

    useEffect(() => {
        if (!order || status !== 'Pending') return;
        let cancelled = false;
        const timer = window.setInterval(() => {
            void api.paymentStatus(order.id)
                .then(result => { if (!cancelled) setStatus(result.status); })
                .catch(() => { /* keep polling */ });
        }, 2000);
        return () => { cancelled = true; window.clearInterval(timer); };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [order?.id, status]);

    if (loading) return <div className="page"><Spinner label="Loading payment…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} /></div>;
    if (!order) return <div className="page"><EmptyState title="Order not found" /></div>;

    const startPayment = async () => {
        setBusy(true);
        setActionError(null);
        try {
            const start = await api.initiatePayment({ orderId: order.id, amount: order.totalAmount });
            setPayment(start);
            setStatus('Pending');
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Could not start payment.');
        } finally {
            setBusy(false);
        }
    };

    const simulatePaid = async () => {
        if (!payment) return;
        setBusy(true);
        setActionError(null);
        try {
            await api.webhook({ transactionReference: payment.transactionReference, status: 'success', gatewayPayload: 'simulated' });
            setStatus('Succeeded');
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Simulation failed.');
        } finally {
            setBusy(false);
        }
    };

    return (
        <div className="page narrow">
            <Link to={`/orders/${order.id}`} className="back-link"><IconChevronLeft size={14} /> Back to order</Link>

            {status === 'Succeeded' ? (
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

                        {status === 'Failed' && <div className="error" role="alert">Payment failed. Please try again.</div>}
                        {actionError && <div className="error" role="alert">{actionError}</div>}

                        {payment ? (
                            <>
                                <p className="muted">Transaction {payment.transactionReference}</p>
                                <a href={payment.clientSecret} target="_blank" rel="noreferrer" className="button">Open payment page</a>
                                {isDevMode() && (
                                    <button type="button" className="button button-outline" disabled={busy} onClick={() => void simulatePaid()}>
                                        {busy ? 'Processing…' : 'Simulate paid (dev)'}
                                    </button>
                                )}
                                {status === 'Pending' && <div className="notice"><Spinner label="Waiting for payment confirmation…" /></div>}
                                {status !== 'Pending' && status !== 'Failed' && (
                                    <button type="button" className="button button-outline" disabled={busy} onClick={() => void startPayment()}>Retry payment</button>
                                )}
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
