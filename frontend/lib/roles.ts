import { auth } from "@/lib/auth";
import type { Session } from "next-auth";

export type UserRole = "admin" | "receptionist" | "doctor" | "pharmacist" | "cashier" | "patient";

export function hasRole(session: Session | null, role: UserRole): boolean {
  if (!session) return false;
  return (session.roles ?? []).includes(role);
}

export function hasAnyRole(session: Session | null, roles: UserRole[]): boolean {
  if (!session) return false;
  return roles.some((r) => (session.roles ?? []).includes(r));
}

export async function getServerSession() {
  return auth();
}
