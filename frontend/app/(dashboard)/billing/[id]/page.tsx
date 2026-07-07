import { auth } from "@/lib/auth";
import { billingApi } from "@/lib/api";
import type { Bill } from "@/lib/types";
import { formatRupiah, formatDateTime } from "@/lib/utils";
import Link from "next/link";
import StatusBadge from "@/components/StatusBadge";
import BillActions from "@/components/BillActions";
import { ArrowLeft, Receipt, Calendar, User, FileText } from "lucide-react";

export default async function BillDetailPage({ params }: { params: { id: string } }) {
  const session = await auth();
  const token = session?.accessToken ?? "";

  let bill: Bill | null = null;
  let error: string | null = null;

  try {
    bill = await billingApi.get(token, params.id) as Bill;
  } catch (e) {
    error = (e as Error).message;
  }

  if (error) return (
    <div className="p-8 text-center text-red-500 bg-white rounded-2xl border border-red-100">{error}</div>
  );
  if (!bill) return null;

  const roles = session?.roles ?? [];
  const canIssue  = bill.status === "Draft"   && (roles.includes("cashier") || roles.includes("admin"));
  const canPay    = bill.status === "Issued"  && (roles.includes("cashier") || roles.includes("admin"));
  const canCancel = ["Draft", "Issued"].includes(bill.status) && (roles.includes("cashier") || roles.includes("admin"));

  return (
    <div className="max-w-3xl space-y-5">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/billing" className="w-8 h-8 rounded-lg bg-white border border-gray-200 flex items-center justify-center hover:bg-gray-50 transition-colors">
          <ArrowLeft size={15} className="text-gray-500" />
        </Link>
        <div className="flex-1">
          <h1 className="text-lg font-bold text-gray-900">Detail Tagihan</h1>
          <p className="text-xs text-gray-400 font-mono">{bill.id.slice(0, 18)}…</p>
        </div>
        <StatusBadge status={bill.status} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        {/* Info */}
        <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-5 space-y-3">
          <h2 className="text-sm font-bold text-gray-800 pb-2 border-b border-gray-100">Info Tagihan</h2>
          {[
            { icon: User,     label: "Patient ID",     value: bill.patientId.slice(0,12) + "…" },
            { icon: FileText, label: "Appointment ID", value: bill.appointmentId.slice(0,12) + "…" },
            { icon: Calendar, label: "Dibuat",         value: formatDateTime(bill.createdAt) },
            { icon: Calendar, label: "Diterbitkan",    value: bill.issuedAt ? formatDateTime(bill.issuedAt) : "—" },
            { icon: Calendar, label: "Dibayar",        value: bill.paidAt   ? formatDateTime(bill.paidAt)   : "—" },
          ].map(({ icon: Icon, label, value }) => (
            <div key={label} className="flex items-start gap-2.5">
              <div className="w-6 h-6 rounded-md bg-gray-100 flex items-center justify-center shrink-0">
                <Icon size={11} className="text-gray-500" />
              </div>
              <div>
                <p className="text-[10px] text-gray-400 uppercase">{label}</p>
                <p className="text-xs text-gray-700 font-medium">{value}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Line items + total */}
        <div className="md:col-span-2 bg-white rounded-2xl border border-gray-100 shadow-card overflow-hidden">
          <div className="px-5 py-4 border-b border-gray-100">
            <h2 className="text-sm font-bold text-gray-800">Rincian Tagihan</h2>
          </div>
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-50">
                <th className="text-left px-5 py-3 text-xs font-semibold text-gray-400 uppercase">Deskripsi</th>
                <th className="text-right px-5 py-3 text-xs font-semibold text-gray-400 uppercase">Qty</th>
                <th className="text-right px-5 py-3 text-xs font-semibold text-gray-400 uppercase">Harga</th>
                <th className="text-right px-5 py-3 text-xs font-semibold text-gray-400 uppercase">Total</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {bill.lineItems.map(item => (
                <tr key={item.id} className="hover:bg-gray-50/60">
                  <td className="px-5 py-3 text-gray-700 text-xs">{item.description}</td>
                  <td className="px-5 py-3 text-gray-500 text-xs text-right">{item.quantity}</td>
                  <td className="px-5 py-3 text-gray-500 text-xs text-right">{formatRupiah(item.unitPrice)}</td>
                  <td className="px-5 py-3 text-gray-800 font-medium text-xs text-right">{formatRupiah(item.amount)}</td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr className="border-t-2 border-gray-200">
                <td colSpan={3} className="px-5 py-4 text-sm font-bold text-gray-800 text-right">Total</td>
                <td className="px-5 py-4 text-base font-bold text-primary text-right">{formatRupiah(bill.totalAmount)}</td>
              </tr>
            </tfoot>
          </table>

          {/* Actions */}
          {(canIssue || canPay || canCancel) && (
            <div className="px-5 py-4 border-t border-gray-100 flex gap-2">
              <BillActions billId={bill.id} canIssue={canIssue} canPay={canPay} canCancel={canCancel} token={token} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
