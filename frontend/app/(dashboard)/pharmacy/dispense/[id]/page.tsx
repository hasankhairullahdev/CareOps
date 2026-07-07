import { auth } from "@/lib/auth";
import { pharmacyApi } from "@/lib/api";
import type { Prescription } from "@/lib/types";
import { formatDateTime, formatRupiah } from "@/lib/utils";
import Link from "next/link";
import DispenseButton from "@/components/DispenseButton";
import { ArrowLeft, Pill, Hash, Info } from "lucide-react";

export default async function DispensePage({ params }: { params: { id: string } }) {
  const session = await auth();
  const token = session?.accessToken ?? "";

  let prescription: Prescription | null = null;
  let error: string | null = null;

  try {
    prescription = await pharmacyApi.getPrescription(token, params.id) as Prescription;
  } catch (e) {
    error = (e as Error).message;
  }

  if (error) return (
    <div className="p-8 text-center text-red-500 bg-white rounded-2xl border border-red-100">{error}</div>
  );
  if (!prescription) return null;

  const canDispense = prescription.status === "Pending" &&
    (session?.roles?.includes("pharmacist") || session?.roles?.includes("admin"));

  return (
    <div className="max-w-2xl space-y-5">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/pharmacy" className="w-8 h-8 rounded-lg bg-white border border-gray-200 flex items-center justify-center hover:bg-gray-50 transition-colors">
          <ArrowLeft size={15} className="text-gray-500" />
        </Link>
        <div className="flex-1">
          <h1 className="text-lg font-bold text-gray-900">Dispense Resep</h1>
          <p className="text-xs text-gray-400 font-mono">{prescription.id.slice(0, 18)}…</p>
        </div>
        <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
          prescription.status === "Pending"   ? "bg-warning/10 text-warning" :
          prescription.status === "Dispensed" ? "bg-success/10 text-success" :
          "bg-danger/10 text-danger"
        }`}>
          {prescription.status}
        </span>
      </div>

      {/* Info */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-card p-5 space-y-3">
        <h2 className="text-sm font-bold text-gray-800 pb-2 border-b border-gray-100">Info Resep</h2>
        <div className="grid grid-cols-2 gap-3 text-sm">
          <div>
            <p className="text-[10px] text-gray-400 uppercase tracking-wide">Patient ID</p>
            <p className="text-xs text-gray-700 font-medium font-mono">{prescription.patientId.slice(0, 18)}…</p>
          </div>
          <div>
            <p className="text-[10px] text-gray-400 uppercase tracking-wide">Dibuat</p>
            <p className="text-xs text-gray-700 font-medium">{formatDateTime(prescription.createdAt)}</p>
          </div>
          {prescription.dispensedAt && (
            <div>
              <p className="text-[10px] text-gray-400 uppercase tracking-wide">Dispensed</p>
              <p className="text-xs text-gray-700 font-medium">{formatDateTime(prescription.dispensedAt)}</p>
            </div>
          )}
        </div>
      </div>

      {/* Items */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-card overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-100 flex items-center gap-2">
          <Pill size={15} className="text-primary" />
          <h2 className="text-sm font-bold text-gray-800">Daftar Obat</h2>
          <span className="ml-auto text-xs bg-primary/10 text-primary px-2 py-0.5 rounded-full font-semibold">{prescription.items.length} item</span>
        </div>
        <div className="divide-y divide-gray-50">
          {prescription.items.map((item, i) => (
            <div key={item.id} className="px-5 py-4 flex items-start gap-4">
              <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary text-xs font-bold shrink-0">
                {i + 1}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold text-gray-800">{item.medicineName}</p>
                <div className="flex flex-wrap gap-x-4 gap-y-1 mt-1">
                  <span className="flex items-center gap-1 text-xs text-gray-500">
                    <Hash size={11} /> Qty: <strong>{item.quantity}</strong>
                  </span>
                  <span className="flex items-center gap-1 text-xs text-gray-500">
                    <Info size={11} /> {item.dosage}
                  </span>
                </div>
                <p className="text-xs text-gray-400 mt-1 italic">{item.instructions}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Dispense action */}
        {canDispense && (
          <div className="px-5 py-4 border-t border-gray-100 bg-gray-50/50">
            <DispenseButton prescriptionId={prescription.id} token={token} />
          </div>
        )}
        {prescription.status !== "Pending" && (
          <div className="px-5 py-4 border-t border-gray-100 text-center text-sm text-gray-400">
            Resep ini sudah <strong>{prescription.status}</strong>.
          </div>
        )}
      </div>
    </div>
  );
}
