import { useState, type FormEvent } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth';
import { IconBag, IconCheck } from '../ui';

export function LoginPage() {
    const { user, login, register } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const [mode, setMode] = useState<'login' | 'register'>('login');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);
    const [registered, setRegistered] = useState(false);

    const from = (location.state as { from?: string } | null)?.from ?? '/';

    if (user) {
        return (
            <div className="page narrow">
                <div className="card center">
                    <h1>You're signed in</h1>
                    <p className="muted">{user.email}</p>
                    <button type="button" className="button" onClick={() => navigate('/')}>Go to store</button>
                </div>
            </div>
        );
    }

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setError(null);
        if (mode === 'register' && password !== confirmPassword) {
            setError('Passwords do not match.');
            return;
        }
        setBusy(true);
        try {
            if (mode === 'login') {
                await login(email, password);
                navigate(from, { replace: true });
            } else {
                await register({ firstName, lastName, email, password, confirmPassword });
                setRegistered(true);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Something went wrong.');
        } finally {
            setBusy(false);
        }
    };

    if (registered) {
        return (
            <div className="page narrow">
                <div className="card center">
                    <span className="success-icon"><IconCheck size={20} /></span>
                    <h1>Check your inbox</h1>
                    <p className="muted">
                        We sent a confirmation link to <strong>{email}</strong>. Confirm your email, then sign in.
                    </p>
                    <button type="button" className="button button-outline" onClick={() => { setMode('login'); setRegistered(false); }}>
                        Back to sign in
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="page narrow">
            <div className="auth-card">
                <div className="auth-brand">
                    <span className="logo"><IconBag size={22} /></span>
                    <h1>Matgar</h1>
                    <p className="muted">Small rituals, made richer.</p>
                </div>
                <div className="tabs" role="tablist">
                    <button type="button" role="tab" aria-selected={mode === 'login'} className={mode === 'login' ? 'tab active' : 'tab'} onClick={() => { setMode('login'); setError(null); }}>
                        Sign in
                    </button>
                    <button type="button" role="tab" aria-selected={mode === 'register'} className={mode === 'register' ? 'tab active' : 'tab'} onClick={() => { setMode('register'); setError(null); }}>
                        Create account
                    </button>
                </div>
                <form onSubmit={submit} className="stack">
                    {mode === 'register' && (
                        <div className="field-row">
                            <label className="field">
                                <span className="field-label">First name</span>
                                <input value={firstName} onChange={event => setFirstName(event.target.value)} required minLength={3} maxLength={50} />
                            </label>
                            <label className="field">
                                <span className="field-label">Last name</span>
                                <input value={lastName} onChange={event => setLastName(event.target.value)} required minLength={3} maxLength={50} />
                            </label>
                        </div>
                    )}
                    <label className="field">
                        <span className="field-label">Email</span>
                        <input type="email" value={email} onChange={event => setEmail(event.target.value)} required />
                    </label>
                    <label className="field">
                        <span className="field-label">Password</span>
                        <input type="password" value={password} onChange={event => setPassword(event.target.value)} required minLength={8} autoComplete="current-password" />
                    </label>
                    {mode === 'register' && (
                        <label className="field">
                            <span className="field-label">Confirm password</span>
                            <input type="password" value={confirmPassword} onChange={event => setConfirmPassword(event.target.value)} required minLength={8} autoComplete="new-password" />
                        </label>
                    )}
                    {error && <div className="error" role="alert">{error}</div>}
                    <button type="submit" className="button" disabled={busy}>
                        {busy ? 'Please wait…' : mode === 'login' ? 'Sign in' : 'Create account'}
                    </button>
                </form>
            </div>
        </div>
    );
}