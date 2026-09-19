import { useState, type ReactNode } from 'react';
import { BrowserRouter, Link, Navigate, NavLink, Outlet, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './auth';
import { CartProvider, useCart } from './cart';
import { IconBag, IconBell, IconClipboard, IconHome, IconLogOut, IconMapPin, IconMenu, IconShield, IconStore, IconUser, IconX } from './ui';
import { useLoad } from './ui';
import { api } from './api';
import { LoginPage } from './pages/LoginPage';
import { CatalogPage } from './pages/CatalogPage';
import { ProductPage } from './pages/ProductPage';
import { CartPage } from './pages/CartPage';
import { OrdersPage } from './pages/OrdersPage';
import { OrderDetailPage } from './pages/OrderDetailPage';
import { PaymentPage } from './pages/PaymentPage';
import { AddressesPage } from './pages/AddressesPage';
import { NotificationsPage } from './pages/NotificationsPage';
import { ProfilePage } from './pages/ProfilePage';
import { VendorPage } from './pages/VendorPage';
import { AdminPage } from './pages/AdminPage';
import { ConfirmEmailPage } from './pages/ConfirmEmailPage';

function NavBrand() {
    return (
        <Link to="/" className="brand">
            <span className="brand-mark"><IconBag size={18} /></span>
            <span className="brand-name">Matgar</span>
        </Link>
    );
}

function Shell() {
    const { user, logout } = useAuth();
    const { count } = useCart();
    const navigate = useNavigate();
    const [open, setOpen] = useState(false);

    const { data: notifications } = useLoad(
        () => (user ? api.notifications() : Promise.resolve([])),
        [user?.email],
    );
    const unread = (notifications ?? []).filter(item => !item.isRead).length;

    const links = [{ to: '/', icon: <IconHome size={16} />, label: 'Store' }];
    if (user) {
        links.push({ to: '/orders', icon: <IconClipboard size={16} />, label: 'Orders' });
        links.push({ to: '/addresses', icon: <IconMapPin size={16} />, label: 'Addresses' });
        links.push({ to: '/notifications', icon: <IconBell size={16} />, label: `Updates${unread ? ` (${unread})` : ''}` });
        links.push({ to: '/profile', icon: <IconUser size={16} />, label: 'Profile' });
        if (user.roles.includes('Vendor')) links.push({ to: '/vendor', icon: <IconStore size={16} />, label: 'Vendor' });
        if (user.roles.includes('Admin')) links.push({ to: '/admin', icon: <IconShield size={16} />, label: 'Admin' });
    }

    const close = () => setOpen(false);

    const signOut = async () => {
        close();
        await logout();
        navigate('/');
    };

    return (
        <div className="app">
            <header className="topbar">
                <div className="topbar-inner">
                    <NavBrand />
                    <nav className="nav" aria-label="Main navigation">
                        {links.map(link => (
                            <NavLink key={link.to} to={link.to} className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')} onClick={close}>
                                {link.icon}{link.label}
                            </NavLink>
                        ))}
                    </nav>
                    <div className="topbar-actions">
                        {user && (
                            <NavLink to="/cart" className={({ isActive }) => (isActive ? 'cart-button active' : 'cart-button')}>
                                <IconBag size={17} />
                                {count > 0 && <span className="cart-count">{count}</span>}
                            </NavLink>
                        )}
                        {user ? (
                            <button type="button" className="topbar-link" title="Sign out" onClick={() => void signOut()}>
                                <IconLogOut size={17} />
                            </button>
                        ) : (
                            <NavLink to="/login" className="topbar-link" style={{ marginRight: 0 }}>Sign in</NavLink>
                        )}
                        <button type="button" className="topbar-link menu-toggle" aria-label="Toggle menu" onClick={() => setOpen(v => !v)}>
                            {open ? <IconX size={18} /> : <IconMenu size={18} />}
                        </button>
                    </div>
                </div>
                {open && (
                    <nav className="mobile-nav" aria-label="Mobile navigation">
                        {links.map(link => (
                            <NavLink key={link.to} to={link.to} className="mobile-link" onClick={close}>
                                {link.icon}{link.label}
                            </NavLink>
                        ))}
                        {!user && <NavLink to="/login" className="mobile-link" onClick={close}><IconUser size={16} /> Sign in</NavLink>}
                    </nav>
                )}
            </header>

            <main className="content"><Outlet /></main>

            <footer className="footer">
                <div className="footer-inner">
                    <NavBrand />
                    <p className="muted">Small rituals, made richer. Built on Matgar API.</p>
                </div>
            </footer>
        </div>
    );
}

function RequireAuth({ children }: { children: ReactNode }) {
    const { user, loading } = useAuth();
    const location = useLocation();
    if (loading) return null;
    if (!user) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
    return <>{children}</>;
}

function RequireRole({ roles, children }: { roles: string[]; children: ReactNode }) {
    const { user } = useAuth();
    if (!user?.roles.some(role => roles.includes(role))) return <Navigate to="/" replace />;
    return <>{children}</>;
}

export function AppRoutes() {
    return (
        <Routes>
            <Route element={<Shell />}>
                <Route path="/" element={<CatalogPage />} />
                <Route path="/products/:id" element={<ProductPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/confirm-email" element={<ConfirmEmailPage />} />
                <Route path="/cart" element={<RequireAuth><CartPage /></RequireAuth>} />
                <Route path="/orders" element={<RequireAuth><OrdersPage /></RequireAuth>} />
                <Route path="/orders/:id" element={<RequireAuth><OrderDetailPage /></RequireAuth>} />
                <Route path="/orders/:id/pay" element={<RequireAuth><PaymentPage /></RequireAuth>} />
                <Route path="/addresses" element={<RequireAuth><AddressesPage /></RequireAuth>} />
                <Route path="/notifications" element={<RequireAuth><NotificationsPage /></RequireAuth>} />
                <Route path="/profile" element={<RequireAuth><ProfilePage /></RequireAuth>} />
                <Route path="/vendor" element={<RequireRole roles={['Vendor']}><VendorPage /></RequireRole>} />
                <Route path="/admin" element={<RequireRole roles={['Admin']}><AdminPage /></RequireRole>} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Route>
        </Routes>
    );
}

export default function App() {
    return (
        <BrowserRouter>
            <AuthProvider>
                <CartProvider>
                    <AppRoutes />
                </CartProvider>
            </AuthProvider>
        </BrowserRouter>
    );
}