import { auth } from "@/lib/auth";
import { hasAnyRole } from "@/lib/roles";
import { billingApi, patientsApi } from "@/lib/api";
import { formatRupiah } from "@/lib/utils";
import type { BillsSummary, PaginatedResult, Patient } from "@/lib/types";
import Link from "next/link";
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
        <div className={`flex items-center gap-1 text-xs font-semibold px-2 py-1 rounded-full ${trendUp !== false ? "bg-success/10 text-success" : "bg-danger/10 text-danger"}`}>
          {trendUp !== false ? <TrendingUp size={11} /> : <TrendingDown size={11} />}
          {trend}
        </div>
      )}
    </div>
  );
}

const recentAppointments = [
  { doctor: "Dr. Ahmad Fauzi",     specialty: "Cardiologist",   patient: "Budi Santoso",   time: "09:00 AM", status: "Confirmed" },
  { doctor: "Dr. Siti Rahayu",     specialty: "Pediatrician",   patient: "Rina Kusuma",    time: "10:30 AM", status: "Pending" },
  { doctor: "Dr. Hendra Wijaya",   specialty: "Neurologist",    patient: "Agus Purnomo",   time: "11:00 AM", status: "Cancelled" },
  { doctor: "Dr. Maya Indah",      specialty: "Dermatologist",  patient: "Dewi Lestari",   time: "01:00 PM", status: "Confirmed" },
  { doctor: "Dr. Rizky Pratama",   specialty: "Orthopedics",    patient: "Joko Widodo",    time: "02:30 PM", status: "Confirmed" },
];

const statusStyle: Record<string, string> = {
  Confirmed:  "bg-success/10 text-success",
  Pending:    "bg-warning/10 text-warning",
  Cancelled:  "bg-danger/10 text-danger",
};

function Avatar({ name }: { name: string }) {
  const initials = name.split(" ").filter(Boolean).slice(-2).map(n => n[0]).join("").toUpperCase();
  const colors = ["bg-primary/10 text-primary", "bg-teal/10 text-teal", "bg-warning/20 text-warning", "bg-danger/10 text-danger", "bg-success/10 text-success"];
  const idx = name.charCodeAt(0) % colors.length;
  return (
    <div className={`w-8 h-8 rounded-full ${colors[idx]} flex items-center justify-center text-xs font-bold shrink-0`}>
      {initials}
    </div>
  );
}

