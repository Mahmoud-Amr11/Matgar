import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { isAdmin, isVendor, useAuth } from '../auth';
import { EmptyState, ErrorNotice, IconChevronRight, Spinner, StatusBadge, money, useLoad } from '../ui';

export function OrdersPage() {
    const { user } = useAuth();
    const [statusFilter, setStatusFilter] = useState('');
    const { data, loading, error, reload } = useLoad(() => api.orders(), [user?.email]);

    if (loading) return <div className="page"><Spinner label="Loading orders…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} onRetry={reload} /></div>;

    const rows = (data?.items ?? []).filter(order => !statusFilter || order.status === statusFilter);

    const workspaces: { to: string; label: string }[] = [];
    if (isVendor(user)) workspaces.push({ to: '/vendor', label: 'Vendor workspace' });
    if (isAdmin(user)) workspaces.push({ to: '/admin', label: 'Admin console' });

    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Orders</p>
                    <h1>Your orders</h1>
                    <p className="muted">Purchases you placed as a customer.</p>
                </div>
                <select aria-label="Filter by status" value={statusFilter} onChange={event => setStatusFilter(event.target.value)}>
                    <option value="">All statuses</option>
                    {['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'].map(status => (
                        <option key={status} value={status}>{status}</option>
                    ))}
                </select>
            </header>

            {workspaces.length > 0 && (
                <div className="notice">
                    <span>Selling or managing the store?</span>
                    {workspaces.map(workspace => (
                        <Link key={workspace.to} to={workspace.to} className="link-button">{workspace.label}</Link>
                    ))}
                </div>
            )}

            {rows.length === 0 ? (
                <EmptyState title="No orders found" hint="Orders will appear here once placed." />
            ) : (
                <div className="list">
                    {rows.map(order => (
                        <Link key={order.id} to={`/orders/${order.id}`} className="row-card">
                            <div className="row-main">
                                <strong>#{order.id.slice(0, 8).toUpperCase()}</strong>
                                <span className="muted">{new Date(order.createdAt).toLocaleDateString()}</span>
                            </div>
                            <div className="row-side">
                                <StatusBadge status={order.status} />
                                <span className="row-price">{money(order.totalAmount)}</span>
                                <IconChevronRight size={16} className="muted" />
                            </div>
                        </Link>
                    ))}
                </div>
            )}
        </div>
    );
}
