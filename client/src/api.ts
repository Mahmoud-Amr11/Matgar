import type {
    AddCartItemResponse,
    Address,
    AuthResponse,
    CartResponse,
    Category,
    Coupon,
    CouponValidation,
    Notification,
    OrderDetail,
    OrderSummary,
    PagedResult,
    PaymentStart,
    PaymentStatus,
    ProductDetails,
    ProductListItem,
    ProductReview,
    ProductVariant,
} from './types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';
const API_PREFIX = '/api/v1';
const REFRESH_PATH = import.meta.env.VITE_AUTH_REFRESH_PATH ?? `${API_PREFIX}/auth/refresh-token`;
const DEV = import.meta.env.VITE_DEV_MODE === 'true';

const storage = typeof localStorage !== 'undefined' && typeof localStorage.getItem === 'function' ? localStorage : null;

let accessToken: string | null = storage?.getItem('matgar.accessToken') ?? null;
let devToken: string | null = storage?.getItem('matgar.devToken') ?? null;
let refreshPromise: Promise<boolean> | null = null;

export const tokenStore = {
    get: () => devToken ?? accessToken,
    set: (token: string) => {
        accessToken = token;
        if (storage) storage.setItem('matgar.accessToken', token);
    },
    setDev: (token: string | null) => {
        devToken = token;
        if (storage) {
            if (token) storage.setItem('matgar.devToken', token);
            else storage.removeItem('matgar.devToken');
        }
    },
    clear: () => {
        accessToken = null;
        if (storage) storage.removeItem('matgar.accessToken');
    },
};

export const isDevMode = () => DEV;

export class ApiError extends Error {
    status: number;
    constructor(message: string, status: number) {
        super(message);
        this.name = 'ApiError';
        this.status = status;
    }
}

interface ProblemDetail {
    status?: number;
    title?: string;
    detail?: string;
    errors?: Array<{ code?: string; message?: string }>;
    message?: string;
}

function problemMessage(payload: unknown): { message: string; status: number } {
    if (!payload || typeof payload !== 'object') return { message: 'Something went wrong.', status: 0 };
    const value = payload as ProblemDetail;
    if (Array.isArray(value.errors) && value.errors.length && value.errors[0]?.message) {
        return { message: value.errors[0].message, status: value.status ?? 0 };
    }
    if (typeof value.detail === 'string' && value.detail) return { message: value.detail, status: value.status ?? 0 };
    if (typeof value.title === 'string' && value.title) return { message: value.title, status: value.status ?? 0 };
    if (typeof value.message === 'string' && value.message) return { message: value.message, status: value.status ?? 0 };
    return { message: 'Something went wrong.', status: value.status ?? 0 };
}

async function request<T>(path: string, init: RequestInit = {}, retry = true): Promise<T> {
    const headers = new Headers(init.headers);
    headers.set('Content-Type', 'application/json');
    const token = tokenStore.get();
    if (token) headers.set('Authorization', `Bearer ${token}`);
    const response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers, credentials: 'include', cache: 'no-store' });
    if (response.status === 401 && retry && !path.includes(REFRESH_PATH)) {
        const refreshed = await refresh();
        if (refreshed) return request<T>(path, init, false);
        if (!path.includes('/auth/')) {
            tokenStore.clear();
            window.dispatchEvent(new CustomEvent('matgar:unauthorized'));
        }
    }
    if (response.status === 204) return null as T;
    const body = await response.json().catch(() => null);
    if (!response.ok) {
        const { message, status } = problemMessage(body);
        throw new ApiError(message, status || response.status);
    }
    return body as T;
}

export async function refresh(): Promise<boolean> {
    if (!refreshPromise) {
        refreshPromise = fetch(`${API_BASE_URL}${REFRESH_PATH}`, {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
        })
            .then(async response => {
                if (!response.ok) return false;
                const result = (await response.json().catch(() => null)) as AuthResponse | null;
                if (result?.accessToken) tokenStore.set(result.accessToken);
                return Boolean(result?.accessToken);
            })
            .catch(() => false)
            .finally(() => { refreshPromise = null; });
    }
    return refreshPromise;
}

