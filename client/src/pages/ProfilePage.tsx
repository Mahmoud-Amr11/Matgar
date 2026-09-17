import { useState, type FormEvent } from 'react';
import { api, isDevMode, tokenStore } from '../api';
import { useAuth } from '../auth';
import { IconLogOut } from '../ui';

export function ProfilePage() {
    const { user, logout } = useAuth();
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [message, setMessage] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);
    const [devToken, setDevToken] = useState(tokenStore.get() ? '••••••••' : '');

    const changePassword = async (event: FormEvent) => {
        event.preventDefault();
        setMessage(null);
        setError(null);
        if (newPassword !== confirmPassword) { setError('New passwords do not match.'); return; }
        setBusy(true);
        try {
            await api.changePassword({ currentPassword, newPassword, confirmPassword });
            setMessage('Password updated.');
            setCurrentPassword('');
            setNewPassword('');
            setConfirmPassword('');
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Could not change password.');
        } finally {
            setBusy(false);
        }
    };

    const saveDevToken = () => {
        if (!devToken) { tokenStore.setDev(null); setDevToken(''); return; }
        if (devToken === '••••••••') return;
        tokenStore.setDev(devToken);
        setDevToken('••••••••');
    };

    const handleLogout = async () => {
        await logout();
    };

    return (
        <div className="page narrow">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Account</p>
                    <h1>Profile</h1>
                </div>
            </header>

            <div className="card">
                <h3>Account</h3>
                <div className="kv">
                    <span>Email</span><span>{user?.email}</span>
                    <span>Roles</span><span>{user?.roles.join(', ') || '—'}</span>
                </div>
                <button type="button" className="button button-outline" onClick={() => void handleLogout()}>
                    <IconLogOut size={16} /> Sign out
                </button>
            </div>

            <form className="card stack" onSubmit={changePassword}>
                <h3>Change password</h3>
                <label className="field">
                    <span className="field-label">Current password</span>
                    <input type="password" value={currentPassword} onChange={event => setCurrentPassword(event.target.value)} required autoComplete="current-password" />
                </label>
                <label className="field">
                    <span className="field-label">New password</span>
                    <input type="password" value={newPassword} onChange={event => setNewPassword(event.target.value)} required minLength={8} autoComplete="new-password" />
                </label>
                <label className="field">
                    <span className="field-label">Confirm new password</span>
                    <input type="password" value={confirmPassword} onChange={event => setConfirmPassword(event.target.value)} required minLength={8} autoComplete="new-password" />
                </label>
                {message && <div className="success">{message}</div>}
                {error && <div className="error" role="alert">{error}</div>}
                <button type="submit" className="button" disabled={busy}>{busy ? 'Saving…' : 'Update password'}</button>
            </form>

            {isDevMode() && (
                <div className="card stack">
                    <h3>Developer token</h3>
                    <p className="muted">Paste any JWT to impersonate its role in development.</p>
                    <label className="field">
                        <span className="field-label">Token</span>
                        <input value={devToken} onChange={event => setDevToken(event.target.value)} placeholder="eyJhbGciOi…" />
                    </label>
                    <button type="button" className="button button-outline" onClick={saveDevToken}>Apply token</button>
                </div>
            )}
        </div>
    );
}