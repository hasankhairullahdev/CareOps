"use client";
import { signIn } from "next-auth/react";
import { useEffect } from "react";

export default function LoginPage() {
  useEffect(() => {
    signIn("keycloak", { callbackUrl: "/dashboard" });
  }, []);

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-900">
      <div className="text-center">
        <div className="text-white text-2xl font-semibold mb-2">CareOps</div>
        <div className="text-slate-400 text-sm">Redirecting to login...</div>
        <div className="mt-4 w-8 h-8 border-2 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto" />
      </div>
    </div>
  );
}
