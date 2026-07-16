"use client";
import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { useRouter, useParams } from "next/navigation";
import Link from "next/link";
import { doctorsApi } from "@/lib/api";
import { Doctor } from "@/lib/types";
import { ArrowLeft, UserCog } from "lucide-react";

export default function EditDoctorPage() {
  const { data: session } = useSession();
  const router = useRouter();
  const { id } = useParams<{ id: string }>();

  const [form, setForm] = useState({
    name: "",
    specialization: "",
    licenseNumber: "",
    schedule: "",
    phone: "",
    email: "",
    isActive: true,
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!session?.accessToken) return;
    doctorsApi.get(session.accessToken as string, id)
      .then((d: unknown) => {
        const doc = d as Doctor;
        setForm({
          name: doc.name,
          specialization: doc.specialization,
          licenseNumber: doc.licenseNumber,
          schedule: doc.schedule,
          phone: doc.phone ?? "",
          email: doc.email ?? "",
          isActive: doc.isActive,
        });
      })
      .catch((e: unknown) => setError(e instanceof Error ? e.message : "Failed to load doctor"))
      .finally(() => setLoading(false));
  }, [session, id]);

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const { name, value, type, checked } = e.target;
    setForm(prev => ({ ...prev, [name]: type === "checkbox" ? checked : value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!session?.accessToken) return;
    setSaving(true);
    setError(null);
    try {
      await doctorsApi.update(session.accessToken as string, id, {
        name: form.name,
        specialization: form.specialization,
        licenseNumber: form.licenseNumber,
        schedule: form.schedule,
        phone: form.phone || null,
        email: form.email || null,
        isActive: form.isActive,
      });
      router.push("/doctors");
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Failed to update doctor");
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="p-6 text-sm text-gray-400">Loading…</div>;

  return (
    <div className="p-6 max-w-2xl mx-auto space-y-5">
      <div className="flex items-center gap-3">
        <Link href="/doctors" className="p-2 rounded-lg hover:bg-gray-100 text-gray-500 transition-colors">
          <ArrowLeft size={16} />
        </Link>
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
            <UserCog size={20} className="text-primary" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-gray-900">Edit Doctor</h1>
            <p className="text-xs text-gray-400">Update doctor profile</p>
          </div>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-100 p-6 space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div className="col-span-2">
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Full Name *</label>
            <input name="name" required value={form.name} onChange={handleChange}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Specialization *</label>
            <input name="specialization" required value={form.specialization} onChange={handleChange}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">License Number *</label>
            <input name="licenseNumber" required value={form.licenseNumber} onChange={handleChange}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
          </div>
          <div className="col-span-2">
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Schedule *</label>
            <input name="schedule" required value={form.schedule} onChange={handleChange}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Phone</label>
            <input name="phone" value={form.phone} onChange={handleChange}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-600 mb-1.5">Email</label>
            <input name="email" type="email" value={form.email} onChange={handleChange}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30" />
          </div>
          <div className="col-span-2 flex items-center gap-3 pt-1">
            <input
              type="checkbox"
              id="isActive"
              name="isActive"
              checked={form.isActive}
              onChange={handleChange}
              className="w-4 h-4 accent-primary rounded"
            />
            <label htmlFor="isActive" className="text-sm text-gray-700 font-medium">Active doctor</label>
          </div>
        </div>

        <div className="flex items-center justify-end gap-3 pt-2 border-t border-gray-100">
          <Link href="/doctors"
            className="px-4 py-2 text-sm text-gray-500 hover:text-gray-700 transition-colors">
            Cancel
          </Link>
          <button type="submit" disabled={saving}
            className="px-5 py-2 bg-primary text-white text-sm rounded-lg hover:bg-primary/90 disabled:opacity-60 transition-colors">
            {saving ? "Saving…" : "Save Changes"}
          </button>
        </div>
      </form>
    </div>
  );
}
