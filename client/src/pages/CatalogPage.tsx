import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import type { Category, ProductListItem } from '../types';
import { EmptyState, ErrorNotice, IconSearch, Pager, Spinner, money, productStatusLabel, useLoad } from '../ui';
import { useAuth } from '../auth';
import { useCart } from '../cart';

export function CatalogPage() {
    const { user } = useAuth();
    const { add } = useCart();
    const [search, setSearch] = useState('');
    const [categoryId, setCategoryId] = useState('');
    const [status, setStatus] = useState('');
    const [page, setPage] = useState(1);
    const [added, setAdded] = useState<string | null>(null);
    const [addError, setAddError] = useState<string | null>(null);

    const isStaff = user?.roles.includes('Vendor') || user?.roles.includes('Admin');

    const products = useLoad(
        () => api.products({
            search: search || undefined,
            categoryId: categoryId || undefined,
            status: status ? Number(status) : undefined,
            page,
            pageSize: 12,
        }),
        [search, categoryId, status, page],
    );

    const categories = useLoad(() => api.categories({ page: 1, pageSize: 100 }), []);

    const addToCart = async (product: ProductListItem) => {
        setAddError(null);
        setAdded(null);
        if (!user) return;
        const details = await api.product(product.productId);
        const variant = details.variants[0];
        if (!variant) { setAddError('This product has no variants yet.'); return; }
        if (variant.availableQuantity <= 0) { setAddError('This product is out of stock.'); return; }
        try {
            await add({ productVariantId: variant.variantId, sku: variant.sku, name: product.productName, price: variant.price, quantity: 1, image: variant.imageUrl, attributesJson: variant.attributesJson });
            setAdded(product.productId);
            window.setTimeout(() => setAdded(null), 1500);
        } catch (err) {
            setAddError(err instanceof Error ? err.message : 'Could not add to cart.');
        }
    };

    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">The live catalog</p>
                    <h1>Small rituals, made richer</h1>
                </div>
                {isStaff && (
                    <Link to={user?.roles.includes('Vendor') ? '/vendor' : '/admin'} className="button button-outline">Manage products</Link>
                )}
            </header>

            <div className="filters">
                <div className="search-box">
                    <IconSearch size={16} />
                    <input
                        aria-label="Search products"
                        placeholder="Search products…"
                        value={search}
                        onChange={event => { setSearch(event.target.value); setPage(1); }}
                    />
                </div>
                <select aria-label="Category" value={categoryId} onChange={event => { setCategoryId(event.target.value); setPage(1); }}>
                    <option value="">All categories</option>
                    {categories.data?.items.map((category: Category) => (
                        <option key={category.categoryId} value={category.categoryId}>{category.categoryName}</option>
                    ))}
                </select>
                {isStaff && (
                    <select aria-label="Status" value={status} onChange={event => { setStatus(event.target.value); setPage(1); }}>
                        <option value="">All statuses</option>
                        <option value="0">Draft</option>
                        <option value="1">Pending review</option>
                        <option value="2">Active</option>
                        <option value="3">Suspended</option>
                    </select>
                )}
            </div>

            {addError && <div className="error" role="alert">{addError}</div>}

            {products.loading && <Spinner label="Loading products…" />}
            {products.error && <ErrorNotice message={products.error} onRetry={products.reload} />}

            {products.data && (
                <>
                    {products.data.items.length === 0 ? (
                        <EmptyState title="No products found" hint="Try a different search or category." />
                    ) : (
                        <div className="product-grid">
                            {products.data.items.map(product => (
                                <article className="product-card" key={product.productId}>
                                    <Link to={`/products/${product.productId}`} className="product-media">
                                        {product.thumbnailUrl ? (
                                            <img src={product.thumbnailUrl} alt="" />
                                        ) : (
                                            <span className="product-placeholder">{product.productName.slice(0, 1)}</span>
                                        )}
                                    </Link>
                                    <div className="product-body">
                                        <span className="product-category">{product.categoryName}</span>
                                        <Link to={`/products/${product.productId}`} className="product-title">{product.productName}</Link>
                                        <div className="product-price">
                                            {product.maxPrice > product.minPrice && product.minPrice > 0
                                                ? `${money(product.minPrice)} – ${money(product.maxPrice)}`
                                                : money(product.minPrice)}
                                        </div>
                                        {isStaff && (
                                            <span className={`badge badge-${productStatusLabel(product.status).toLowerCase().replace(' ', '-')}`}>
                                                {productStatusLabel(product.status)}
                                            </span>
                                        )}
                                        <button
                                            type="button"
                                            className="button button-small"
                                            disabled={Boolean(added === product.productId) || !user}
                                            onClick={() => void addToCart(product)}
                                            title={user ? undefined : 'Sign in to add items'}
                                        >
                                            {added === product.productId ? 'Added ✓' : user ? 'Add to cart' : 'Sign in to buy'}
                                        </button>
                                    </div>
                                </article>
                            ))}
                        </div>
                    )}
                    <Pager page={page} totalPages={products.data.totalPages} onChange={setPage} />
                </>
            )}
        </div>
    );
}