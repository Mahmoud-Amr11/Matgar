import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { CartProvider } from '../cart';
import { AuthProvider } from '../auth';
import { AppRoutes } from '../App';
import { beforeEach, expect, test, vi } from 'vitest';

function renderApp(path = '/') {
    return render(
        <MemoryRouter initialEntries={[path]}>
            <AuthProvider>
                <CartProvider>
                    <AppRoutes />
                </CartProvider>
            </AuthProvider>
        </MemoryRouter>
    );
}

const emptyPage = { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false };

beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify(emptyPage), { status: 200, headers: { 'Content-Type': 'application/json' } })));
});

test('renders the live catalog shell', async () => {
    renderApp();
    expect(await screen.findByText('The live catalog')).toBeInTheDocument();
    expect(screen.getByText('No products found')).toBeInTheDocument();
});

test('protects the cart for signed-out users', async () => {
    renderApp('/cart');
    expect(await screen.findByRole('tab', { name: 'Sign in' })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Create account' })).toBeInTheDocument();
});