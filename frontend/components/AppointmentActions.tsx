"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { appointmentsApi } from "@/lib/api";
import { CheckCircle, XCircle, Pill } from "lucide-react";
import Link from "next/link";

export default function AppointmentActions({
  appointmentId, patientId, canComplete, canCancel, canPrescribe, token,
}: {
  appointmentId: string; patientId: string;
  canComplete: boolean; canCancel: boolean; canPrescribe: boolean;
  token: string;
}) {
  const router = useRouter();
  const [loading, setLoading]     = useState<string | null>(null);
  const [error, setError]         = useState<string | null>(null);
  const [showCancel, setShowCancel] = useState(false);
  const [reason, setReason]       = useState("");

  const complete = async () => {
    setLoading("complete");
    setError(null);
    try {
      await appointmentsApi.complete(token, appointmentId);
      router.refresh();
    } catch (e) { setError((e as Error).message); }
    finally { setLoading(null); }
  };

  const cancel = async () => {
    setLoading("cancel");
    setError(null);
    try {
      await appointmentsApi.cancel(token, appointmentId, reason);
      router.refresh();
    } catch (e) { setError((e as Error).message); }
    finally { setLoading(null); setShowCancel(false); }
  };

  const btnBase = "flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg transition-colors disabled:opacity-60 w-full";

  return (
    <div className="space-y-2">
      {canComplete && (
        <button onClick={complete} disabled={!!loading} className={`${btnBase} bg-success text-white hover:opacity-90`}>
          <CheckCircle size={15} />
          {loading === "complete" ? "Memproses..." : "Selesaikan Appointment"}
        </button>
      )}
      {canPrescribe && (
        <Link href={`/appointments/${appointmentId}/prescriptions/new`}
          className={`${btnBase} bg-primary text-white hover:bg-primary-hover`}>
          <Pill size={15} />
          Buat Resep
        </Link>
      )}
      {canCancel && !showCancel && (
        <button onClick={() => setShowCancel(true)} className={`${btnBase} bg-danger/10 text-danger hover:bg-danger/20`}>
          <XCircle size={15} />
          Batalkan Appointment
        </button>
      )}
      {showCancel && (
        <div className="space-y-2 bg-danger/5 rounded-xl p-3 border border-danger/20">
          <textarea
            rows={2}
            value={reason}
            onChange={e => setReason(e.target.value)}
            placeholder="Alasan pembatalan..."
            className="w-full text-sm px-3 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-danger/30 resize-none"
          />
          <div className="flex gap-2">
            <button onClick={cancel} disabled={!!loading}
              className="flex-1 bg-danger text-white text-sm font-medium py-2 rounded-lg hover:opacity-90 disabled:opacity-60">
              {loading === "cancel" ? "Memproses..." : "Konfirmasi Batal"}
            </button>
            <button onClick={() => setShowCancel(false)}
              className="px-4 text-sm text-gray-500 bg-gray-100 rounded-lg hover:bg-gray-200">
              Kembali
            </button>
          </div>
        </div>
      )}
      {error && <p className="text-xs text-danger">{error}</p>}
    </div>
  );
}
