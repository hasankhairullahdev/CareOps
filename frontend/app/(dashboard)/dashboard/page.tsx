import { auth } from "@/lib/auth";
import { hasAnyRole } from "@/lib/roles";
import { billingApi, patientsApi } from "@/lib/api";
import { formatRupiah } from "@/lib/utils";
import type { BillsSummary, PaginatedResult, Patient } from "@/lib/types";

async function StatCard({ title, value, sub, color }: {
  title: string; value: string | number; sub?: string; color?: string;
}) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      <p className="text-sm text-gray-500">{title}</p>
      <p className={`text-2xl font-bold mt-1 ${color ?? "text-gray-900"}`}>{value}</p>
      {sub && <p className="text-xs text-gray-400 mt-1">{sub}</p>}
    </div>
  );
}

export default async function DashboardPage() {
  const session = await auth();
  if (!session) return null;

  const token = session.accessToken ?? "";
  const isAdmin = hasAnyRole(session, ["admin"]);
  const isCashier = hasAnyRole(session, ["cashier"]);
  const isReceptionist = hasAnyRole(session, ["receptionist"]);
  const isDoctor = hasAnyRole(session, ["doctor"]);
  const isPharmacist = hasAnyRole(session, ["pharmacist"]);

  let summary: BillsSummary | null = null;
  let patientCount = 0;

  try {
    if (isAdmin || isCashier) {
      summary = await billingApi.summary(token) as BillsSummary;
    }
    if (isAdmin || isReceptionist) {
      const patients = await patientsApi.list(token, 1, 1) as PaginatedResult<Patient>;
      patientCount = patients.totalCount;
    }
  } catch { /* ignore — services may be loading */ }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">
        Dashboard
      </h1>
      <p className="text-gray-500 mb-8">
        Selamat datang, <span className="font-medium text-gray-800">{session.user?.name}</span>
      </p>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        {(isAdmin || isReceptionist) && (
          <StatCard title="Total Pasien" value={patientCount} sub="terdaftar" />
        )}
        {(isAdmin || isCashier) && summary && (
          <>
            <StatCard title="Tagihan Pending" value={summary.pendingCount} sub={formatRupiah(summary.pendingAmount)} color="text-yellow-600" />
            <StatCard title="Dibayar Hari Ini" value={summary.paidTodayCount} sub={formatRupiah(summary.paidTodayAmount)} color="text-green-600" />
            <StatCard title="Total Tagihan Hari Ini" value={summary.totalTodayCount} sub="tagihan" />
          </>
        )}
        {isDoctor && (
          <StatCard title="Role" value="Doctor" sub="Lihat appointment Anda" />
        )}
        {isPharmacist && (
          <StatCard title="Role" value="Pharmacist" sub="Kelola resep & stok" />
        )}
      </div>

      {/* Quick links */}
      <div className="mt-10">
        <h2 className="text-base font-semibold text-gray-700 mb-4">Aksi Cepat</h2>
        <div className="flex flex-wrap gap-3">
          {(isAdmin || isReceptionist) && (
            <a href="/patients/new" className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 transition-colors">
              + Daftar Pasien Baru
            </a>
          )}
          {(isAdmin || isReceptionist) && (
            <a href="/appointments/new" className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 transition-colors">
              + Buat Appointment
            </a>
          )}
          {(isAdmin || isPharmacist) && (
            <a href="/pharmacy" className="px-4 py-2 bg-emerald-600 text-white rounded-lg text-sm hover:bg-emerald-700 transition-colors">
              Resep Pending
            </a>
          )}
        </div>
      </div>
    </div>
  );
}
