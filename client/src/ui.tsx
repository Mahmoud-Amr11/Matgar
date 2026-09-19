import { useEffect, useRef, useState, type ReactNode, type SVGProps } from 'react';
import type { OrderStatus } from './types';

type IconProps = { size?: number } & SVGProps<SVGSVGElement>;

function Svg({ size = 18, children, ...rest }: IconProps) {
    return (
        <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" {...rest}>
            {children}
        </svg>
    );
}

export const IconBag = (props: IconProps) => <Svg {...props}><path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4Z" /><path d="M3 6h18" /><path d="M16 10a4 4 0 0 1-8 0" /></Svg>;
export const IconBell = (props: IconProps) => <Svg {...props}><path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9" /><path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" /></Svg>;
export const IconUser = (props: IconProps) => <Svg {...props}><path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" /></Svg>;
export const IconLogOut = (props: IconProps) => <Svg {...props}><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" /><polyline points="16 17 21 12 16 7" /><line x1="21" y1="12" x2="9" y2="12" /></Svg>;
export const IconMenu = (props: IconProps) => <Svg {...props}><line x1="4" y1="6" x2="20" y2="6" /><line x1="4" y1="12" x2="20" y2="12" /><line x1="4" y1="18" x2="20" y2="18" /></Svg>;
export const IconX = (props: IconProps) => <Svg {...props}><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></Svg>;
export const IconSearch = (props: IconProps) => <Svg {...props}><circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" /></Svg>;
export const IconStar = (props: IconProps) => <Svg fill="currentColor" stroke="none" {...props}><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" /></Svg>;
export const IconChevronLeft = (props: IconProps) => <Svg {...props}><polyline points="15 18 9 12 15 6" /></Svg>;
export const IconChevronRight = (props: IconProps) => <Svg {...props}><polyline points="9 18 15 12 9 6" /></Svg>;
export const IconPlus = (props: IconProps) => <Svg {...props}><line x1="12" y1="5" x2="12" y2="19" /><line x1="5" y1="12" x2="19" y2="12" /></Svg>;
export const IconMinus = (props: IconProps) => <Svg {...props}><line x1="5" y1="12" x2="19" y2="12" /></Svg>;
export const IconRefresh = (props: IconProps) => <Svg {...props}><polyline points="23 4 23 10 17 10" /><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10" /></Svg>;
export const IconPencil = (props: IconProps) => <Svg {...props}><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z" /></Svg>;
export const IconCheck = (props: IconProps) => <Svg {...props}><polyline points="20 6 9 17 4 12" /></Svg>;
export const IconTrash = (props: IconProps) => <Svg {...props}><polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" /></Svg>;
export const IconMapPin = (props: IconProps) => <Svg {...props}><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z" /><circle cx="12" cy="10" r="3" /></Svg>;
export const IconPackage = (props: IconProps) => <Svg {...props}><path d="M16.5 9.4 7.55 4.24" /><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16Z" /><polyline points="3.27 6.96 12 12.07 20.73 6.96" /><line x1="12" y1="22.08" x2="12" y2="12" /></Svg>;
export const IconStore = (props: IconProps) => <Svg {...props}><path d="m2 7 4.41-4.41A2 2 0 0 1 7.83 2h8.34a2 2 0 0 1 1.42.59L22 7" /><path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8" /><path d="M15 22v-4a2 2 0 0 0-2-2h-2a2 2 0 0 0-2 2v4" /><path d="M2 7h20" /><path d="M22 7v3a2 2 0 0 1-2 2 2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 16 12a2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 12 12a2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 8 12a2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 4 12a2 2 0 0 1-2-2V7" /></Svg>;
export const IconShield = (props: IconProps) => <Svg {...props}><path d="M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1Z" /></Svg>;
export const IconCreditCard = (props: IconProps) => <Svg {...props}><rect width="20" height="14" x="2" y="5" rx="2" /><line x1="2" y1="10" x2="22" y2="10" /></Svg>;
export const IconClipboard = (props: IconProps) => <Svg {...props}><rect width="8" height="4" x="8" y="2" rx="1" ry="1" /><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" /></Svg>;
export const IconSend = (props: IconProps) => <Svg {...props}><line x1="22" y1="2" x2="11" y2="13" /><polygon points="22 2 15 22 11 13 2 9 22 2" /></Svg>;
export const IconEye = (props: IconProps) => <Svg {...props}><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" /><circle cx="12" cy="12" r="3" /></Svg>;
export const IconDotsVertical = (props: IconProps) => <Svg {...props}><circle cx="12" cy="12" r="1" /><circle cx="12" cy="5" r="1" /><circle cx="12" cy="19" r="1" /></Svg>;
export const IconHome = (props: IconProps) => <Svg {...props}><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2Z" /><polyline points="9 22 9 12 15 12 15 22" /></Svg>;

