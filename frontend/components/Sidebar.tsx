"use client";
import { useSession, signOut } from "next-auth/react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { hasAnyRole, type UserRole } from "@/lib/roles";
import {
  LayoutDashboard, Users, Calendar, Pill, Receipt, Settings, LogOut,
} from "lucide-react";
import { cn } from "@/lib/utils";

const navItems = [
  { href: "/dashboard",    label: "Dashboard",    icon: LayoutDashboard, roles: ["admin","receptionist","doctor","pharmacist","cashier","patient"] as UserRole[] },
  { href: "/patients",     label: "Patients",     icon: Users,           roles: ["admin","receptionist","doctor","pharmacist","cashier"] as UserRole[] },
  { href: "/appointments", label: "Appointments", icon: Calendar,        roles: ["admin","receptionist","doctor","cashier","patient"] as UserRole[] },
  { href: "/pharmacy",     label: "Pharmacy",     icon: Pill,            roles: ["admin","pharmacist"] as UserRole[] },
  { href: "/billing",      label: "Billing",      icon: Receipt,         roles: ["admin","cashier","patient"] as UserRole[] },
  { href: "/admin",        label: "Admin",        icon: Settings,        roles: ["admin"] as UserRole[] },
];

export default function Sidebar() {
  const { data: session } = useSession();
  const pathname = usePathname();

  const visible = navItems.filter(item => hasAnyRole(session, item.roles));

  return (
    <aside className="w-60 min-h-screen bg-[#1e293b] flex flex-col text-white shrink-0">
      {/* Logo */}
      <div className="px-6 py-5 border-b border-slate-700">
        <span className="text-xl font-bold tracking-tight">CareOps</span>
      </div>

      {/* User info */}
      <div className="px-6 py-4 border-b border-slate-700">
        <p className="text-sm font-medium truncate">{session?.user?.name}</p>
        <p className="text-xs text-slate-400 capitalize mt-0.5">
          {session?.roles?.filter(r => !r.startsWith("default-") && r !== "offline_access" && r !== "uma_authorization")[0] ?? "user"}
        </p>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-3 py-4 space-y-1">
        {visible.map(item => {
          const Icon = item.icon;
          const active = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors",
                active
                  ? "bg-blue-600 text-white"
                  : "text-slate-300 hover:bg-slate-700 hover:text-white"
              )}
            >
              <Icon size={18} />
              {item.label}
            </Link>
          );
        })}
      </nav>

      {/* Sign out */}
      <div className="px-3 pb-4">
        <button
          onClick={() => signOut({ callbackUrl: "/login" })}
          className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm text-slate-400 hover:text-white hover:bg-slate-700 w-full transition-colors"
        >
          <LogOut size={18} />
          Sign out
        </button>
      </div>
    </aside>
  );
}
