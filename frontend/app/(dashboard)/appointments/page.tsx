import { auth } from "@/lib/auth";
import { appointmentsApi } from "@/lib/api";
import type { PaginatedResult, Appointment } from "@/lib/types";
import { formatDateTime } from "@/lib/utils";
import Link from "next/link";
import StatusBadge from "@/components/StatusBadge";

export default async function AppointmentsPage({
  searchParams,
}: {
  searchParams: { page?: string; status?: string };
}) {
  const session = await auth();
  const token = session?.accessToken ?? "";
  const page = Number(searchParams.page ?? 1);

  let result: PaginatedResult<Appointment> | null = null;
  let error: string | null = null;

  try {
    const params: Record<string, string> = { page: String(page), pageSize: "20" };
    if (searchParams.status) params.status = searchParams.status;
    result = await appointmentsApi.list(token, params) as PaginatedResult<Appointment>;
  } catch (e) {
    error = (e as Error).message;
  }

  const statuses = ["Scheduled", "InProgress", "Completed", "Cancelled"];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Appointments</h1>
        <Link href="/appointments/new"
          className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 transition-colors">
          + Buat Appointment
        </Link>
      </div>

      {/* Filter */}
      <div className="flex gap-2 mb-6">
        <Link href="/appointments" className={`px-3 py-1.5 rounded-full text-xs font-medium border transition-colors ${!searchParams.status ? "bg-blue-600 text-white border-blue-600" : "border-gray-300 text-gray-600 hover:bg-gray-50"}`}>
          Semua
        </Link>
        {statuses.map(s => (
          <Link key={s} href={`/appointments?status=${s}`}
            className={`px-3 py-1.5 rounded-full text-xs font-medium border transition-colors ${searchParams.status === s ? "bg-blue-600 text-white border-blue-600" : "border-gray-300 text-gray-600 hover:bg-gray-50"}`}>
            {s}
          </Link>
        ))}
      </div>

      {error && <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg p-4 mb-4 text-sm">{error}</div>}

      {result && (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="text-left px-4 py-3 font-medium text-gray-600">Dokter</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Jadwal</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Status</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Catatan</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {result.items.map((a) => (
                <tr key={a.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 font-medium">{a.doctorName}</td>
                  <td className="px-4 py-3 text-gray-600">{formatDateTime(a.scheduledAt)}</td>
                  <td className="px-4 py-3"><StatusBadge status={a.status} /></td>
                  <td className="px-4 py-3 text-gray-500 text-xs max-w-xs truncate">{a.notes ?? "—"}</td>
                  <td className="px-4 py-3">
                    <Link href={`/appointments/${a.id}`} className="text-blue-600 hover:underline text-xs">Detail</Link>
                  </td>
                </tr>
              ))}
              {result.items.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-8 text-center text-gray-400">Tidak ada appointment.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
