"use client";
import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { lookupsApi } from "@/lib/api";
import { AllergyType } from "@/lib/types";
import { AlertCircle, Plus, Pencil, Trash2, CheckCircle2, XCircle, X } from "lucide-react";

type FormState = { name: string; description: string; isActive: boolean };
const EMPTY: FormState = { name: "", description: "", isActive: true };

export default function AllergyTypesPage() {
  const { data: session } = useSession();
  const [items, setItems] = useState<AllergyType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modal, setModal] = useState<{ open: boolean; editing: AllergyType | null }>({ open: false, editing: null });
  const [form, setForm] = useState<FormState>(EMPTY);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<string | null>(null);

  async function load() {
    if (!session?.accessToken) return;
    setLoading(true);
    try {
      const data = await lookupsApi.allergyTypes(session.accessToken as string) as AllergyType[];
      setItems(data);
    } catch (e: unknown) { setError(e instanceof Error ? e.message : "Failed to load"); }
    finally { setLoading(false); }
  }

  useEffect(() => { if (session?.accessToken) load(); }, [session]);

  function openCreate() { setForm(EMPTY); setFormError(null); setModal({ open: true, editing: null }); }
  function openEdit(a: AllergyType) {
    setForm({ name: a.name, description: a.description ?? "", isActive: a.isActive });
    setFormError(null);
    setModal({ open: true, editing: a });
  }
  function closeModal() { setModal({ open: false, editing: null }); }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    if (!session?.accessToken) return;
    setSaving(true); setFormError(null);
    try {
      const payload = { name: form.name, description: form.description || null, isActive: form.isActive };
      if (modal.editing) await lookupsApi.updateAllergyType(session.accessToken as string, modal.editing.id, payload);
      else await lookupsApi.createAllergyType(session.accessToken as string, payload);
      closeModal(); load();
    } catch (e: unknown) { setFormError(e instanceof Error ? e.message : "Save failed"); }
    finally { setSaving(false); }
  }

  async function handleDelete(id: string, name: string) {
    if (!confirm(`Delete "${name}"?`)) return;
    setDeleting(id);
    try {
      await lookupsApi.deleteAllergyType(session!.accessToken as string, id);
      setItems(prev => prev.filter(a => a.id !== id));
    } catch (e: unknown) { alert(e instanceof Error ? e.message : "Delete failed"); }
    finally { setDeleting(null); }
  }

  return (
    <div className="p-6 space-y-5">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
            <AlertCircle size={20} className="text-primary" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-gray-900">Allergy Types</h1>
            <p className="text-xs text-gray-400">Manage allergy type reference data</p>
          </div>
        </div>
        <button onClick={openCreate}
          className="flex items-center gap-1.5 bg-primary text-white text-sm px-4 py-2 rounded-lg hover:bg-primary/90 transition-colors">
          <Plus size={15} /> Add Allergy Type
        </button>
      </div>

      {error && <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>}
      {loading ? <div className="text-sm text-gray-400 py-10 text-center">Loading…</div> : (
        <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/60">
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Name</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Description</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && <tr><td colSpan={4} className="text-center py-10 text-gray-400">No allergy types yet.</td></tr>}
              {items.map(a => (
                <tr key={a.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                  <td className="px-4 py-3 font-medium text-gray-900">{a.name}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">{a.description ?? "—"}</td>
                  <td className="px-4 py-3">
                    {a.isActive
                      ? <span className="flex items-center gap-1 text-xs text-emerald-600 font-medium"><CheckCircle2 size={12} /> Active</span>
                      : <span className="flex items-center gap-1 text-xs text-gray-400 font-medium"><XCircle size={12} /> Inactive</span>}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2 justify-end">
                      <button onClick={() => openEdit(a)} className="p-1.5 text-gray-400 hover:text-primary hover:bg-primary/10 rounded-lg transition-colors"><Pencil size={13} /></button>
                      <button onClick={() => handleDelete(a.id, a.name)} disabled={deleting === a.id}
                        className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50"><Trash2 size={13} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="px-4 py-3 border-t border-gray-100 text-xs text-gray-400">{items.length} allergy type{items.length !== 1 ? "s" : ""}</div>
        </div>
      )}

      {modal.open && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 p-6">
            <div className="flex items-center justify-between mb-5">
              <h2 className="text-base font-bold text-gray-900">{modal.editing ? "Edit Allergy Type" : "Add Allergy Type"}</h2>
              <button onClick={closeModal} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 transition-colors"><X size={16} /></button>
            </div>
            {formError && <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{formError}</div>}
            <form onSubmit={handleSave} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-600 mb-1.5">Name *</label>
                <input required value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} placeholder="Penicillin"
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-600 mb-1.5">Description</label>
                <textarea rows={2} value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))}
                  placeholder="Optional description…"
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30 resize-none" />
              </div>
              {modal.editing && (
                <div>
                  <label className="block text-xs font-semibold text-gray-600 mb-1.5">Status</label>
                  <select value={String(form.isActive)} onChange={e => setForm(p => ({ ...p, isActive: e.target.value === "true" }))}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30">
                    <option value="true">Active</option>
                    <option value="false">Inactive</option>
                  </select>
                </div>
              )}
              <div className="flex justify-end gap-3 pt-2">
                <button type="button" onClick={closeModal} className="px-4 py-2 text-sm text-gray-500 hover:text-gray-700 transition-colors">Cancel</button>
                <button type="submit" disabled={saving}
                  className="px-5 py-2 bg-primary text-white text-sm rounded-lg hover:bg-primary/90 disabled:opacity-60 transition-colors">
                  {saving ? "Saving…" : modal.editing ? "Save Changes" : "Add"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
