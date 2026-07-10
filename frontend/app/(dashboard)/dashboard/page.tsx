import { auth } from "@/lib/auth";
import { hasAnyRole } from "@/lib/roles";
import { billingApi, patientsApi, appointmentsApi } from "@/lib/api";
import { formatRupiah, formatDateTime } from "@/lib/utils";
import type { BillsSummary, PaginatedResult, Patient, Appointment } from "@/lib/types";
import Link from "next/link";
import StatusBadge from "@/components/StatusBadge";
import {
  Users, CalendarDays, ClipboardList, CreditCard,
  TrendingUp, TrendingDown, Stethoscope, Plus,
} from "lucide-react";

function StatCard({
  title, value, sub, icon: Icon, bg, iconColor, trend, trendUp,
}: {
  title: string; value: string | number; sub?: string;
  icon: React.ElementType; bg: string; iconColor: string;
  trend?: string; trendUp?: boolean;
}) {
  return (
    <div className="bg-white rounded-2xl p-5 border border-gray-100 shadow-card flex items-center gap-4 hover:shadow-card2 transition-shadow">
      <div className={`w-14 h-14 rounded-2xl ${bg} flex items-center justify-center shrink-0`}>
        <Icon size={24} className={iconColor} />
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-xs text-gray-400 uppercase tracking-wide font-medium">{title}</p>
        <p className="text-2xl font-bold text-gray-900 mt-0.5 leading-none">{value}</p>
        {sub && <p className="text-[11px] text-gray-500 mt-1">{sub}</p>}
      </div>
      {trend && (
        <div className={`flex items-center gap-1 text-xs font-semibold px-2 py-1 rounded-full shrink-0 ${trendUp !== false ? "bg-success/10 text-success" : "bg-danger/10 text-danger"}`}>
          {trendUp !== false ? <TrendingUp size={11} /> : <TrendingDown size={11} />}
          {trend}
        </div>
      )}
    </div>
  );
}

const statusStyle: Record<string, string> = {
  Scheduled:  "bg-primary/10 text-primary",
  InProgress: "bg-warning/10 text-warning",
  Completed:  "bg-success/10 text-success",
  Cancelled:  "bg-danger/10 text-danger",
};

function Avatar({ name }: { name: string }) {
  const initials = name.split(" ").filter(Boolean).slice(-2).map(n => n[0]).join("").toUpperCase();
  const colors = [
    "bg-primary/10 text-primary", "bg-teal/10 text-teal",
    "bg-warning/20 text-warning", "bg-danger/10 text-danger", "bg-success/10 text-success",
  ];
  return (
    <div className={`w-8 h-8 rounded-full ${colors[name.charCodeAt(0) % colors.length]} flex items-center justify-center text-xs font-bold shrink-0`}>
      {initials}
    </div>
  );
}

