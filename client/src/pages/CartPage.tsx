import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api, isDevMode } from '../api';
import { useCart } from '../cart';
import { useAuth } from '../auth';
import type { Address } from '../types';
import { EmptyState, IconCreditCard, IconMinus, IconPlus, IconTrash, Spinner, money, useLoad } from '../ui';
import { useEffect } from 'react';

export function CartPage() {
    const { user } = useAuth();
    const { items, count, subtotal, remove, update, clear, refresh } = useCart();
    const navigate = useNavigate();
    const [couponCode, setCouponCode] = useState('');
    const [discount, setDiscount] = useState(0);
    const [couponError, setCouponError] = useState<string | null>(null);
    const [addressId, setAddressId] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);
    const [orderId, setOrderId] = useState<string | null>(null);
    const [payment, setPayment] = useState<{ transactionReference: string; clientSecret: string } | null>(null);
    const [paymentStatus, setPaymentStatus] = useState<string | null>(null);

    const addresses = useLoad<Address[]>(() => (user ? api.addresses() : Promise.resolve([])), [user?.email]);

    useEffect(() => {
        if (addresses.data) {
            const current = addresses.data.find(a => a.addressId === addressId) ?? addresses.data[0];
            if (current) setAddressId(current.addressId);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [addresses.data]);

    useEffect(() => {
        if (user?.roles.includes('Customer') && orderId && paymentStatus !== 'Succeeded' && paymentStatus !== 'Failed') {
            let cancelled = false;
            const poll = async () => {
                try {
                    const status = await api.paymentStatus(orderId);
                    if (!cancelled) {
                        setPaymentStatus(status.status);
                        if (status.status === 'Succeeded') {
                            await clear();
                            setOrderId(orderId);
                        }
                    }
                } catch { /* keep polling */ }
            };
            const timer = window.setInterval(() => { void poll(); }, 2000);
            void poll();
            return () => { cancelled = true; window.clearInterval(timer); };
        }
        return undefined;
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [user, orderId]);

    const applyCoupon = async () => {
        if (!couponCode.trim()) return;
        setCouponError(null);
        setDiscount(0);
        try {
            const result = await api.validateCoupon(couponCode.trim(), subtotal);
            setDiscount(result.discountAmount);
        } catch (err) {
            setCouponError(err instanceof Error ? err.message : 'Invalid coupon.');
            setCouponCode('');
        }
    };

    const startCheckout = async () => {
        setError(null);
        setOrderId(null);
        setPayment(null);
        setPaymentStatus(null);
        if (!user) { navigate('/login', { state: { from: '/cart' } }); return; }
        if (!addressId) { setError('Please add and select a delivery address first.'); return; }
        setBusy(true);
        try {
            const created = await api.checkout({ addressId, couponCode: discount > 0 ? couponCode : null });
            setOrderId(created.id);
            const start = await api.initiatePayment({ orderId: created.id, amount: subtotal - discount });
            setPayment(start);
            setPaymentStatus('Pending');
            await refresh();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Checkout failed.');
        } finally {
            setBusy(false);
        }
    };

    const simulatePaid = async () => {
        if (!payment) return;
        try {
            await api.webhook({ transactionReference: payment.transactionReference, status: 'success', gatewayPayload: 'simulated' });
            setPaymentStatus('Succeeded');
            await clear();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Simulation failed.');
        }
    };

    if (!user) {
        return (
            <div className="page narrow">
                <div className="card center">
                    <h1>Your cart</h1>
                    <p className="muted">Sign in to see your cart and check out.</p>
                    <button type="button" className="button" onClick={() => navigate('/login', { state: { from: '/cart' } })}>Sign in to Matgar</button>
                </div>
            </div>
        );
    }

    if (paymentStatus === 'Succeeded') {
        return (
            <div className="page narrow">
                <div className="card center success">
                    <h1>Order complete</h1>
                    <p className="muted">Thank you! Your order has been placed successfully.</p>
                    <Link to={`/orders/${orderId}`} className="button">View order</Link>
                    <Link to="/" className="button button-outline">Keep shopping</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Checkout</p>
                    <h1>Your cart ({count})</h1>
                </div>
            </header>

            {items.length === 0 && !payment ? (
                <EmptyState title="Your cart is empty" hint="Browse the catalog and add something you love." />
            ) : (
                <div className="cart-layout">
                    <div className="cart-items">
                        {items.map(item => (
                            <div className="cart-item" key={item.productVariantId}>
                                {item.image ? <img src={item.image} alt="" /> : <span className="mini-thumb">{item.name.slice(0, 1)}</span>}
                                <div className="cart-item-info">
                                    <Link to={`/products/${item.productVariantId}`} className="cart-item-name">{item.name}</Link>
                                    <span className="muted sku">{item.sku}</span>
                                    {item.attributesJson && item.attributesJson !== '{}' && <span className="muted">{item.attributesJson}</span>}
                                </div>
                                <div className="stepper">
                                    <button type="button" aria-label={`Decrease quantity for ${item.name}`} disabled={item.quantity <= 1} onClick={() => void update(item.productVariantId, item.quantity - 1)}><IconMinus size={14} /></button>
                                    <span>{item.quantity}</span>
                                    <button type="button" aria-label={`Increase quantity for ${item.name}`} disabled={item.quantity >= 100} onClick={() => void update(item.productVariantId, item.quantity + 1)}><IconPlus size={14} /></button>
                                </div>
                                <div className="cart-item-total">{money(item.price * item.quantity)}</div>
                                <button type="button" className="icon-button danger" aria-label={`Remove ${item.name}`} onClick={() => void remove(item.productVariantId)}><IconTrash size={16} /></button>
                            </div>
                        ))}
                        {payment && paymentStatus !== 'Succeeded' && (
                            <div className="card">
                                <div className="payment-box">
                                    <IconCreditCard size={22} />
                                    <h3>Complete your payment</h3>
                                    {paymentStatus === 'Failed' && <div className="error" role="alert">Payment failed. Please try again.</div>}
                                    <p className="muted">Transaction {payment.transactionReference}</p>
                                    <a href={payment.clientSecret} target="_blank" rel="noreferrer" className="button">Open payment page</a>
                                    {isDevMode() && (
                                        <button type="button" className="button button-outline" onClick={() => void simulatePaid()}>Simulate paid (dev)</button>
                                    )}
                                    <div className="notice"><Spinner label="Waiting for payment confirmation…" /></div>
                                </div>
                            </div>
                        )}
                    </div>

                    {!payment && (
                        <aside className="summary card">
                            <h3>Order summary</h3>
                            <div className="summary-line"><span>Subtotal</span><span>{money(subtotal)}</span></div>
                            <div className="summary-line"><span>Discount</span><span>{discount > 0 ? `– ${money(discount)}` : '—'}</span></div>
                            <div className="summary-line total"><span>Total</span><span>{money(Math.max(subtotal - discount, 0))}</span></div>

                            <form className="coupon-form" onSubmit={event => { event.preventDefault(); void applyCoupon(); }}>
                                <input
                                    aria-label="Coupon code"
                                    placeholder="Coupon code"
                                    value={couponCode}
                                    onChange={event => setCouponCode(event.target.value)}
                                />
                                <button type="submit" className="button button-small button-outline">Apply</button>
                            </form>
                            {couponError && <span className="error">{couponError}</span>}

                            {user?.roles.includes('Customer') && (
                                <label className="field">
                                    <span className="field-label">Delivery address</span>
                                    <select value={addressId} onChange={event => setAddressId(event.target.value)}>
                                        {addresses.data?.map((address: Address) => (
                                            <option key={address.addressId} value={address.addressId}>
                                                {address.fullAddress} — {address.city}{address.isDefault ? ' (default)' : ''}
                                            </option>
                                        ))}
                                    </select>
                                </label>
                            )}
                            {addresses.data && addresses.data.length === 0 && (
                                <p className="muted">No saved addresses. <Link to="/addresses">Add one here</Link>.</p>
                            )}
                            {error && <div className="error" role="alert">{error}</div>}
                            {!user?.roles.includes('Customer') && (
                                <p className="muted">Meanwhile, admin and vendor accounts cannot place orders.</p>
                            )}
                            <button
                                type="button"
                                className="button checkout-button"
                                disabled={busy || !user?.roles.includes('Customer') || (addresses.data?.length === 0)}
                                onClick={() => void startCheckout()}
                            >
                                {busy ? 'Placing order…' : 'Place order'}
                            </button>
                        </aside>
                    )}
                </div>
            )}
        </div>
    );
}