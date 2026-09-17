import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { api } from '../api';
import { IconCheck, Spinner } from '../ui';

export function ConfirmEmailPage() {
    const [params] = useSearchParams();
    const userId = params.get('userId') ?? '';
    const token = params.get('token') ?? '';
    const [state, setState] = useState<'pending' | 'ok' | 'error'>('pending');
    const [message, setMessage] = useState('');

    useEffect(() => {
        let cancelled = false;
        const confirm = async () => {
            if (!userId || !token) {
                setState('error');
                setMessage('Missing confirmation parameters.');
                return;
            }
            try {
                await api.confirmEmail(userId, token);
                if (!cancelled) setState('ok');
            } catch (err) {
                if (!cancelled) {
                    setState('error');
                    setMessage(err instanceof Error ? err.message : 'Confirmation failed.');
                }
            }
        };
        setState('pending');
        void confirm();
        return () => { cancelled = true; };
    }, [userId, token]);

    return (
        <div className="page narrow">
            <div className="card center">
                {state === 'pending' && <><Spinner label="Confirming your email…" /></>}
                {state === 'ok' && (
                    <>
                        <span className="success-icon"><IconCheck size={20} /></span>
                        <h1>Email confirmed</h1>
                        <p className="muted">You can now sign in to your account.</p>
                        <Link to="/login" className="button">Sign in</Link>
                    </>
                )}
                {state === 'error' && (
                    <>
                        <h1>Confirmation failed</h1>
                        <p className="error">{message}</p>
                        <p className="muted">The link may be invalid or expired. Try registering again.</p>
                    </>
                )}
            </div>
        </div>
    );
}