// ---- format helpers ----
export function money(value: number): string {
    return `$${value.toFixed(2)}`;
}

export function date(iso: string): string {
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return iso;
    return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

export function productStatusLabel(status: number): string {
    return ['Draft', 'Pending review', 'Active', 'Suspended'][status] ?? 'Unknown';
}

export function orderStatusLabel(status: OrderStatus | string): string {
    return status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
}

export function percent(value: number): string {
    return `${value}%`;
}

// ---- hooks ----
export function useLoad<T>(load: () => Promise<T>, deps: unknown[]) {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [nonce, setNonce] = useState(0);
    useEffect(() => {
        let cancelled = false;
        setLoading(true);
        setError(null);
        load()
            .then(result => { if (!cancelled) { setData(result); setLoading(false); } })
            .catch(err => { if (!cancelled) { setError(err instanceof Error ? err.message : 'Something went wrong.'); setLoading(false); } });
        return () => { cancelled = true; };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [...deps, nonce]);
    const reload = () => setNonce(n => n + 1);
    return { data, loading, error, reload };
}

export function useMutation<T extends unknown[]>(action: (...args: T) => Promise<unknown>) {
    const actionRef = useRef(action);
    useEffect(() => { actionRef.current = action; });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [done, setDone] = useState(false);
    const run = async (...args: T): Promise<boolean> => {
        setLoading(true);
        setError(null);
        setDone(false);
        try {
            await actionRef.current(...args);
            setDone(true);
            return true;
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Something went wrong.');
            return false;
        } finally {
            setLoading(false);
        }
    };
    return { run, loading, error, done };
}

// ---- components ----
export function Spinner({ label = 'Loading…' }: { label?: string }) {
    return (
        <div className="notice" role="status">
            <IconRefresh size={16} className="spin" />
            <span>{label}</span>
        </div>
    );
}

export function ErrorNotice({ message, onRetry }: { message: string; onRetry?: () => void }) {
    return (
        <div className="error" role="alert">
            <span>{message}</span>
            {onRetry && (
                <button type="button" className="link-button" onClick={onRetry}>Try again</button>
            )}
        </div>
    );
}

export function EmptyState({ title, hint }: { title: string; hint?: string }) {
    return (
        <div className="empty">
            <h3>{title}</h3>
            {hint && <p className="muted">{hint}</p>}
        </div>
    );
}

export function Pager({ page, totalPages, onChange }: { page: number; totalPages: number; onChange: (page: number) => void }) {
    if (totalPages <= 1) return null;
    return (
        <div className="pager">
            <button type="button" className="pager-button" disabled={page <= 1} onClick={() => onChange(page - 1)} aria-label="Previous page">
                <IconChevronLeft size={16} />
            </button>
            <span className="muted">Page {page} of {totalPages}</span>
            <button type="button" className="pager-button" disabled={page >= totalPages} onClick={() => onChange(page + 1)} aria-label="Next page">
                <IconChevronRight size={16} />
            </button>
        </div>
    );
}

export function StatusBadge({ status }: { status: string }) {
    const key = status.toLowerCase().replace(' ', '-');
    return <span className={`badge badge-${key}`}>{status}</span>;
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
    return (
        <label className="field">
            <span className="field-label">{label}</span>
            {children}
        </label>
    );
}

export function StarRating({ value, size = 14 }: { value: number; size?: number }) {
    return (
        <span className="stars" aria-label={`${value} out of 5 stars`}>
            {[1, 2, 3, 4, 5].map(star => (
                <IconStar key={star} size={size} style={{ color: star <= Math.round(value) ? 'var(--star)' : 'var(--border)' }} />
            ))}
        </span>
    );
}

export function AttributesBlock({ json }: { json: string }) {
    if (!json || json === '{}') return null;
    let parsed: Record<string, unknown> | null = null;
    try { parsed = JSON.parse(json) as Record<string, unknown>; } catch { /* fall through */ }
    if (!parsed) return <pre className="attributes">{json}</pre>;
    return (
        <div className="attributes">
            {Object.entries(parsed).map(([key, value]) => (
                <span key={key}><strong>{key}:</strong> {String(value)}</span>
            ))}
        </div>
    );
}