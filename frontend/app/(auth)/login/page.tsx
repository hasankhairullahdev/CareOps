"use client";
import { signIn, useSession } from "next-auth/react";
import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function LoginPage() {
  const { data: session, status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (status === "loading") return;
    if (status === "authenticated") {
      // Sudah login — langsung ke dashboard
      router.replace("/dashboard");
      return;
    }
    // Belum login — redirect ke Keycloak
    signIn("keycloak", { callbackUrl: "/dashboard" });
  }, [status, router]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#1e293b]">
      <div className="text-center">
        <div className="w-14 h-14 rounded-2xl bg-primary flex items-center justify-center mx-auto mb-4">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <path d="M12 2a3 3 0 0 0-3 3v1H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-2V5a3 3 0 0 0-3-3Z"/>
            <path d="M8 12h8M8 16h5"/>
          </svg>
        </div>
        <p className="text-white text-lg font-bold mb-1">CareOps</p>
        <p className="text-slate-400 text-sm mb-5">
          {status === "authenticated" ? "Redirecting..." : "Mengarahkan ke halaman login..."}
        </p>
        <div className="w-6 h-6 border-2 border-primary border-t-transparent rounded-full animate-spin mx-auto" />
      </div>
    </div>
  );
}
