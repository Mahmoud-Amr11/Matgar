import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { CartItem } from './types';
import { api } from './api';
import { useAuth } from './auth';

type CartContextValue = {
    items: CartItem[];
    count: number;
    subtotal: number;
    refresh: () => Promise<void>;
    add: (item: CartItem) => Promise<void>;
    remove: (id: string) => Promise<void>;
    update: (id: string, quantity: number) => Promise<void>;
    clear: () => Promise<void>;
};

const CartContext = createContext<CartContextValue | null>(null);
const STORAGE_KEY = 'matgar.cart';

function readLocal(): CartItem[] {
    try {
        return JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '[]') as CartItem[];
    } catch {
        return [];
    }
}

export function CartProvider({ children }: { children: ReactNode }) {
    const { user } = useAuth();
    const [items, setItems] = useState<CartItem[]>([]);

    useEffect(() => {
        if (!user) {
            setItems(readLocal());
            return;
        }
        let cancelled = false;
        api.cart()
            .then(cart => {
                if (cancelled) return;
                setItems(cart.items.map(item => ({
                    itemId: item.itemId,
                    productVariantId: item.productVariantId,
                    sku: item.sku,
                    name: item.productName,
                    price: item.unitPrice,
                    quantity: item.quantity,
                    image: item.imageUrl ?? undefined,
                    attributesJson: item.attributesJson,
                })));
            })
            .catch(() => { if (!cancelled) setItems(readLocal()); });
        return () => { cancelled = true; };
    }, [user]);

    useEffect(() => {
        if (!user && typeof localStorage !== 'undefined' && typeof localStorage.setItem === 'function') localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    }, [user, items]);

    const value = useMemo<CartContextValue>(() => ({
        items,
        count: items.reduce((total, item) => total + item.quantity, 0),
        subtotal: items.reduce((total, item) => total + item.price * item.quantity, 0),
        refresh: async () => {
            if (!user) { setItems(readLocal()); return; }
            try {
                const cart = await api.cart();
                setItems(cart.items.map(item => ({
                    itemId: item.itemId,
                    productVariantId: item.productVariantId,
                    sku: item.sku,
                    name: item.productName,
                    price: item.unitPrice,
                    quantity: item.quantity,
                    image: item.imageUrl ?? undefined,
                    attributesJson: item.attributesJson,
                })));
            } catch { /* keep current */ }
        },
        add: async (item: CartItem) => {
            setItems(current => {
                const found = current.find(existing => existing.productVariantId === item.productVariantId);
                if (found) {
                    return current.map(existing => existing.productVariantId === item.productVariantId
                        ? { ...existing, quantity: existing.quantity + item.quantity, itemId: existing.itemId ?? item.itemId }
                        : existing);
                }
                return [...current, item];
            });
            if (user) {
                try {
                    const added = await api.addCartItem(item.productVariantId, item.quantity);
                    setItems(current => current.map(existing => existing.productVariantId === item.productVariantId
                        ? { ...existing, itemId: existing.itemId ?? added.cartItemId }
                        : existing));
                } catch { /* keep local optimistic */ }
            }
        },
        remove: async (id: string) => {
            const target = items.find(item => item.productVariantId === id);
            setItems(current => current.filter(item => item.productVariantId !== id));
            if (user && target?.itemId) {
                try { await api.removeCartItem(target.itemId); } catch { /* ignore */ }
            }
        },
        update: async (id: string, quantity: number) => {
            const target = items.find(item => item.productVariantId === id);
            setItems(current =>
                quantity > 0
                    ? current.map(item => item.productVariantId === id ? { ...item, quantity } : item)
                    : current.filter(item => item.productVariantId !== id));
            if (user && target?.itemId) {
                try {
                    if (quantity > 0) await api.updateCartItem(target.itemId, quantity);
                    else await api.removeCartItem(target.itemId);
                } catch { /* ignore */ }
            }
        },
        clear: async () => {
            setItems([]);
            if (user) { try { await api.clearCart(); } catch { /* ignore */ } }
        },
    }), [items, user]);

    return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
    const context = useContext(CartContext);
    if (!context) throw new Error('useCart must be used inside CartProvider');
    return context;
}