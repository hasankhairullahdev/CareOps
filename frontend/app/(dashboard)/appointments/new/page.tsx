"use client";
import { useSession } from "next-auth/react";
import { appointmentsApi, patientsApi } from "@/lib/api";
import type { Doctor, PaginatedResult, Patient } from "@/lib/types";
import { useRouter } from "next/navigation";
import { useState, useEffect } from "react";
import Link from "next/link";
import { ArrowLeft, Calendar, User, FileText } from "lucide-react";

export default function NewAppointmentPage() {
  const { data: session } = useSession();
  const router = useRouter();

  const [doctors, setDoctors]   = useState<Doctor[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState<string | null>(null);

  const [form, setForm] = useState({
    patientId: "", doctorId: "", scheduledAt: "", notes: "",
  });

  useEffect(() => {
    const token = session?.accessToken ?? "";
    if (!token) return;
    Promise.all([
      fetch(`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000"}/api/doctors`, {
        headers: { Authorization: `Bearer ${token}` },
      }).then(r => r.json()),
      patientsApi.list(token, 1, 50) as Promise<PaginatedResult<Patient>>,
    ]).then(([docs, pts]) => {
      setDoctors(Array.isArray(docs) ? docs : docs.items ?? []);
      setPatients(pts.items ?? []);
    }).catch(() => {});
  }, [session]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await appointmentsApi.create(session?.accessToken ?? "", form);
      router.push("/appointments");
    } catch (e) {
      setError((e as Error).message);
      setLoading(false);
    }
  };

  const inputCls = "w-full px-3 py-2.5 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary bg-white text-gray-800";
  const labelCls = "block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1.5";

  return (
    <div className="max-w-2xl space-y-5">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/appointments" className="w-8 h-8 rounded-lg bg-white border border-gray-200 flex items-center justify-center hover:bg-gray-50 transition-colors">
          <ArrowLeft size={15} className="text-gray-500" />
        </Link>
        <div>
          <h1 className="text-lg font-bold text-gray-900">Buat Appointment</h1>
          <p className="text-xs text-gray-400">Isi detail appointment baru</p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-6 space-y-5">

          {/* Patient */}
          <div>
            <label className={labelCls}>
              <span className="flex items-center gap-1.5"><User size={12} /> Pasien</span>
            </label>
            <select
              required
              value={form.patientId}
              onChange={e => setForm(f => ({ ...f, patientId: e.target.value }))}
              className={inputCls}
            >
              <option value="">-- Pilih Pasien --</option>
              {patients.map(p => (
                <option key={p.id} value={p.id}>{p.firstName} {p.lastName} ({p.medicalRecordNumber})</option>
              ))}
            </select>
          </div>

          {/* Doctor */}
          <div>
            <label className={labelCls}>
              <span className="flex items-center gap-1.5"><User size={12} /> Dokter</span>
            </label>
            <select
              required
              value={form.doctorId}
              onChange={e => setForm(f => ({ ...f, doctorId: e.target.value }))}
              className={inputCls}
            >
              <option value="">-- Pilih Dokter --</option>
              {doctors.map(d => (
                <option key={d.id} value={d.id}>{d.name} — {d.specialization}</option>
              ))}
            </select>
          </div>

          {/* Date & Time */}
          <div>
            <label className={labelCls}>
              <span className="flex items-center gap-1.5"><Calendar size={12} /> Jadwal</span>
            </label>
            <input
              type="datetime-local"
              required
              value={form.scheduledAt}
              onChange={e => setForm(f => ({ ...f, scheduledAt: e.target.value }))}
              className={inputCls}
            />
          </div>

          {/* Notes */}
          <div>
            <label className={labelCls}>
              <span className="flex items-center gap-1.5"><FileText size={12} /> Catatan</span>
            </label>
            <textarea
              rows={3}
              value={form.notes}
              onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
              placeholder="Keluhan atau catatan tambahan..."
              className={`${inputCls} resize-none`}
            />
          </div>

          {error && (
            <div className="bg-danger/5 border border-danger/20 text-danger rounded-lg p-3 text-sm">{error}</div>
          )}

          <div className="flex items-center gap-3 pt-1">
            <button
              type="submit"
              disabled={loading}
              className="flex-1 bg-primary text-white text-sm font-semibold py-2.5 rounded-lg hover:bg-primary-hover transition-colors disabled:opacity-60"
            >
              {loading ? "Menyimpan..." : "Buat Appointment"}
            </button>
            <Link href="/appointments"
              className="px-5 py-2.5 text-sm font-medium text-gray-500 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors">
              Batal
            </Link>
          </div>
        </div>
      </form>
    </div>
  );
}
