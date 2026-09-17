import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { useAuth } from '../auth';
import { EmptyState, ErrorNotice, IconChevronRight, Spinner, StatusBadge, money, useLoad } from '../ui';

export function OrdersPage() {
    const { user } = useAuth();
    const [statusFilter, setStatusFilter] = useState('');
    const { data, loading, error, reload } = useLoad(
        () => {
            if (user?.roles.includes('Admin')) return api.adminOrders();
            if (user?.roles.includes('Vendor')) return api.vendorOrders();
            return api.orders();
        },
        [user?.email],
    );

    if (loading) return <div className="page"><Spinner label="Loading orders…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} onRetry={reload} /></div>;

    const rows = (data?.items ?? []).filter(order => !statusFilter || order.status === statusFilter);

    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Orders</p>
                    <h1>{user?.roles.includes('Vendor') ? 'Incoming orders' : 'Your orders'}</h1>
                </div>
                <select aria-label="Filter by status" value={statusFilter} onChange={event => setStatusFilter(event.target.value)}>
                    <option value="">All statuses</option>
                    {['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'].map(status => (
                        <option key={status} value={status}>{status}</option>
                    ))}
                </select>
            </header>

            {rows.length === 0 ? (
                <EmptyState title="No orders found" hint="Orders will appear here once placed." />
            ) : (
                <div className="list">
                    {rows.map(order => (
                        <Link key={order.id} to={`/orders/${order.id}`} className="row-card">
                            <div className="row-main">
                                <strong>#{order.id.slice(0, 8).toUpperCase()}</strong>
                                <span className="muted">{new Date(order.createdAt).toLocaleDateString()}</span>
                                {order.customerId && <span className="muted">Customer {order.customerId.slice(0, 8)}</span>}
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