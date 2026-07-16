"use client";
import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import Link from "next/link";
import { doctorsApi } from "@/lib/api";
import { Doctor, PaginatedDoctorsResult } from "@/lib/types";
import { hasRole } from "@/lib/roles";
import { UserCog, Plus, Search, Pencil, Trash2, CheckCircle2, XCircle } from "lucide-react";

export default function DoctorsPage() {
  const { data: session } = useSession();
  const [result, setResult] = useState<PaginatedDoctorsResult | null>(null);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<string | null>(null);

  const isAdmin = hasRole(session, "admin");

  async function load(searchTerm = search) {
    if (!session?.accessToken) return;
    setLoading(true);
    setError(null);
    try {
      const params: Record<string, string> = { pageSize: "50" };
      if (searchTerm) params.search = searchTerm;
      const data = await doctorsApi.list(session.accessToken as string, params) as PaginatedDoctorsResult;
      setResult(data);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Failed to load doctors");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { if (session?.accessToken) load(); }, [session]);

  async function handleDelete(id: string, name: string) {
    if (!confirm(`Delete ${name}? This cannot be undone.`)) return;
    setDeleting(id);
    try {
      await doctorsApi.delete(session!.accessToken as string, id);
      setResult(prev => prev ? { ...prev, items: prev.items.filter(d => d.id !== id), totalCount: prev.totalCount - 1 } : prev);
    } catch (e: unknown) {
      alert(e instanceof Error ? e.message : "Delete failed");
    } finally {
      setDeleting(null);
    }
  }

  return (
    <div className="p-6 space-y-5">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
            <UserCog size={20} className="text-primary" />
          </div>
          <div>
            <h1 className="text-lg font-bold text-gray-900">Doctors</h1>
            <p className="text-xs text-gray-400">Manage doctor records & schedules</p>
          </div>
        </div>
        {isAdmin && (
          <Link
            href="/doctors/new"
            className="flex items-center gap-1.5 bg-primary text-white text-sm px-4 py-2 rounded-lg hover:bg-primary/90 transition-colors"
          >
            <Plus size={15} />
            Add Doctor
          </Link>
        )}
      </div>

      {/* Search */}
      <div className="relative max-w-sm">
        <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/30"
          placeholder="Search by name, specialization…"
          value={search}
          onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === "Enter" && load(search)}
        />
      </div>

      {/* Content */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">{error}</div>
      )}

      {loading ? (
        <div className="text-sm text-gray-400 py-10 text-center">Loading…</div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/60">
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Name</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Specialization</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">License</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Schedule</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Contact</th>
                <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Status</th>
                {isAdmin && <th className="px-4 py-3" />}
              </tr>
            </thead>
            <tbody>
              {result?.items.length === 0 && (
                <tr>
                  <td colSpan={7} className="text-center py-10 text-gray-400">No doctors found.</td>
                </tr>
              )}
              {result?.items.map(doctor => (
                <tr key={doctor.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                  <td className="px-4 py-3 font-medium text-gray-900">{doctor.name}</td>
                  <td className="px-4 py-3 text-gray-600">{doctor.specialization}</td>
                  <td className="px-4 py-3 text-gray-500 font-mono text-xs">{doctor.licenseNumber}</td>
                  <td className="px-4 py-3 text-gray-600 text-xs">{doctor.schedule}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {doctor.phone && <div>{doctor.phone}</div>}
                    {doctor.email && <div className="text-primary/70">{doctor.email}</div>}
                  </td>
                  <td className="px-4 py-3">
                    {doctor.isActive ? (
                      <span className="flex items-center gap-1 text-xs text-emerald-600 font-medium">
                        <CheckCircle2 size={13} /> Active
                      </span>
                    ) : (
                      <span className="flex items-center gap-1 text-xs text-gray-400 font-medium">
                        <XCircle size={13} /> Inactive
                      </span>
                    )}
                  </td>
                  {isAdmin && (
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2 justify-end">
                        <Link
                          href={`/doctors/${doctor.id}/edit`}
                          className="p-1.5 text-gray-400 hover:text-primary hover:bg-primary/10 rounded-lg transition-colors"
                        >
                          <Pencil size={14} />
                        </Link>
                        <button
                          onClick={() => handleDelete(doctor.id, doctor.name)}
                          disabled={deleting === doctor.id}
                          className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50"
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
          {result && (
            <div className="px-4 py-3 border-t border-gray-100 text-xs text-gray-400">
              {result.totalCount} doctor{result.totalCount !== 1 ? "s" : ""} total
            </div>
          )}
        </div>
      )}
    </div>
  );
}