export const api = {
    // ---- Auth ----
    register: (payload: { firstName: string; lastName: string; email: string; password: string; confirmPassword: string }) =>
        request<void>(`${API_PREFIX}/auth/register`, { method: 'POST', body: JSON.stringify(payload) }),
    login: (email: string, password: string) =>
        request<AuthResponse>(`${API_PREFIX}/auth/login`, { method: 'POST', body: JSON.stringify({ email, password }) }),
    confirmEmail: (userId: string, token: string) =>
        request<void>(`${API_PREFIX}/auth/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`, { method: 'POST' }),
    changePassword: (payload: { currentPassword: string; newPassword: string; confirmPassword: string }) =>
        request<void>(`${API_PREFIX}/auth/change-password`, { method: 'POST', body: JSON.stringify(payload) }),
    logout: () => request<void>(`${API_PREFIX}/auth/logout`, { method: 'POST' }),
    revokeToken: (token?: string) =>
        request<void>(`${API_PREFIX}/auth/revoke-token`, { method: 'POST', body: token ? JSON.stringify(token) : 'null' }),

    // ---- Products ----
    products: (params: { search?: string; categoryId?: string; minPrice?: number; maxPrice?: number; status?: number; page?: number; pageSize?: number } = {}) => {
        const query = new URLSearchParams();
        query.set('page', String(params.page ?? 1));
        query.set('pageSize', String(params.pageSize ?? 20));
        if (params.search) query.set('search', params.search);
        if (params.categoryId) query.set('categoryId', params.categoryId);
        if (params.minPrice !== undefined) query.set('minPrice', String(params.minPrice));
        if (params.maxPrice !== undefined) query.set('maxPrice', String(params.maxPrice));
        if (params.status !== undefined) query.set('status', String(params.status));
        return request<PagedResult<ProductListItem>>(`${API_PREFIX}/products?${query.toString()}`);
    },
    product: (id: string) => request<ProductDetails>(`${API_PREFIX}/products/${id}`),
    createProduct: (payload: { name: string; description: string; categoryId: string }) =>
        request<{ id: string }>(`${API_PREFIX}/products`, { method: 'POST', body: JSON.stringify(payload) }),
    updateProduct: (id: string, payload: { name: string; description: string; categoryId: string }) =>
        request<void>(`${API_PREFIX}/products/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
    deleteProduct: (id: string) => request<void>(`${API_PREFIX}/products/${id}`, { method: 'DELETE' }),
    submitProduct: (id: string) => request<void>(`${API_PREFIX}/products/${id}/submit-for-review`, { method: 'POST' }),
    approveProduct: (id: string) => request<void>(`${API_PREFIX}/products/${id}/approve`, { method: 'POST' }),
    suspendProduct: (id: string, reason: string) =>
        request<void>(`${API_PREFIX}/products/${id}/suspend`, { method: 'POST', body: JSON.stringify({ reason }) }),

    // ---- Variants ----
    variants: (productId: string) => request<ProductVariant[]>(`${API_PREFIX}/products/${productId}/variants`),
    createVariant: (productId: string, payload: { sku: string; price: number; imageUrl?: string | null; attributesJson: string; initialQuantity: number }) =>
        request<{ variantId: string }>(`${API_PREFIX}/products/${productId}/variants`, { method: 'POST', body: JSON.stringify(payload) }),
    updateVariant: (productId: string, variantId: string, payload: { price: number; imageUrl?: string | null; attributesJson: string }) =>
        request<void>(`${API_PREFIX}/products/${productId}/variants/${variantId}`, { method: 'PUT', body: JSON.stringify(payload) }),
    deleteVariant: (productId: string, variantId: string) =>
        request<void>(`${API_PREFIX}/products/${productId}/variants/${variantId}`, { method: 'DELETE' }),

    // ---- Reviews ----
    reviews: (productId: string, page = 1, pageSize = 20) =>
        request<PagedResult<ProductReview>>(`${API_PREFIX}/products/${productId}/reviews?page=${page}&pageSize=${pageSize}`),
    createReview: (productId: string, rating: number, comment?: string) =>
        request<{ reviewId: string }>(`${API_PREFIX}/products/${productId}/reviews`, { method: 'POST', body: JSON.stringify({ rating, comment }) }),

    // ---- Categories ----
    categories: (params: { search?: string; page?: number; pageSize?: number } = {}) => {
        const query = new URLSearchParams();
        query.set('page', String(params.page ?? 1));
        query.set('pageSize', String(params.pageSize ?? 10));
        if (params.search) query.set('search', params.search);
        return request<PagedResult<Category>>(`${API_PREFIX}/categories?${query.toString()}`);
    },
    category: (id: string) => request<Category>(`${API_PREFIX}/categories/${id}`),
    createCategory: (categoryName: string) =>
        request<void>(`${API_PREFIX}/categories`, { method: 'POST', body: JSON.stringify({ categoryName }) }),
    updateCategory: (id: string, categoryName: string) =>
        request<void>(`${API_PREFIX}/categories/${id}`, { method: 'PUT', body: JSON.stringify({ categoryName }) }),
    deleteCategory: (id: string) => request<void>(`${API_PREFIX}/categories/${id}`, { method: 'DELETE' }),

    // ---- Carts ----
    cart: () => request<CartResponse>(`${API_PREFIX}/carts`),
    addCartItem: (productVariantId: string, quantity: number) =>
        request<AddCartItemResponse>(`${API_PREFIX}/carts/items`, { method: 'POST', body: JSON.stringify({ productVariantId, quantity }) }),
    updateCartItem: (itemId: string, quantity: number) =>
        request<void>(`${API_PREFIX}/carts/items/${itemId}`, { method: 'PUT', body: JSON.stringify({ quantity }) }),
    removeCartItem: (itemId: string) => request<void>(`${API_PREFIX}/carts/items/${itemId}`, { method: 'DELETE' }),
    clearCart: () => request<void>(`${API_PREFIX}/carts`, { method: 'DELETE' }),

    // ---- Addresses ----
    addresses: () => request<Address[]>(`${API_PREFIX}/addresses`),
    createAddress: (payload: { fullAddress: string; city: string; governorate: string; phoneNumber: string; isDefault: boolean }) =>
        request<{ addressId: string }>(`${API_PREFIX}/addresses`, { method: 'POST', body: JSON.stringify(payload) }),
    updateAddress: (id: string, payload: { fullAddress: string; city: string; governorate: string; phoneNumber: string }) =>
        request<void>(`${API_PREFIX}/addresses/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
    deleteAddress: (id: string) => request<void>(`${API_PREFIX}/addresses/${id}`, { method: 'DELETE' }),
    setDefaultAddress: (id: string) => request<void>(`${API_PREFIX}/addresses/${id}/set-default`, { method: 'PUT' }),

    // ---- Orders ----
    checkout: async (payload: { addressId: string; couponCode?: string | null }) => {
        const id = await request<string>(`${API_PREFIX}/orders/checkout`, { method: 'POST', body: JSON.stringify(payload) });
        return { id };
    },
    order: (id: string) => request<OrderDetail>(`${API_PREFIX}/orders/${id}`),
    orders: () => request<PagedResult<OrderSummary>>(`${API_PREFIX}/orders?page=1&pageSize=20`),
    cancelOrder: (id: string) => request<void>(`${API_PREFIX}/orders/${id}/cancel`, { method: 'POST' }),
    vendorOrders: () => request<PagedResult<OrderSummary>>(`${API_PREFIX}/vendor/VendorOrders?page=1&pageSize=20`),
    updateVendorOrderStatus: (orderId: string, newStatus: number) =>
        request<void>(`${API_PREFIX}/vendor/VendorOrders/${orderId}/status`, { method: 'PUT', body: JSON.stringify({ orderId, newStatus }) }),
    adminOrders: () => request<PagedResult<OrderSummary>>(`${API_PREFIX}/admin/AdminOrders?page=1&pageSize=20`),

    // ---- Coupons ----
    coupons: () => request<Coupon[]>(`${API_PREFIX}/coupons`),
    createCoupon: (payload: { code: string; discountType: number; discountValue: number; minOrderAmount?: number | null; maxUsageCount: number; expiryDate: string }) =>
        request<{ couponId: string }>(`${API_PREFIX}/coupons`, { method: 'POST', body: JSON.stringify(payload) }),
    validateCoupon: (code: string, orderTotal: number) =>
        request<CouponValidation>(`${API_PREFIX}/coupons/validate`, { method: 'POST', body: JSON.stringify({ code, orderTotal }) }),

    // ---- Payments (unversioned) ----
    initiatePayment: (payload: { orderId: string; amount: number; currency?: string }) =>
        request<PaymentStart>('/api/Payments/initiate', { method: 'POST', body: JSON.stringify(payload) }),
    paymentStatus: (orderId: string) => request<PaymentStatus>(`/api/Payments/${orderId}/status`),
    webhook: (payload: { transactionReference: string; status: string; gatewayPayload: string }) =>
        request<boolean>('/api/Payments/webhook', { method: 'POST', body: JSON.stringify(payload) }),

    // ---- Notifications (unversioned) ----
    notifications: () => request<Notification[]>('/api/Notifications'),
    markNotificationRead: (id: string) => request<boolean>(`/api/Notifications/${id}/mark-read`, { method: 'PUT' }),
};