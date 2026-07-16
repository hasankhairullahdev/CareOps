"use client";
import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { suppliersApi } from "@/lib/api";
import { Supplier, GetSuppliersResult } from "@/lib/types";
import { Truck, Plus, Pencil, Trash2, CheckCircle2, XCircle, X } from "lucide-react";

type FormState = {
  name: string; contactPerson: string; phone: string; email: string; address: string; isActive: boolean;
};
const EMPTY: FormState = { name: "", contactPerson: "", phone: "", email: "", address: "", isActive: true };

export default function SuppliersPage() {
  const { data: session } = useSession();
  const [result, setResult] = useState<GetSuppliersResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modal, setModal] = useState<{ open: boolean; editing: Supplier | null }>({ open: false, editing: null });
  const [form, setForm] = useState<FormState>(EMPTY);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<string | null>(null);

  async function load() {
    if (!session?.accessToken) return;
    setLoading(true);
    try {
      const data = await suppliersApi.list(session.accessToken as string) as GetSuppliersResult;
      setResult(data);
    } catch (e: unknown) { setError(e instanceof Error ? e.message : "Failed to load"); }
    finally { setLoading(false); }
  }

  useEffect(() => { if (session?.accessToken) load(); }, [session]);

  function openCreate() { setForm(EMPTY); setFormError(null); setModal({ open: true, editing: null }); }
  function openEdit(s: Supplier) {
    setForm({ name: s.name, contactPerson: s.contactPerson ?? "", phone: s.phone ?? "", email: s.email ?? "", address: s.address ?? "", isActive: s.isActive });
    setFormError(null);
    setModal({ open: true, editing: s });
  }
  function closeModal() { setModal({ open: false, editing: null }); }

  function handleChange(e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) {
    const { name, value } = e.target;
    if (name === "isActive") setForm(p => ({ ...p, isActive: value === "true" }));
    else setForm(p => ({ ...p, [name]: value }));
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    if (!session?.accessToken) return;
    setSaving(true); setFormError(null);
    try {
      const payload = {
        name: form.name,
        contactPerson: form.contactPerson || null,
        phone: form.phone || null,
        email: form.email || null,
        address: form.address || null,
        isActive: form.isActive,
      };
      if (modal.editing) await suppliersApi.update(session.accessToken as string, modal.editing.id, payload);
      else await suppliersApi.create(session.accessToken as string, payload);
      closeModal(); load();
    } catch (e: unknown) { setFormError(e instanceof Error ? e.message : "Save failed"); }
    finally { setSaving(false); }
  }

  async function handleDelete(id: string, name: string) {
    if (!confirm(`Delete "${name}"?`)) return;
    setDeleting(id);
    try {
      await suppliersApi.delete(session!.accessToken as string, id);
      setResult(prev => prev ? { ...prev, items: prev.items.filter(s => s.id !== id), totalCount: prev.totalCount - 1 } : prev);
    } catch (e: unknown) { alert(e instanceof Error ? e.message : "Delete failed"); }
    finally { setDeleting(null); }
  }

  return (
    <div className="p-6 space-y-5">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
            <Truck size={20} className="text-primary" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-gray-900">Suppliers</h1>
            <p className="text-xs text-gray-400">Manage medicine & supply vendors</p>
          </div>
        </div>
        <button onClick={openCreate}
          className="flex items-center gap-1.5 bg-primary text-white text-sm px-4 py-2 rounded-lg hover:bg-primary/90 transition-colors">
          <Plus size={15} /> Add Supplier
        </button>
      </div>

      {error && <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>}
      {loading ? <div className="text-sm text-gray-400 py-10 text-center">Loading…</div> : (
        <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/60">
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Name</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Contact</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Phone / Email</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody>
              {result?.items.length === 0 && <tr><td colSpan={5} className="text-center py-10 text-gray-400">No suppliers yet.</td></tr>}
              {result?.items.map(s => (
                <tr key={s.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                  <td className="px-4 py-3 font-medium text-gray-900">{s.name}</td>
                  <td className="px-4 py-3 text-gray-600 text-xs">{s.contactPerson ?? "—"}</td>
                  <td className="px-4 py-3 text-xs">
                    {s.phone && <div className="text-gray-600">{s.phone}</div>}
                    {s.email && <div className="text-primary/70">{s.email}</div>}
                    {!s.phone && !s.email && <span className="text-gray-400">—</span>}
                  </td>
                  <td className="px-4 py-3">
                    {s.isActive
                      ? <span className="flex items-center gap-1 text-xs text-emerald-600 font-medium"><CheckCircle2 size={12} /> Active</span>
                      : <span className="flex items-center gap-1 text-xs text-gray-400 font-medium"><XCircle size={12} /> Inactive</span>}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2 justify-end">
                      <button onClick={() => openEdit(s)} className="p-1.5 text-gray-400 hover:text-primary hover:bg-primary/10 rounded-lg transition-colors"><Pencil size={13} /></button>
                      <button onClick={() => handleDelete(s.id, s.name)} disabled={deleting === s.id}
                        className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50"><Trash2 size={13} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {result && <div className="px-4 py-3 border-t border-gray-100 text-xs text-gray-400">{result.totalCount} supplier{result.totalCount !== 1 ? "s" : ""}</div>}
        </div>
      )}

      {modal.open && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg mx-4 p-6">
            <div className="flex items-center justify-between mb-5">
              <h2 className="text-base font-bold text-gray-900">{modal.editing ? "Edit Supplier" : "Add Supplier"}</h2>
              <button onClick={closeModal} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"><X size={16} /></button>
            </div>
            {formError && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{formError}</div>}
            <form onSubmit={handleSave} className="space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div className="col-span-2">
                  <label className="block text-xs font-semibold text-gray-600 mb-1">Name *</label>
                  <input name="name" required value={form.name} onChange={handleChange} placeholder="PT Kimia Farma"
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-gray-600 mb-1">Contact Person</label>
                  <input name="contactPerson" value={form.contactPerson} onChange={handleChange}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-gray-600 mb-1">Phone</label>
                  <input name="phone" value={form.phone} onChange={handleChange}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-gray-600 mb-1">Email</label>
                  <input name="email" type="email" value={form.email} onChange={handleChange}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
                </div>
                {modal.editing && (
                  <div>
                    <label className="block text-xs font-semibold text-gray-600 mb-1">Status</label>
                    <select name="isActive" value={String(form.isActive)} onChange={handleChange}
                      className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30">
                      <option value="true">Active</option>
                      <option value="false">Inactive</option>
                    </select>
                  </div>
                )}
                <div className="col-span-2">
                  <label className="block text-xs font-semibold text-gray-600 mb-1">Address</label>
                  <textarea name="address" rows={2} value={form.address} onChange={handleChange}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30 resize-none" />
                </div>
              </div>
              <div className="flex justify-end gap-3 pt-1">
                <button type="button" onClick={closeModal} className="px-4 py-2 text-sm text-gray-500 hover:text-gray-700 transition-colors">Cancel</button>
                <button type="submit" disabled={saving}
                  className="px-5 py-2 bg-primary text-white text-sm rounded-lg hover:bg-primary/90 disabled:opacity-60 transition-colors">
                  {saving ? "Saving…" : modal.editing ? "Save Changes" : "Add Supplier"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
