import { useState, type FormEvent } from 'react';
import { api } from '../api';
import { EmptyState, ErrorNotice, IconMapPin, IconPencil, IconTrash, Spinner, useLoad } from '../ui';
import type { Address } from '../types';

const emptyForm = { fullAddress: '', city: '', governorate: '', phoneNumber: '' };

export function AddressesPage() {
    const { data, loading, error, reload } = useLoad<Address[]>(() => api.addresses(), []);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [form, setForm] = useState(emptyForm);
    const [isDefault, setIsDefault] = useState(false);
    const [busy, setBusy] = useState(false);
    const [formError, setFormError] = useState<string | null>(null);

    const startEdit = (address: Address) => {
        setEditingId(address.addressId);
        setForm({ fullAddress: address.fullAddress, city: address.city, governorate: address.governorate, phoneNumber: address.phoneNumber });
        setFormError(null);
    };

    const cancelEdit = () => {
        setEditingId(null);
        setForm(emptyForm);
        setFormError(null);
    };

    const save = async (event: FormEvent) => {
        event.preventDefault();
        setFormError(null);
        setBusy(true);
        try {
            if (editingId) await api.updateAddress(editingId, form);
            else await api.createAddress({ ...form, isDefault });
            reload();
            cancelEdit();
        } catch (err) {
            setFormError(err instanceof Error ? err.message : 'Could not save address.');
        } finally {
            setBusy(false);
        }
    };

    const remove = async (addressId: string) => {
        await api.deleteAddress(addressId);
        reload();
    };

    const setDefault = async (addressId: string) => {
        await api.setDefaultAddress(addressId);
        reload();
    };

    if (loading) return <div className="page"><Spinner label="Loading addresses…" /></div>;
    if (error) return <div className="page"><ErrorNotice message={error} onRetry={reload} /></div>;

    return (
        <div className="page">
            <header className="catalog-head">
                <div>
                    <p className="eyebrow">Delivery</p>
                    <h1>Addresses</h1>
                </div>
                {!editingId && (
                    <button type="button" className="button" onClick={() => { setEditingId('new'); setForm(emptyForm); setFormError(null); }}>Add address</button>
                )}
            </header>

            {editingId && (
                <form className="card stack" onSubmit={save}>
                    <h3>{editingId === 'new' ? 'New address' : 'Edit address'}</h3>
                    <div className="field-row">
                        <label className="field">
                            <span className="field-label">Full address</span>
                            <input value={form.fullAddress} onChange={event => setForm({ ...form, fullAddress: event.target.value })} required />
                        </label>
                        <label className="field">
                            <span className="field-label">City</span>
                            <input value={form.city} onChange={event => setForm({ ...form, city: event.target.value })} required />
                        </label>
                    </div>
                    <div className="field-row">
                        <label className="field">
                            <span className="field-label">Governorate</span>
                            <input value={form.governorate} onChange={event => setForm({ ...form, governorate: event.target.value })} required />
                        </label>
                        <label className="field">
                            <span className="field-label">Phone</span>
                            <input value={form.phoneNumber} onChange={event => setForm({ ...form, phoneNumber: event.target.value })} required />
                        </label>
                    </div>
                    {editingId === 'new' && (
                        <label className="check">
                            <input type="checkbox" checked={isDefault} onChange={event => setIsDefault(event.target.checked)} />
                            <span>Use as default delivery address</span>
                        </label>
                    )}
                    {formError && <div className="error" role="alert">{formError}</div>}
                    <div className="actions">
                        <button type="submit" className="button" disabled={busy}>{busy ? 'Saving…' : 'Save'}</button>
                        <button type="button" className="button button-outline" onClick={cancelEdit}>Cancel</button>
                    </div>
                </form>
            )}

            {(data?.length ?? 0) === 0 && !editingId ? (
                <EmptyState title="No addresses yet" hint="Add a delivery address to check out." />
            ) : (
                <div className="address-grid">
                    {data?.map(address => (
                        <div className="address-card card" key={address.addressId}>
                            <div className="address-icon"><IconMapPin size={18} /></div>
                            <div className="address-body">
                                <strong>{address.city}{address.isDefault && <span className="badge badge-active">Default</span>}</strong>
                                <p>{address.fullAddress}</p>
                                <p className="muted">{address.governorate} · {address.phoneNumber}</p>
                            </div>
                            <div className="address-actions">
                                {!address.isDefault && (
                                    <button type="button" className="button button-small button-outline" onClick={() => void setDefault(address.addressId)}>Set default</button>
                                )}
                                <button type="button" className="icon-button" aria-label="Edit address" onClick={() => startEdit(address)}><IconPencil size={16} /></button>
                                <button type="button" className="icon-button danger" aria-label="Delete address" onClick={() => void remove(address.addressId)}><IconTrash size={16} /></button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}