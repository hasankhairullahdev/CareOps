import { auth } from "@/lib/auth";
import { patientsApi, appointmentsApi } from "@/lib/api";
import type { Patient, PaginatedResult, Appointment } from "@/lib/types";
import { formatDate, formatDateTime } from "@/lib/utils";
import Link from "next/link";
import StatusBadge from "@/components/StatusBadge";
import { ArrowLeft, User, Phone, Mail, MapPin, Calendar, Edit } from "lucide-react";

export default async function PatientDetailPage({ params }: { params: { id: string } }) {
  const session = await auth();
  const token = session?.accessToken ?? "";

  let patient: Patient | null = null;
  let appointments: PaginatedResult<Appointment> | null = null;
  let error: string | null = null;

  try {
    [patient, appointments] = await Promise.all([
      patientsApi.get(token, params.id) as Promise<Patient>,
      appointmentsApi.list(token, { patientId: params.id, pageSize: "10" }) as Promise<PaginatedResult<Appointment>>,
    ]);
  } catch (e) {
    error = (e as Error).message;
  }

  if (error) return (
    <div className="p-8 text-center text-red-500 bg-white rounded-2xl border border-red-100">
      {error}
    </div>
  );

  if (!patient) return null;

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/patients" className="w-8 h-8 rounded-lg bg-white border border-gray-200 flex items-center justify-center hover:bg-gray-50 transition-colors">
          <ArrowLeft size={15} className="text-gray-500" />
        </Link>
        <div>
          <h1 className="text-lg font-bold text-gray-900">Detail Pasien</h1>
          <p className="text-xs text-gray-400">{patient.medicalRecordNumber}</p>
        </div>
        <div className="ml-auto">
          <Link href={`/patients/${patient.id}/edit`}
            className="flex items-center gap-1.5 text-sm font-medium text-primary bg-primary/10 hover:bg-primary/20 px-3 py-1.5 rounded-lg transition-colors">
            <Edit size={14} />
            Edit
          </Link>
        </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-5">
        {/* Profile card */}
        <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-6">
          {/* Avatar */}
          <div className="flex flex-col items-center text-center pb-5 border-b border-gray-100">
            <div className="w-20 h-20 rounded-full bg-primary/10 flex items-center justify-center text-primary text-2xl font-bold mb-3">
              {patient.firstName.charAt(0)}{patient.lastName.charAt(0)}
            </div>
            <h2 className="text-base font-bold text-gray-900">{patient.firstName} {patient.lastName}</h2>
            <p className="text-xs text-gray-400 mt-1">{patient.medicalRecordNumber}</p>
            <span className="mt-2 px-3 py-1 bg-success/10 text-success text-xs font-semibold rounded-full">Active</span>
          </div>

          {/* Info rows */}
          <div className="pt-4 space-y-3">
            {[
              { icon: User,     label: "Gender",      value: patient.gender },
              { icon: Calendar, label: "Tanggal Lahir", value: formatDate(patient.dateOfBirth) },
              { icon: Phone,    label: "Telepon",      value: patient.phoneNumber },
              { icon: Mail,     label: "Email",        value: patient.email },
              { icon: MapPin,   label: "Alamat",       value: patient.address },
            ].map(({ icon: Icon, label, value }) => (
              <div key={label} className="flex items-start gap-3">
                <div className="w-7 h-7 rounded-lg bg-gray-100 flex items-center justify-center shrink-0 mt-0.5">
                  <Icon size={13} className="text-gray-500" />
                </div>
                <div className="min-w-0">
                  <p className="text-[10px] text-gray-400 uppercase tracking-wide">{label}</p>
                  <p className="text-sm text-gray-700 font-medium">{value || "—"}</p>
                </div>
              </div>
            ))}
            <div className="pt-2 text-[11px] text-gray-400">
              Terdaftar: {formatDate(patient.createdAt)}
            </div>
          </div>
        </div>

        {/* Appointment history */}
        <div className="xl:col-span-2 bg-white rounded-2xl border border-gray-100 shadow-card overflow-hidden">
          <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
            <h2 className="text-sm font-bold text-gray-800">Riwayat Appointment</h2>
            <span className="text-xs text-gray-400">{appointments?.totalCount ?? 0} total</span>
          </div>
          {appointments && appointments.items.length > 0 ? (
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-50">
                  <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase">Dokter</th>
                  <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase">Jadwal</th>
                  <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase">Status</th>
                  <th className="px-5 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {appointments.items.map(a => (
                  <tr key={a.id} className="hover:bg-gray-50/60 transition-colors">
                    <td className="px-5 py-3 font-medium text-gray-800 text-xs">{a.doctorName}</td>
                    <td className="px-5 py-3 text-gray-500 text-xs">{formatDateTime(a.scheduledAt)}</td>
                    <td className="px-5 py-3"><StatusBadge status={a.status} /></td>
                    <td className="px-5 py-3">
                      <Link href={`/appointments/${a.id}`} className="text-primary text-xs hover:underline">Detail</Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <div className="px-5 py-16 text-center text-gray-400 text-sm">Belum ada riwayat appointment.</div>
          )}
        </div>
      </div>
    </div>
  );
}
