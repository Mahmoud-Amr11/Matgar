import { api } from '../api';
import { EmptyState, ErrorNotice, IconBell, Spinner, date, useLoad } from '../ui';
import type { Notification } from '../types';

export function NotificationsPage() {
    const { data, loading, error, reload } = useLoad<Notification[]>(() => api.notifications(), []);

    const markRead = async (id: string) => {
        try {
            await api.markNotificationRead(id);
            reload();
        } catch { /* ignore */ }
    };

    if (loading) return <div className="page"><Spinner label="Loading notifications…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} onRetry={reload} /></div>;

    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Inbox</p>
                    <h1>Notifications</h1>
                </div>
            </header>

            {(data?.length ?? 0) === 0 ? (
                <EmptyState title="No notifications" hint="Updates about your orders will land here." />
            ) : (
                <div className="list">
                    {data?.map(item => (
                        <div className={`row-card ${item.isRead ? '' : 'unread'}`} key={item.id}>
                            <span className="notif-icon"><IconBell size={16} /></span>
                            <div className="row-main">
                                <strong>{item.title || 'Order update'}</strong>
                                <p>{item.body}</p>
                                <span className="muted">{date(item.createdAt)}</span>
                            </div>
                            {!item.isRead && (
                                <button type="button" className="button button-small" onClick={() => void markRead(item.id)}>Mark read</button>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}