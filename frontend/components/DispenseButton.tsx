"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { pharmacyApi } from "@/lib/api";
import { Pill, CheckCircle } from "lucide-react";

export default function DispenseButton({ prescriptionId, token }: {
  prescriptionId: string; token: string;
}) {
  const router  = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError]     = useState<string | null>(null);
  const [done, setDone]       = useState(false);

  const dispense = async () => {
    setLoading(true);
    setError(null);
    try {
      await pharmacyApi.dispense(token, prescriptionId);
      setDone(true);
      setTimeout(() => router.push("/pharmacy"), 1500);
    } catch (e) {
      setError((e as Error).message);
      setLoading(false);
    }
  };

  if (done) return (
    <div className="flex items-center gap-2 text-success font-medium text-sm">
      <CheckCircle size={16} />
      Berhasil didispense! Mengalihkan...
    </div>
  );

  return (
    <div className="space-y-2">
      <button
        onClick={dispense}
        disabled={loading}
        className="flex items-center gap-2 bg-success text-white text-sm font-semibold px-5 py-2.5 rounded-lg hover:opacity-90 disabled:opacity-60 transition-colors"
      >
        <Pill size={15} />
        {loading ? "Memproses..." : "Dispense Resep Ini"}
      </button>
      {error && <p className="text-xs text-danger">{error}</p>}
    </div>
  );
}
