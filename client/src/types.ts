export type Role = 'User' | 'Customer' | 'Vendor' | 'Admin';

export const ProductStatusValues = ['Draft', 'PendingReview', 'Active', 'Suspended'] as const;
export type ProductStatus = (typeof ProductStatusValues)[number];

export const DiscountTypeValues = ['Percentage', 'FixedAmount'] as const;
export type DiscountType = (typeof DiscountTypeValues)[number];

export const OrderStatusValues = ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'] as const;
export type OrderStatus = (typeof OrderStatusValues)[number];

export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}

// ---- Auth ----
export interface AuthResponse {
    userId: string;
    email: string;
    accessToken: string;
    accessTokenExpiresAt: string;
}

export interface User {
    id?: string;
    email: string;
    roles: Role[];
}

// ---- Products ----
export interface ProductListItem {
    productId: string;
    productName: string;
    categoryName: string;
    categoryId: string;
    status: number;
    minPrice: number;
    maxPrice: number;
    thumbnailUrl: string | null;
}

export interface ProductVariant {
    variantId: string;
    productId?: string;
    sku: string;
    price: number;
    imageUrl: string | null;
    attributesJson: string;
    availableQuantity: number;
}

export interface ProductReview {
    reviewId: string;
    userId: string;
    rating: number;
    comment: string | null;
    createdAt: string;
}

export interface ProductDetails {
    productId: string;
    productName: string;
    description: string;
    status: number;
    categoryId: string;
    categoryName: string;
    variants: ProductVariant[];
    reviews: ProductReview[];
    averageRating: number;
    reviewsCount: number;
}

// ---- Categories ----
export interface Category {
    categoryId: string;
    categorySlug: string;
    categoryName: string;
}

// ---- Carts ----
export interface CartItemResponse {
    itemId: string;
    productVariantId: string;
    sku: string;
    productName: string;
    imageUrl: string | null;
    attributesJson: string;
    unitPrice: number;
    quantity: number;
    lineTotal: number;
}

export interface CartResponse {
    cartId: string;
    userId: string | null;
    createdAt: string;
    items: CartItemResponse[];
    totalPrice: number;
}

export interface AddCartItemResponse {
    cartId: string;
    cartItemId: string;
    quantity: number;
}

// ---- Addresses ----
export interface Address {
    addressId: string;
    fullAddress: string;
    city: string;
    governorate: string;
    phoneNumber: string;
    isDefault: boolean;
}

// ---- Orders ----
export interface OrderSummary {
    id: string;
    createdAt: string;
    subTotal: number;
    discountAmount: number;
    totalAmount: number;
    status: OrderStatus | string;
    customerId?: string;
}

export interface OrderItem {
    productVariantId: string;
    quantity: number;
    unitPrice: number;
    sku: string;
}

export interface OrderDetail extends OrderSummary {
    shippingAddressSnapshot: string;
    items: OrderItem[];
}

// ---- Coupons ----
export interface Coupon {
    id: string;
    code: string;
    discountType: number;
    discountValue: number;
    minOrderAmount: number | null;
    maxUsageCount: number;
    usedCount: number;
    expiryDate: string;
    isActive: boolean;
}

export interface CouponValidation extends Coupon {
    discountAmount: number;
}

// ---- Payments ----
export interface PaymentStart {
    clientSecret: string;
    transactionReference: string;
}

export interface PaymentStatus {
    transactionReference: string;
    status: OrderStatus | string;
}

// ---- Notifications ----
export interface Notification {
    id: string;
    title: string;
    body: string;
    isRead: boolean;
    createdAt: string;
}

// ---- Local cart item ----
export interface CartItem {
    itemId?: string;
    productVariantId: string;
    sku: string;
    name: string;
    price: number;
    quantity: number;
    image?: string | null;
    attributesJson?: string;
}