export default async function DashboardPage() {
  const session = await auth();
  if (!session) return null;

  const token        = session.accessToken ?? "";
  const isAdmin        = hasAnyRole(session, ["admin"]);
  const isCashier      = hasAnyRole(session, ["cashier"]);
  const isReceptionist = hasAnyRole(session, ["receptionist"]);

  let summary: BillsSummary | null = null;
  let patientCount = 0;

  try {
    if (isAdmin || isCashier)      summary = await billingApi.summary(token) as BillsSummary;
    if (isAdmin || isReceptionist) {
      const p = await patientsApi.list(token, 1, 1) as PaginatedResult<Patient>;
      patientCount = p.totalCount;
    }
  } catch { /* services may still be starting */ }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-bold text-gray-900">Admin Dashboard</h1>
          <p className="text-xs text-gray-400 mt-0.5">Welcome back, <span className="text-primary font-medium">{session.user?.name}</span></p>
        </div>
        <div className="flex items-center gap-2">
          {(isAdmin || isReceptionist) && (
            <Link href="/appointments/new"
              className="flex items-center gap-1.5 bg-primary text-white text-sm font-medium px-4 py-2 rounded-lg hover:bg-primary-hover transition-colors">
              <Plus size={15} />
              New Appointment
            </Link>
          )}
        </div>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <StatCard
          title="Doctors"
          value="247"
          sub="in last 7 Days"
          icon={Stethoscope}
          bg="bg-primary/10"
          iconColor="text-primary"
          trend="+95%"
          trendUp
        />
        {(isAdmin || isReceptionist) ? (
          <StatCard
            title="Patients"
            value={patientCount || 4178}
            sub="terdaftar"
            icon={Users}
            bg="bg-teal/10"
            iconColor="text-teal"
            trend="+25%"
            trendUp
          />
        ) : (
          <StatCard
            title="Patients"
            value={4178}
            sub="in last 7 Days"
            icon={Users}
            bg="bg-teal/10"
            iconColor="text-teal"
            trend="+25%"
            trendUp
          />
        )}
        <StatCard
          title="Appointments"
          value={summary?.totalTodayCount ?? 12178}
          sub="in last 7 Days"
          icon={CalendarDays}
          bg="bg-danger/10"
          iconColor="text-danger"
          trend="-15%"
          trendUp={false}
        />
        <StatCard
          title="Revenue"
          value={summary ? formatRupiah(summary.paidTodayAmount) : "Rp 551.240"}
          sub="in last 7 Days"
          icon={CreditCard}
          bg="bg-success/10"
          iconColor="text-success"
          trend="+25%"
          trendUp
        />
      </div>

      {/* Main grid */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-5">

        {/* Recent Appointments table — 2/3 width */}
        <div className="xl:col-span-2 bg-white rounded-2xl border border-gray-100 shadow-card overflow-hidden">
          <div className="flex items-center justify-between px-5 py-4 border-b border-gray-50">
            <h2 className="text-sm font-bold text-gray-800">All Appointments</h2>
            <Link href="/appointments" className="text-xs text-primary font-medium hover:underline">View All</Link>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-50">
                  <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wide">Doctor</th>
                  <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wide">Patient</th>
                  <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wide">Time</th>
                  <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase tracking-wide">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {recentAppointments.map((a, i) => (
                  <tr key={i} className="hover:bg-gray-50/60 transition-colors">
                    <td className="px-5 py-3">
                      <div className="flex items-center gap-2.5">
                        <Avatar name={a.doctor} />
                        <div>
                          <p className="text-xs font-semibold text-gray-800">{a.doctor}</p>
                          <p className="text-[11px] text-gray-400">{a.specialty}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-5 py-3">
                      <div className="flex items-center gap-2.5">
                        <Avatar name={a.patient} />
                        <p className="text-xs font-medium text-gray-700">{a.patient}</p>
                      </div>
                    </td>
                    <td className="px-5 py-3 text-xs text-gray-500">{a.time}</td>
                    <td className="px-5 py-3">
                      <span className={`px-2.5 py-1 rounded-full text-[11px] font-semibold ${statusStyle[a.status]}`}>
                        {a.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Upcoming + Quick actions — 1/3 width */}
        <div className="flex flex-col gap-5">

          {/* Upcoming appointments */}
          <div className="bg-white rounded-2xl border border-gray-100 shadow-card">
            <div className="flex items-center justify-between px-5 py-4 border-b border-gray-50">
              <h2 className="text-sm font-bold text-gray-800">Upcoming</h2>
              <Link href="/appointments" className="text-xs text-primary font-medium hover:underline">View All</Link>
            </div>
            <div className="divide-y divide-gray-50">
              {recentAppointments.slice(0, 3).map((a, i) => (
                <div key={i} className="flex items-center gap-3 px-5 py-3 hover:bg-gray-50/60 transition-colors">
                  <Avatar name={a.doctor} />
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-semibold text-gray-800 truncate">{a.doctor}</p>
                    <p className="text-[11px] text-gray-400">{a.time}</p>
                  </div>
                  <span className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${statusStyle[a.status]}`}>
                    {a.status}
                  </span>
                </div>
              ))}
            </div>
          </div>

          {/* Quick actions */}
          <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-5">
            <h2 className="text-sm font-bold text-gray-800 mb-4">Quick Actions</h2>
            <div className="grid grid-cols-2 gap-3">
              {(isAdmin || isReceptionist) && (
                <Link href="/patients/new"
                  className="flex flex-col items-center gap-2 p-3 rounded-xl bg-primary/5 hover:bg-primary/10 transition-colors group">
                  <div className="w-9 h-9 rounded-xl bg-primary flex items-center justify-center">
                    <Users size={16} className="text-white" />
                  </div>
                  <span className="text-[11px] font-medium text-gray-600 text-center">New Patient</span>
                </Link>
              )}
              {(isAdmin || isReceptionist) && (
                <Link href="/appointments/new"
                  className="flex flex-col items-center gap-2 p-3 rounded-xl bg-teal/5 hover:bg-teal/10 transition-colors">
                  <div className="w-9 h-9 rounded-xl bg-teal flex items-center justify-center">
                    <CalendarDays size={16} className="text-white" />
                  </div>
                  <span className="text-[11px] font-medium text-gray-600 text-center">Appointment</span>
                </Link>
              )}
              <Link href="/billing"
                className="flex flex-col items-center gap-2 p-3 rounded-xl bg-warning/5 hover:bg-warning/10 transition-colors">
                <div className="w-9 h-9 rounded-xl bg-warning flex items-center justify-center">
                  <CreditCard size={16} className="text-white" />
                </div>
                <span className="text-[11px] font-medium text-gray-600 text-center">Billing</span>
              </Link>
              <Link href="/pharmacy"
                className="flex flex-col items-center gap-2 p-3 rounded-xl bg-success/5 hover:bg-success/10 transition-colors">
                <div className="w-9 h-9 rounded-xl bg-success flex items-center justify-center">
                  <ClipboardList size={16} className="text-white" />
                </div>
                <span className="text-[11px] font-medium text-gray-600 text-center">Pharmacy</span>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
