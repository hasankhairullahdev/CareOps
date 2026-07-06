import { auth } from "@/lib/auth";
import { billingApi } from "@/lib/api";
import type { Bill, BillsSummary, PaginatedResult } from "@/lib/types";
import { formatRupiah, formatDate } from "@/lib/utils";
import Link from "next/link";
import StatusBadge from "@/components/StatusBadge";

export default async function BillingPage({
  searchParams,
}: {
  searchParams: { patientId?: string; page?: string };
}) {
  const session = await auth();
  const token = session?.accessToken ?? "";
  const page = Number(searchParams.page ?? 1);

  let result: PaginatedResult<Bill> | null = null;
  let summary: BillsSummary | null = null;
  let error: string | null = null;

  try {
    summary = await billingApi.summary(token) as BillsSummary;
    if (searchParams.patientId) {
      result = await billingApi.list(token, searchParams.patientId, page) as PaginatedResult<Bill>;
    }
  } catch (e) {
    error = (e as Error).message;
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Billing</h1>

      {/* Summary cards */}
      {summary && (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500">Pending</p>
            <p className="text-xl font-bold text-yellow-600">{summary.pendingCount}</p>
            <p className="text-xs text-gray-400">{formatRupiah(summary.pendingAmount)}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <p className="text-xs text-gray-500">Dibayar Hari Ini</p>
            <p className="text-xl font-bold text-green-600">{summary.paidTodayCount}</p>
            <p className="text-xs text-gray-400">{formatRupiah(summary.paidTodayAmount)}</p>
          </div>
        </div>
      )}

      {/* Search by patientId */}
      <form method="GET" className="mb-6 flex gap-2">
        <input
          name="patientId"
          defaultValue={searchParams.patientId}
          placeholder="Masukkan Patient ID untuk lihat tagihan..."
          className="flex-1 max-w-md px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button type="submit" className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700">Cari</button>
      </form>

      {error && <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg p-4 mb-4 text-sm">{error}</div>}

      {result && (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="text-left px-4 py-3 font-medium text-gray-600">ID Tagihan</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Status</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Total</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Dibuat</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {result.items.map((b) => (
                <tr key={b.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-mono text-xs text-gray-500">{b.id.slice(0, 8)}…</td>
                  <td className="px-4 py-3"><StatusBadge status={b.status} /></td>
                  <td className="px-4 py-3 font-medium">{formatRupiah(b.totalAmount)}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">{formatDate(b.createdAt)}</td>
                  <td className="px-4 py-3">
                    <Link href={`/billing/${b.id}`} className="text-blue-600 hover:underline text-xs">Detail</Link>
                  </td>
                </tr>
              ))}
              {result.items.length === 0 && (
                <tr><td colSpan={5} className="px-4 py-8 text-center text-gray-400">Tidak ada tagihan.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {!searchParams.patientId && !error && (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center text-gray-400">
          Masukkan Patient ID untuk melihat daftar tagihan.
        </div>
      )}
    </div>
  );
}
