import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { Role, User } from './types';
import { api, tokenStore } from './api';

type AuthContextValue = {
    user: User | null;
    loading: boolean;
    login: (email: string, password: string) => Promise<void>;
    register: (payload: { firstName: string; lastName: string; email: string; password: string; confirmPassword: string }) => Promise<void>;
    logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

function base64UrlDecode(input: string): string {
    const pad = input.length % 4 === 0 ? '' : '='.repeat(4 - (input.length % 4));
    return atob(input.replace(/-/g, '+').replace(/_/g, '/') + pad);
}

export function userFromAccessToken(token: string | null): User | null {
    if (!token) return null;
    try {
        const segment = token.split('.')[1];
        if (!segment) return null;
        const payload = JSON.parse(base64UrlDecode(segment)) as Record<string, unknown>;
        const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        let roles: Role[] = [];
        if (Array.isArray(roleClaim)) roles = roleClaim as Role[];
        else if (typeof roleClaim === 'string' && roleClaim) roles = [roleClaim as Role];
        const sub = typeof payload.sub === 'string' ? payload.sub : undefined;
        const email = typeof payload.email === 'string' ? payload.email : '';
        if (!email) return null;
        return { id: sub, email, roles };
    } catch {
        return null;
    }
}

function loadCachedUser(): User | null {
    return userFromAccessToken(tokenStore.get());
}

export function isCustomer(user: User | null): boolean {
    return user?.roles.includes('Customer') ?? false;
}

export function isVendor(user: User | null): boolean {
    return user?.roles.includes('Vendor') ?? false;
}

export function isAdmin(user: User | null): boolean {
    return user?.roles.includes('Admin') ?? false;
}

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<User | null>(() => loadCachedUser());
    const [loading] = useState(false);

    useEffect(() => {
        const listener = () => setUser(loadCachedUser());
        window.addEventListener('storage', listener);
        const unauthorized = () => setUser(null);
        window.addEventListener('matgar:unauthorized', unauthorized);
        return () => {
            window.removeEventListener('storage', listener);
            window.removeEventListener('matgar:unauthorized', unauthorized);
        };
    }, []);

    const value = useMemo<AuthContextValue>(() => ({
        user,
        loading,
        login: async (email: string, password: string) => {
            const result = await api.login(email, password);
            tokenStore.set(result.accessToken);
            setUser(userFromAccessToken(result.accessToken));
        },
        register: async payload => {
            await api.register(payload);
        },
        logout: async () => {
            try { await api.logout(); } catch { /* ignore */ }
            tokenStore.clear();
            tokenStore.setDev(null);
            setUser(null);
        },
    }), [user, loading]);

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) throw new Error('useAuth must be used inside AuthProvider');
    return context;
}