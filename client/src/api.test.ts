import { beforeEach, afterEach, describe, expect, test, vi } from 'vitest';
import { api, refresh, tokenStore } from './api';

function jsonResponse(body: unknown, status = 200) {
    return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

const emptyPage = { items: [], page: 1, pageSize: 12, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false };

let fetchMock: ReturnType<typeof vi.fn>;

beforeEach(() => {
    tokenStore.clear();
    tokenStore.setDev(null);
    fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
    vi.unstubAllGlobals();
});

describe('api contract', () => {
    test('returns the raw body (no data wrapper) and builds product query params', async () => {
        fetchMock.mockResolvedValueOnce(jsonResponse(emptyPage));
        const result = await api.products({ page: 1, pageSize: 12, search: 'tea', categoryId: 'cat-1', status: 2 });
        expect(result.items).toEqual([]);
        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe('/api/v1/products?page=1&pageSize=12&search=tea&categoryId=cat-1&status=2');
        expect(init.credentials).toBe('include');
    });

    test('sends Bearer token when one is set', async () => {
        tokenStore.set('at-1');
        fetchMock.mockResolvedValueOnce(jsonResponse([]));
        await api.notifications();
        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe('/api/Notifications');
        expect(init.headers.get('Authorization')).toBe('Bearer at-1');
    });

    test('surface condition: uses unversioned routes and returns raw arrays', async () => {
        fetchMock.mockResolvedValueOnce(jsonResponse([{ id: 'n1', title: 'Hi', body: 'b', isRead: false, createdAt: '2026-01-01' }]));
        const notifications = await api.notifications();
        expect(notifications).toHaveLength(1);
        expect(fetchMock.mock.calls[0][0]).toBe('/api/Notifications');

        fetchMock.mockResolvedValueOnce(jsonResponse({ clientSecret: 'cs', transactionReference: 'trx' }));
        const payment = await api.initiatePayment({ orderId: 'o1', amount: 10.5 });
        expect(payment.transactionReference).toBe('trx');
        expect(fetchMock.mock.calls[1][0]).toBe('/api/Payments/initiate');
    });

    test('rejects with the errors[0].message of a ProblemDetails response', async () => {
        fetchMock.mockResolvedValueOnce(jsonResponse({ type: 'https://tools.ietf.org/html/rfc9110', title: 'Bad Request', status: 400, errors: [{ code: 'InvalidEmail', message: 'Email is invalid.' }] }, 400));
        await expect(api.login('bad', 'x')).rejects.toThrow('Email is invalid.');
    });

    test('falls back to detail then title when errors array is absent', async () => {
        fetchMock.mockResolvedValueOnce(jsonResponse({ title: 'Conflict', detail: 'Code already in use.', status: 409 }, 409));
        await expect(api.createCoupon({ code: 'TAKE10', discountType: 0, discountValue: 10, maxUsageCount: 1, expiryDate: '2027-01-01' })).rejects.toThrow('Code already in use.');
    });

    test('retries a 401 once after a successful refresh and sets the new token', async () => {
        fetchMock
            .mockResolvedValueOnce(jsonResponse({ title: 'Unauthorized', status: 401 }, 401))
            .mockResolvedValueOnce(jsonResponse({ userId: 'u1', email: 'a@b.c', accessToken: 'at-2', refreshToken: 'rt', accessTokenExpiresAt: '2027-01-01' }))
            .mockResolvedValueOnce(jsonResponse(emptyPage))
            .mockResolvedValueOnce(jsonResponse({ title: 'Unauthorized', status: 401 }, 401));
        const result = await api.products({ page: 1, pageSize: 12 });
        expect(result.totalPages).toBe(0);
        expect(fetchMock).toHaveBeenCalledTimes(3);
        const refreshInit = fetchMock.mock.calls[1][1];
        expect(fetchMock.mock.calls[1][0]).toBe('/api/v1/auth/refresh-token');
        expect(refreshInit.credentials).toBe('include');
        const retryInit = fetchMock.mock.calls[2][1];
        expect(retryInit.headers.get('Authorization')).toBe('Bearer at-2');
        expect((await refresh())).toBe(false);
    });

    test('checkout unwraps the raw GUID string into { id }', async () => {
        fetchMock.mockResolvedValueOnce(jsonResponse('d4b7d5b1-9a2c-4b0e-9f3a-6d1c8e2a4f00'));
        expect(await api.checkout({ addressId: 'addr-1' })).toEqual({ id: 'd4b7d5b1-9a2c-4b0e-9f3a-6d1c8e2a4f00' });
        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe('/api/v1/orders/checkout');
        expect(JSON.parse(init.body)).toEqual({ addressId: 'addr-1' });
    });

    test('confirm-email sends userId and token as query string', async () => {
        fetchMock.mockResolvedValueOnce(new Response(null, { status: 204 }));
        await api.confirmEmail('u 1', 't/k');
        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe('/api/v1/auth/confirm-email?userId=u%201&token=t%2Fk');
        expect(init.method).toBe('POST');
    });

    test('204 responses resolve as null (logout)', async () => {
        fetchMock.mockResolvedValueOnce(new Response(null, { status: 204 }));
        expect(await api.logout()).toBeNull();
    });

    test('vendor order update posts integer status', async () => {
        fetchMock.mockResolvedValueOnce(new Response(null, { status: 204 }));
        await api.updateVendorOrderStatus('o1', 1);
        const [url, init] = fetchMock.mock.calls[0];
        expect(url).toBe('/api/v1/vendor/VendorOrders/o1/status');
        expect(JSON.parse(init.body)).toEqual({ orderId: 'o1', newStatus: 1 });
    });
});