export default async function DashboardPage() {
  const session = await auth();
  if (!session) return null;

  const token          = session.accessToken ?? "";
  const isAdmin        = hasAnyRole(session, ["admin"]);
  const isCashier      = hasAnyRole(session, ["cashier"]);
  const isReceptionist = hasAnyRole(session, ["receptionist"]);
  const isDoctor       = hasAnyRole(session, ["doctor"]);
  const isPharmacist   = hasAnyRole(session, ["pharmacist"]);
  const isPatient      = hasAnyRole(session, ["patient"]);

  // Fetch real data dari API
  let summary: BillsSummary | null = null;
  let patientCount = 0;
  let appointments: Appointment[] = [];

  try {
    const fetches = await Promise.allSettled([
      (isAdmin || isCashier)
        ? billingApi.summary(token)
        : Promise.resolve(null),
      (isAdmin || isReceptionist)
        ? patientsApi.list(token, 1, 1)
        : Promise.resolve(null),
      (isAdmin || isReceptionist || isDoctor || isCashier)
        ? appointmentsApi.list(token, { pageSize: "5", page: "1" })
        : Promise.resolve(null),
    ]);

    if (fetches[0].status === "fulfilled" && fetches[0].value)
      summary = fetches[0].value as BillsSummary;
    if (fetches[1].status === "fulfilled" && fetches[1].value)
      patientCount = (fetches[1].value as PaginatedResult<Patient>).totalCount;
    if (fetches[2].status === "fulfilled" && fetches[2].value)
      appointments = (fetches[2].value as PaginatedResult<Appointment>).items;
  } catch { /* services may be loading */ }

  const roleLabel = session.roles?.filter(
    r => !r.startsWith("default-") && r !== "offline_access" && r !== "uma_authorization"
  )[0] ?? "user";

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-bold text-gray-900">
            {isAdmin ? "Admin Dashboard" :
             isReceptionist ? "Receptionist Dashboard" :
             isDoctor ? "Doctor Dashboard" :
             isPharmacist ? "Pharmacist Dashboard" :
             isCashier ? "Cashier Dashboard" : "My Dashboard"}
          </h1>
          <p className="text-xs text-gray-400 mt-0.5">
            Welcome back, <span className="text-primary font-medium">{session.user?.name}</span>
            <span className="ml-2 capitalize text-gray-300">· {roleLabel}</span>
          </p>
        </div>
        {(isAdmin || isReceptionist) && (
          <Link href="/appointments/new"
            className="flex items-center gap-1.5 bg-primary text-white text-sm font-medium px-4 py-2 rounded-lg hover:opacity-90 transition-opacity">
            <Plus size={15} />
            New Appointment
          </Link>
        )}
      </div>

      {/* Stat cards — sesuai role */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        {(isAdmin || isReceptionist) && (
          <StatCard
            title="Total Patients"
            value={patientCount}
            sub="Pasien terdaftar"
            icon={Users}
            bg="bg-teal/10"
            iconColor="text-teal"
          />
        )}
        {(isAdmin || isCashier) && summary && (
          <>
            <StatCard
              title="Pending Bills"
              value={summary.pendingCount}
              sub={formatRupiah(summary.pendingAmount)}
              icon={ClipboardList}
              bg="bg-warning/10"
              iconColor="text-warning"
            />
            <StatCard
              title="Paid Today"
              value={summary.paidTodayCount}
              sub={formatRupiah(summary.paidTodayAmount)}
              icon={CreditCard}
              bg="bg-success/10"
              iconColor="text-success"
              trend="today"
              trendUp
            />
            <StatCard
              title="Total Today"
              value={summary.totalTodayCount}
              sub="tagihan hari ini"
              icon={CalendarDays}
              bg="bg-primary/10"
              iconColor="text-primary"
            />
          </>
        )}
        {isDoctor && (
          <StatCard
            title="Appointments"
            value={appointments.length}
            sub="terbaru"
            icon={CalendarDays}
            bg="bg-primary/10"
            iconColor="text-primary"
          />
        )}
        {isPharmacist && (
          <StatCard
            title="Prescriptions"
            value="—"
            sub="Cek halaman pharmacy"
            icon={ClipboardList}
            bg="bg-success/10"
            iconColor="text-success"
          />
        )}
        {isPatient && (
          <StatCard
            title="Appointments"
            value={appointments.length}
            sub="appointment saya"
            icon={CalendarDays}
            bg="bg-primary/10"
            iconColor="text-primary"
          />
        )}
        {/* Appointments count — semua role yang bisa lihat */}
        {(isAdmin || isReceptionist) && (
          <StatCard
            title="Appointments"
            value={appointments.length > 0 ? `${appointments.length}+` : "0"}
            sub="terbaru"
            icon={Stethoscope}
            bg="bg-primary/10"
            iconColor="text-primary"
          />
        )}
      </div>

      {/* Main grid */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-5">

        {/* Appointments table — tampil untuk semua role kecuali pharmacist */}
        {!isPharmacist && (
          <div className="xl:col-span-2 bg-white rounded-2xl border border-gray-100 shadow-card overflow-hidden">
            <div className="flex items-center justify-between px-5 py-4 border-b border-gray-50">
              <h2 className="text-sm font-bold text-gray-800">Recent Appointments</h2>
              <Link href="/appointments" className="text-xs text-primary font-medium hover:underline">View All</Link>
            </div>
            {appointments.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-50">
                      <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wide">Doctor</th>
                      <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wide">Jadwal</th>
                      <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wide">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-50">
                    {appointments.map(a => (
                      <tr key={a.id} className="hover:bg-gray-50/60 transition-colors">
                        <td className="px-5 py-3">
                          <div className="flex items-center gap-2.5">
                            <Avatar name={a.doctorName} />
                            <p className="text-xs font-semibold text-gray-800">{a.doctorName}</p>
                          </div>
                        </td>
                        <td className="px-5 py-3 text-xs text-gray-500">{formatDateTime(a.scheduledAt)}</td>
                        <td className="px-5 py-3">
                          <span className={`px-2.5 py-1 rounded-full text-[11px] font-semibold ${statusStyle[a.status] ?? "bg-gray-100 text-gray-500"}`}>
                            {a.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="px-5 py-12 text-center text-gray-400 text-sm">
                Belum ada appointment.
              </div>
            )}
          </div>
        )}

        {/* Sidebar kanan */}
        <div className={`flex flex-col gap-5 ${isPharmacist ? "xl:col-span-3" : ""}`}>

          {/* Upcoming 3 */}
          {appointments.length > 0 && !isPharmacist && (
            <div className="bg-white rounded-2xl border border-gray-100 shadow-card">
              <div className="flex items-center justify-between px-5 py-4 border-b border-gray-50">
                <h2 className="text-sm font-bold text-gray-800">Upcoming</h2>
                <Link href="/appointments" className="text-xs text-primary font-medium hover:underline">View All</Link>
              </div>
              <div className="divide-y divide-gray-50">
                {appointments.slice(0, 3).map(a => (
                  <Link key={a.id} href={`/appointments/${a.id}`}
                    className="flex items-center gap-3 px-5 py-3 hover:bg-gray-50/60 transition-colors">
                    <Avatar name={a.doctorName} />
                    <div className="flex-1 min-w-0">
                      <p className="text-xs font-semibold text-gray-800 truncate">{a.doctorName}</p>
                      <p className="text-[11px] text-gray-400">{formatDateTime(a.scheduledAt)}</p>
                    </div>
                    <StatusBadge status={a.status} />
                  </Link>
                ))}
              </div>
            </div>
          )}

          {/* Quick actions */}
          <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-5">
            <h2 className="text-sm font-bold text-gray-800 mb-4">Quick Actions</h2>
            <div className="grid grid-cols-2 gap-3">
              {(isAdmin || isReceptionist) && (
                <Link href="/patients/new"
                  className="flex flex-col items-center gap-2 p-3 rounded-xl bg-teal/5 hover:bg-teal/10 transition-colors">
                  <div className="w-9 h-9 rounded-xl bg-teal flex items-center justify-center">
                    <Users size={16} className="text-white" />
                  </div>
                  <span className="text-[11px] font-medium text-gray-600 text-center">New Patient</span>
                </Link>
              )}
              {(isAdmin || isReceptionist) && (
                <Link href="/appointments/new"
                  className="flex flex-col items-center gap-2 p-3 rounded-xl bg-primary/5 hover:bg-primary/10 transition-colors">
                  <div className="w-9 h-9 rounded-xl bg-primary flex items-center justify-center">
                    <CalendarDays size={16} className="text-white" />
                  </div>
                  <span className="text-[11px] font-medium text-gray-600 text-center">Appointment</span>
                </Link>
              )}
              {(isAdmin || isCashier) && (
                <Link href="/billing"
                  className="flex flex-col items-center gap-2 p-3 rounded-xl bg-warning/5 hover:bg-warning/10 transition-colors">
                  <div className="w-9 h-9 rounded-xl bg-warning flex items-center justify-center">
                    <CreditCard size={16} className="text-white" />
                  </div>
                  <span className="text-[11px] font-medium text-gray-600 text-center">Billing</span>
                </Link>
              )}
              {(isAdmin || isPharmacist) && (
                <Link href="/pharmacy"
                  className="flex flex-col items-center gap-2 p-3 rounded-xl bg-success/5 hover:bg-success/10 transition-colors">
                  <div className="w-9 h-9 rounded-xl bg-success flex items-center justify-center">
                    <ClipboardList size={16} className="text-white" />
                  </div>
                  <span className="text-[11px] font-medium text-gray-600 text-center">Pharmacy</span>
                </Link>
              )}
              {(isPatient || isDoctor) && (
                <Link href="/appointments"
                  className="flex flex-col items-center gap-2 p-3 rounded-xl bg-primary/5 hover:bg-primary/10 transition-colors">
                  <div className="w-9 h-9 rounded-xl bg-primary flex items-center justify-center">
                    <CalendarDays size={16} className="text-white" />
                  </div>
                  <span className="text-[11px] font-medium text-gray-600 text-center">Appointments</span>
                </Link>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
