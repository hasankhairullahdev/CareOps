import { auth } from "@/lib/auth";
import { appointmentsApi } from "@/lib/api";
import type { Appointment } from "@/lib/types";
import { formatDateTime } from "@/lib/utils";
import Link from "next/link";
import StatusBadge from "@/components/StatusBadge";
import AppointmentActions from "@/components/AppointmentActions";
import { ArrowLeft, Calendar, User, Stethoscope, FileText, Clock } from "lucide-react";

export default async function AppointmentDetailPage({ params }: { params: { id: string } }) {
  const session = await auth();
  const token = session?.accessToken ?? "";

  let appointment: Appointment | null = null;
  let error: string | null = null;

  try {
    appointment = await appointmentsApi.get(token, params.id) as Appointment;
  } catch (e) {
    error = (e as Error).message;
  }

  if (error) return (
    <div className="p-8 text-center text-red-500 bg-white rounded-2xl border border-red-100">{error}</div>
  );

  if (!appointment) return null;

  const roles = session?.roles ?? [];
  const canComplete = appointment.status === "Scheduled" && (roles.includes("doctor") || roles.includes("admin"));
  const canCancel   = ["Scheduled", "InProgress"].includes(appointment.status) &&
    (roles.includes("receptionist") || roles.includes("doctor") || roles.includes("admin"));
  const canPrescribe = appointment.status === "Completed" &&
    (roles.includes("doctor") || roles.includes("admin"));

  return (
    <div className="max-w-3xl space-y-5">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/appointments" className="w-8 h-8 rounded-lg bg-white border border-gray-200 flex items-center justify-center hover:bg-gray-50 transition-colors">
          <ArrowLeft size={15} className="text-gray-500" />
        </Link>
        <div className="flex-1">
          <h1 className="text-lg font-bold text-gray-900">Detail Appointment</h1>
          <p className="text-xs text-gray-400 font-mono">{appointment.id.slice(0, 18)}…</p>
        </div>
        <StatusBadge status={appointment.status} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        {/* Info card */}
        <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-6 space-y-4">
          <h2 className="text-sm font-bold text-gray-800 pb-2 border-b border-gray-100">Informasi</h2>

          {[
            { icon: User,        label: "Pasien ID",    value: appointment.patientId.slice(0, 18) + "…" },
            { icon: Stethoscope, label: "Dokter",       value: appointment.doctorName },
            { icon: Calendar,    label: "Jadwal",       value: formatDateTime(appointment.scheduledAt) },
            { icon: Clock,       label: "Dibuat",       value: formatDateTime(appointment.createdAt) },
            { icon: FileText,    label: "Catatan",      value: appointment.notes ?? "—" },
          ].map(({ icon: Icon, label, value }) => (
            <div key={label} className="flex items-start gap-3">
              <div className="w-7 h-7 rounded-lg bg-gray-100 flex items-center justify-center shrink-0">
                <Icon size={13} className="text-gray-500" />
              </div>
              <div>
                <p className="text-[10px] text-gray-400 uppercase tracking-wide">{label}</p>
                <p className="text-sm text-gray-700 font-medium break-all">{value}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Actions card */}
        <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-6 space-y-3">
          <h2 className="text-sm font-bold text-gray-800 pb-2 border-b border-gray-100">Aksi</h2>
          <AppointmentActions
            appointmentId={appointment.id}
            patientId={appointment.patientId}
            canComplete={canComplete}
            canCancel={canCancel}
            canPrescribe={canPrescribe}
            token={token}
          />

          {!canComplete && !canCancel && !canPrescribe && (
            <p className="text-sm text-gray-400 text-center py-4">Tidak ada aksi tersedia untuk status ini.</p>
          )}
        </div>
      </div>
    </div>
  );
}
