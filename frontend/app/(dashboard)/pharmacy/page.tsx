import { auth } from "@/lib/auth";
import { pharmacyApi } from "@/lib/api";
import type { InventoryResult, PaginatedResult, Prescription } from "@/lib/types";
import { formatRupiah } from "@/lib/utils";
import Link from "next/link";
import StatusBadge from "@/components/StatusBadge";

export default async function PharmacyPage() {
  const session = await auth();
  const token = session?.accessToken ?? "";

  let inventory: InventoryResult | null = null;
  let prescriptions: { items: Prescription[]; totalCount: number } | null = null;

  try {
    [inventory, prescriptions] = await Promise.all([
      pharmacyApi.inventory(token) as Promise<InventoryResult>,
      pharmacyApi.pendingPrescriptions(token) as Promise<{ items: Prescription[]; totalCount: number }>,
    ]);
  } catch { }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Pharmacy</h1>

      {/* Tabs — simple CSS tabs */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">

        {/* Resep Pending */}
        <div>
          <h2 className="text-base font-semibold text-gray-700 mb-3">
            Resep Pending
            {prescriptions && <span className="ml-2 px-2 py-0.5 bg-yellow-100 text-yellow-700 rounded-full text-xs">{prescriptions.totalCount}</span>}
          </h2>
          <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200">
                  <th className="text-left px-4 py-3 font-medium text-gray-600">ID Resep</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Item</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Status</th>
                  <th className="px-4 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {prescriptions?.items.map((p) => (
                  <tr key={p.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-mono text-xs text-gray-500">{p.id.slice(0, 8)}…</td>
                    <td className="px-4 py-3 text-gray-700">{p.items.length} item(s)</td>
                    <td className="px-4 py-3"><StatusBadge status={p.status} /></td>
                    <td className="px-4 py-3">
                      <Link href={`/pharmacy/dispense/${p.id}`} className="text-blue-600 hover:underline text-xs">Dispense</Link>
                    </td>
                  </tr>
                ))}
                {!prescriptions?.items.length && (
                  <tr><td colSpan={4} className="px-4 py-8 text-center text-gray-400">Tidak ada resep pending.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Inventory */}
        <div>
          <h2 className="text-base font-semibold text-gray-700 mb-3">
            Inventory
            {inventory && inventory.lowStockCount > 0 && (
              <span className="ml-2 px-2 py-0.5 bg-red-100 text-red-700 rounded-full text-xs">{inventory.lowStockCount} low stock</span>
            )}
          </h2>
          <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200">
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Obat</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Stok</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Harga</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Kadaluarsa</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {inventory?.items.map((m) => (
                  <tr key={m.id} className={`hover:bg-gray-50 ${m.isLowStock ? "bg-red-50" : ""}`}>
                    <td className="px-4 py-3">
                      <div className="font-medium">{m.name}</div>
                      <div className="text-xs text-gray-400">{m.category}</div>
                    </td>
                    <td className="px-4 py-3">
                      <span className={m.isLowStock ? "text-red-600 font-medium" : "text-gray-700"}>
                        {m.stockQuantity} {m.unit}
                      </span>
                      {m.isLowStock && <span className="ml-1 text-xs text-red-500">⚠ Low</span>}
                    </td>
                    <td className="px-4 py-3 text-gray-600">{formatRupiah(m.price)}</td>
                    <td className="px-4 py-3 text-gray-500 text-xs">{m.expiryDate}</td>
                  </tr>
                ))}
                {!inventory?.items.length && (
                  <tr><td colSpan={4} className="px-4 py-8 text-center text-gray-400">Tidak ada data inventory.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
