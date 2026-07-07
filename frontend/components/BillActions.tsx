"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { billingApi } from "@/lib/api";
import { CheckCircle, CreditCard, XCircle } from "lucide-react";

export default function BillActions({ billId, canIssue, canPay, canCancel, token }: {
  billId: string; canIssue: boolean; canPay: boolean; canCancel: boolean; token: string;
}) {
  const router = useRouter();
  const [loading, setLoading] = useState<string | null>(null);
  const [error, setError]     = useState<string | null>(null);

  const act = async (action: "issue" | "pay" | "cancel") => {
    setLoading(action);
    setError(null);
    try {
      if (action === "issue")  await billingApi.issue(token, billId);
      if (action === "pay")    await billingApi.pay(token, billId);
      if (action === "cancel") await billingApi.cancel(token, billId);
      router.refresh();
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(null);
    }
  };

  return (
    <div className="flex flex-wrap gap-2 w-full">
      {canIssue && (
        <button onClick={() => act("issue")} disabled={!!loading}
          className="flex items-center gap-1.5 px-4 py-2 bg-primary text-white text-sm font-medium rounded-lg hover:bg-primary-hover disabled:opacity-60 transition-colors">
          <CheckCircle size={14} />
          {loading === "issue" ? "Memproses..." : "Terbitkan"}
        </button>
      )}
      {canPay && (
        <button onClick={() => act("pay")} disabled={!!loading}
          className="flex items-center gap-1.5 px-4 py-2 bg-success text-white text-sm font-medium rounded-lg hover:opacity-90 disabled:opacity-60 transition-colors">
          <CreditCard size={14} />
          {loading === "pay" ? "Memproses..." : "Bayar"}
        </button>
      )}
      {canCancel && (
        <button onClick={() => act("cancel")} disabled={!!loading}
          className="flex items-center gap-1.5 px-4 py-2 bg-danger/10 text-danger text-sm font-medium rounded-lg hover:bg-danger/20 disabled:opacity-60 transition-colors">
          <XCircle size={14} />
          {loading === "cancel" ? "Memproses..." : "Batalkan"}
        </button>
      )}
      {error && <p className="w-full text-xs text-danger mt-1">{error}</p>}
    </div>
  );
}
