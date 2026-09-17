import { useState, type FormEvent } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api } from '../api';
import { AttributesBlock, EmptyState, ErrorNotice, IconChevronLeft, IconMinus, IconPlus, IconStar, Spinner, StatusBadge, StarRating, date, money, productStatusLabel, useLoad, useMutation } from '../ui';
import { useAuth } from '../auth';
import { useCart } from '../cart';
import { useLocation, useNavigate } from 'react-router-dom';

export function ProductPage() {
    const { id = '' } = useParams();
    const { user } = useAuth();
    const { add } = useCart();
    const navigate = useNavigate();
    const location = useLocation();
    const { data, loading, error, reload } = useLoad(() => api.product(id), [id]);

    const [variantId, setVariantId] = useState('');
    const [quantity, setQuantity] = useState(1);
    const [addNote, setAddNote] = useState<string | null>(null);
    const [rating, setRating] = useState(5);
    const [comment, setComment] = useState('');
    const review = useMutation(() => api.createReview(id, rating, comment || undefined));

    if (loading) return <div className="page"><Spinner label="Loading product…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} onRetry={reload} /></div>;
    if (!data) return <div className="page"><EmptyState title="Product not found" /></div>;

    const variants = data.variants;
    const resolved = variants.find(v => v.variantId === variantId) ?? variants[0];
    const isStaff = user?.roles.includes('Vendor') || user?.roles.includes('Admin');
    const canReview = user?.roles.includes('Customer');

    const addToCart = async () => {
        if (!resolved) { setAddNote('This product has no variants yet.'); return; }
        setAddNote(null);
        try {
            await add({
                productVariantId: resolved.variantId,
                sku: resolved.sku,
                name: data.productName,
                price: resolved.price,
                quantity,
                image: resolved.imageUrl,
                attributesJson: resolved.attributesJson,
            });
            setAddNote('Added to cart.');
        } catch (err) {
            setAddNote(err instanceof Error ? err.message : 'Could not add to cart.');
        }
    };

    const submitReview = async (event: FormEvent) => {
        event.preventDefault();
        setAddNote(null);
        const ok = await review.run();
        if (ok) {
            setComment('');
            reload();
        }
    };

    return (
        <div className="page">
            <Link to="/" className="back-link"><IconChevronLeft size={14} /> Back to catalog</Link>
            <div className="product-detail">
                <div className="product-gallery">
                    {data.variants[0]?.imageUrl ? (
                        <img src={data.variants[0].imageUrl} alt={data.productName} />
                    ) : (
                        <span className="product-placeholder large">{data.productName.slice(0, 1)}</span>
                    )}
                </div>
                <div className="product-info">
                    <span className="product-category">{data.categoryName}</span>
                    <h1>{data.productName}</h1>
                    {isStaff && <StatusBadge status={productStatusLabel(data.status)} />}
                    {data.reviewsCount > 0 && (
                        <div className="rating-row">
                            <StarRating value={data.averageRating} />
                            <span className="muted">{data.averageRating.toFixed(1)} · {data.reviewsCount} reviews</span>
                        </div>
                    )}
                    <p className="description">{data.description || 'No description provided.'}</p>

                    {variants.length === 0 ? (
                        <div className="empty"><h3>No variants yet</h3><p className="muted">This product is not purchasable yet.</p></div>
                    ) : (
                        <div className="stack">
                            {variants.length > 1 && (
                                <label className="field">
                                    <span className="field-label">Choose variant</span>
                                    <select value={resolved.variantId} onChange={event => setVariantId(event.target.value)}>
                                        {variants.map(variant => (
                                            <option key={variant.variantId} value={variant.variantId}>
                                                {variant.sku} — {money(variant.price)}
                                            </option>
                                        ))}
                                    </select>
                                </label>
                            )}
                            <AttributesBlock json={resolved.attributesJson} />
                            <div className="price-row">
                                <span className="price">{money(resolved.price)}</span>
                                {resolved.availableQuantity > 0
                                    ? <span className="stock in">In stock ({resolved.availableQuantity})</span>
                                    : <span className="stock out">Out of stock</span>}
                            </div>
                            <div className="qty-row">
                                <div className="stepper">
                                    <button type="button" aria-label="Decrease quantity" disabled={quantity <= 1} onClick={() => setQuantity(q => q - 1)}><IconMinus size={14} /></button>
                                    <span aria-live="polite">{quantity}</span>
                                    <button type="button" aria-label="Increase quantity" disabled={quantity >= 100} onClick={() => setQuantity(q => q + 1)}><IconPlus size={14} /></button>
                                </div>
                                <button
                                    type="button"
                                    className="button"
                                    disabled={Boolean(!resolved || resolved.availableQuantity <= 0)}
                                    onClick={() => { if (user) void addToCart(); else navigate('/login', { state: { from: location.pathname } }); }}
                                >
                                    {user ? 'Add to cart' : 'Sign in to buy'}
                                </button>
                            </div>
                            {addNote && <p className={addNote === 'Added to cart.' ? 'success' : 'muted'}>{addNote}</p>}
                        </div>
                    )}
                </div>
            </div>

            <section className="section">
                <div className="section-heading">
                    <div>
                        <p className="eyebrow">Reviews</p>
                        <h2>What people think</h2>
                    </div>
                    {canReview && (
                        <form className="review-form" onSubmit={submitReview}>
                            <div className="review-stars">
                                {[1, 2, 3, 4, 5].map(star => (
                                    <button key={star} type="button" aria-label={`${star} stars`} onClick={() => setRating(star)}>
                                        <IconStar size={18} style={{ color: star <= rating ? 'var(--star)' : 'var(--border)' }} />
                                    </button>
                                ))}
                            </div>
                            <textarea placeholder="Share your experience… (optional)" value={comment} onChange={event => setComment(event.target.value)} maxLength={2000} />
                            <button type="submit" className="button button-small" disabled={review.loading}>
                                {review.loading ? 'Posting…' : 'Post review'}
                            </button>
                            {review.error && <span className="error">{review.error}</span>}
                        </form>
                    )}
                </div>
                {data.reviews.length === 0 ? (
                    <EmptyState title="No reviews yet" hint="Be the first to review this product." />
                ) : (
                    <div className="review-list">
                        {data.reviews.map(reviewItem => (
                            <div className="review" key={reviewItem.reviewId}>
                                <div className="review-top">
                                    <StarRating value={reviewItem.rating} />
                                    <span className="muted">{date(reviewItem.createdAt)}</span>
                                </div>
                                {reviewItem.comment && <p>{reviewItem.comment}</p>}
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
}