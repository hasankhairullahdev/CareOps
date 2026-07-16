"use client";
import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { tariffsApi } from "@/lib/api";
import { ServiceTariff, PaginatedTariffsResult } from "@/lib/types";
import { Tag, Plus, Pencil, Trash2, CheckCircle2, XCircle, X } from "lucide-react";

type TariffFormState = {
  serviceName: string;
  category: string;
  price: string;
  description: string;
  isActive: boolean;
};

const EMPTY_FORM: TariffFormState = {
  serviceName: "",
  category: "",
  price: "",
  description: "",
  isActive: true,
};

export default function TariffsPage() {
  const { data: session } = useSession();
  const [result, setResult] = useState<PaginatedTariffsResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modal state
  const [modal, setModal] = useState<{ open: boolean; editing: ServiceTariff | null }>({ open: false, editing: null });
  const [form, setForm] = useState<TariffFormState>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<string | null>(null);

  async function load() {
    if (!session?.accessToken) return;
    setLoading(true);
    try {
      const data = await tariffsApi.list(session.accessToken as string, { pageSize: "100" }) as PaginatedTariffsResult;
      setResult(data);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Failed to load tariffs");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { if (session?.accessToken) load(); }, [session]);

  function openCreate() {
    setForm(EMPTY_FORM);
    setFormError(null);
    setModal({ open: true, editing: null });
  }

  function openEdit(t: ServiceTariff) {
    setForm({
      serviceName: t.serviceName,
      category: t.category,
      price: String(t.price),
      description: t.description ?? "",
      isActive: t.isActive,
    });
    setFormError(null);
    setModal({ open: true, editing: t });
  }

  function closeModal() {
    setModal({ open: false, editing: null });
  }

  function handleChange(e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) {
    const { name, value } = e.target;
    if (name === "isActive") {
      setForm(prev => ({ ...prev, isActive: value === "true" }));
    } else {
      setForm(prev => ({ ...prev, [name]: value }));
    }
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    if (!session?.accessToken) return;
    setSaving(true);
    setFormError(null);
    try {
      const payload = {
        serviceName: form.serviceName,
        category: form.category,
        price: parseFloat(form.price),
        description: form.description || null,
        isActive: form.isActive,
      };
      if (modal.editing) {
        await tariffsApi.update(session.accessToken as string, modal.editing.id, payload);
      } else {
        await tariffsApi.create(session.accessToken as string, payload);
      }
      closeModal();
      load();
    } catch (e: unknown) {
      setFormError(e instanceof Error ? e.message : "Save failed");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id: string, name: string) {
    if (!confirm(`Delete "${name}"? This cannot be undone.`)) return;
    setDeleting(id);
    try {
      await tariffsApi.delete(session!.accessToken as string, id);
      setResult(prev => prev ? { ...prev, items: prev.items.filter(t => t.id !== id), totalCount: prev.totalCount - 1 } : prev);
    } catch (e: unknown) {
      alert(e instanceof Error ? e.message : "Delete failed");
    } finally {
      setDeleting(null);
    }
  }

  // Group by category
  const grouped = result?.items.reduce<Record<string, ServiceTariff[]>>((acc, t) => {
    (acc[t.category] = acc[t.category] ?? []).push(t);
    return acc;
  }, {}) ?? {};

  return (
    <div className="p-6 space-y-5">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
            <Tag size={20} className="text-primary" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-gray-900">Service Tariffs</h1>
            <p className="text-xs text-gray-400">Manage service pricing for billing</p>
          </div>
        </div>
        <button
          onClick={openCreate}
          className="flex items-center gap-1.5 bg-primary text-white text-sm px-4 py-2 rounded-lg hover:bg-primary/90 transition-colors"
        >
          <Plus size={15} />
          Add Tariff
        </button>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>
      )}

      {loading ? (
        <div className="text-sm text-gray-400 py-10 text-center">Loading…</div>
      ) : (
        <div className="space-y-4">
          {Object.entries(grouped).map(([category, items]) => (
            <div key={category} className="bg-white rounded-xl border border-gray-100 overflow-hidden">
              <div className="px-4 py-3 border-b border-gray-100 bg-gray-50/60">
                <span className="text-xs font-bold text-gray-500 uppercase tracking-widest">{category}</span>
              </div>
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-50">
                    <th className="text-left px-4 py-2.5 text-xs font-semibold text-gray-400">Service Name</th>
                    <th className="text-left px-4 py-2.5 text-xs font-semibold text-gray-400">Description</th>
                    <th className="text-right px-4 py-2.5 text-xs font-semibold text-gray-400">Price (Rp)</th>
                    <th className="text-left px-4 py-2.5 text-xs font-semibold text-gray-400">Status</th>
                    <th className="px-4 py-2.5" />
                  </tr>
                </thead>
                <tbody>
                  {items.map(t => (
                    <tr key={t.id} className="border-b border-gray-50 hover:bg-gray-50/40 transition-colors">
                      <td className="px-4 py-3 font-medium text-gray-900">{t.serviceName}</td>
                      <td className="px-4 py-3 text-gray-500 text-xs">{t.description ?? "—"}</td>
                      <td className="px-4 py-3 text-right font-semibold text-gray-900 tabular-nums">
                        {t.price.toLocaleString("id-ID")}
                      </td>
                      <td className="px-4 py-3">
                        {t.isActive ? (
                          <span className="flex items-center gap-1 text-xs text-emerald-600 font-medium">
                            <CheckCircle2 size={12} /> Active
                          </span>
                        ) : (
                          <span className="flex items-center gap-1 text-xs text-gray-400 font-medium">
                            <XCircle size={12} /> Inactive
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2 justify-end">
                          <button
                            onClick={() => openEdit(t)}
                            className="p-1.5 text-gray-400 hover:text-primary hover:bg-primary/10 rounded-lg transition-colors"
                          >
                            <Pencil size={13} />
                          </button>
                          <button
                            onClick={() => handleDelete(t.id, t.serviceName)}
                            disabled={deleting === t.id}
                            className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50"
                          >
                            <Trash2 size={13} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ))}
          {result && result.totalCount === 0 && (
            <div className="bg-white rounded-xl border border-gray-100 py-12 text-center text-sm text-gray-400">
              No tariffs configured yet. Add your first service tariff.
            </div>
          )}
        </div>
      )}

      {/* Modal */}
      {modal.open && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 p-6">
            <div className="flex items-center justify-between mb-5">
              <h2 className="text-base font-bold text-gray-900">
                {modal.editing ? "Edit Tariff" : "Add Tariff"}
              </h2>
              <button onClick={closeModal} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors">
                <X size={16} />
              </button>
            </div>

            {formError && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{formError}</div>
            )}

            <form onSubmit={handleSave} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-600 mb-1.5">Service Name *</label>
                <input name="serviceName" required value={form.serviceName} onChange={handleChange}
                  placeholder="Biaya Konsultasi Umum"
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-600 mb-1.5">Category *</label>
                <input name="category" required value={form.category} onChange={handleChange}
                  placeholder="Consultation"
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-600 mb-1.5">Price (Rp) *</label>
                <input name="price" type="number" required min={0} step={1000} value={form.price} onChange={handleChange}
                  placeholder="150000"
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-600 mb-1.5">Description</label>
                <textarea name="description" rows={2} value={form.description} onChange={handleChange}
                  placeholder="Optional description…"
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30 resize-none" />
              </div>
              {modal.editing && (
                <div>
                  <label className="block text-xs font-semibold text-gray-600 mb-1.5">Status</label>
                  <select name="isActive" value={String(form.isActive)} onChange={handleChange}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30">
                    <option value="true">Active</option>
                    <option value="false">Inactive</option>
                  </select>
                </div>
              )}
              <div className="flex justify-end gap-3 pt-2">
                <button type="button" onClick={closeModal}
                  className="px-4 py-2 text-sm text-gray-500 hover:text-gray-700 transition-colors">
                  Cancel
                </button>
                <button type="submit" disabled={saving}
                  className="px-5 py-2 bg-primary text-white text-sm rounded-lg hover:bg-primary/90 disabled:opacity-60 transition-colors">
                  {saving ? "Saving…" : modal.editing ? "Save Changes" : "Add Tariff"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
