"use client";
import { useSession, signOut } from "next-auth/react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { hasAnyRole, type UserRole } from "@/lib/roles";
import {
  LayoutDashboard, Users, Calendar, Pill, Receipt,
  Settings, LogOut, Stethoscope, ChevronRight,
  UserCog, Tag, Truck, CreditCard, AlertCircle,
} from "lucide-react";
import { cn } from "@/lib/utils";

const navGroups = [
  {
    label: "Main Menu",
    items: [
      { href: "/dashboard",    label: "Dashboard",    icon: LayoutDashboard, roles: ["admin","receptionist","doctor","pharmacist","cashier","patient"] as UserRole[] },
    ],
  },
  {
    label: "Clinic",
    items: [
      { href: "/patients",     label: "Patients",     icon: Users,     roles: ["admin","receptionist","doctor","pharmacist","cashier"] as UserRole[] },
      { href: "/appointments", label: "Appointments", icon: Calendar,  roles: ["admin","receptionist","doctor","cashier","patient"] as UserRole[] },
      { href: "/doctors",      label: "Doctors",      icon: UserCog,   roles: ["admin","receptionist","doctor","cashier"] as UserRole[] },
      { href: "/pharmacy",     label: "Pharmacy",     icon: Pill,      roles: ["admin","pharmacist"] as UserRole[] },
    ],
  },
  {
    label: "Finance",
    items: [
      { href: "/billing",      label: "Billing",      icon: Receipt,   roles: ["admin","cashier","patient"] as UserRole[] },
    ],
  },
  {
    label: "Administration",
    items: [
      { href: "/admin/tariffs",         label: "Service Tariffs",   icon: Tag,        roles: ["admin"] as UserRole[] },
      { href: "/admin/payment-methods", label: "Payment Methods",   icon: CreditCard, roles: ["admin"] as UserRole[] },
      { href: "/admin/suppliers",       label: "Suppliers",         icon: Truck,      roles: ["admin","pharmacist"] as UserRole[] },
      { href: "/admin/allergy-types",   label: "Allergy Types",     icon: AlertCircle, roles: ["admin"] as UserRole[] },
      { href: "/admin",                 label: "Admin",              icon: Settings,   roles: ["admin"] as UserRole[] },
    ],
  },
];

export default function Sidebar() {
  const { data: session } = useSession();
  const pathname = usePathname();

  const roleLabel = session?.roles?.filter(
    r => !r.startsWith("default-") && r !== "offline_access" && r !== "uma_authorization"
  )[0] ?? "user";

  const initials = session?.user?.name
    ?.split(" ").map(n => n[0]).join("").slice(0, 2).toUpperCase() ?? "U";

  return (
    <aside className="w-[240px] min-h-screen bg-white flex flex-col shrink-0 border-r border-gray-100">
      {/* Logo */}
      <div className="flex items-center gap-2.5 px-5 h-16 border-b border-gray-100 shrink-0">
        <div className="w-9 h-9 rounded-xl bg-primary flex items-center justify-center">
          <Stethoscope size={18} className="text-white" />
        </div>
        <div>
          <p className="text-sm font-bold text-gray-900 leading-tight">Preclinic</p>
          <p className="text-[10px] text-gray-400 leading-tight">Medical System</p>
        </div>
      </div>

      {/* User card */}
      <div className="px-4 py-3 border-b border-gray-100">
        <div className="flex items-center gap-3 bg-primary/5 rounded-xl px-3 py-2.5">
          <div className="w-9 h-9 rounded-full bg-primary flex items-center justify-center text-white text-xs font-bold shrink-0">
            {initials}
          </div>
          <div className="min-w-0">
            <p className="text-xs font-semibold text-gray-800 truncate">{session?.user?.name ?? "User"}</p>
            <p className="text-[10px] text-primary capitalize font-medium mt-0.5">{roleLabel}</p>
          </div>
        </div>
      </div>

      {/* Nav groups */}
      <nav className="flex-1 overflow-y-auto px-3 py-3 space-y-4">
        {navGroups.map(group => {
          const visibleItems = group.items.filter(item => hasAnyRole(session, item.roles));
          if (!visibleItems.length) return null;
          return (
            <div key={group.label}>
              <p className="text-[10px] font-semibold text-gray-400 uppercase tracking-widest px-2 mb-1.5">{group.label}</p>
              <div className="space-y-0.5">
                {visibleItems.map(item => {
                  const Icon = item.icon;
                  const active = pathname === item.href || pathname.startsWith(item.href + "/");
                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      className={cn(
                        "flex items-center gap-2.5 px-2.5 py-2 rounded-lg text-sm transition-all",
                        active
                          ? "bg-primary text-white font-medium"
                          : "text-gray-500 hover:bg-gray-50 hover:text-gray-700"
                      )}
                    >
                      <Icon size={16} />
                      <span className="flex-1">{item.label}</span>
                      {active && <ChevronRight size={13} className="opacity-70" />}
                    </Link>
                  );
                })}
              </div>
            </div>
          );
        })}
      </nav>

      {/* Sign out */}
      <div className="px-3 pb-4 border-t border-gray-100 pt-2 shrink-0">
        <button
          onClick={() => signOut({ callbackUrl: "/login" })}
          className="flex items-center gap-2.5 px-2.5 py-2 rounded-lg text-sm text-gray-400 hover:text-danger hover:bg-danger/10 w-full transition-all"
        >
          <LogOut size={16} />
          Sign out
        </button>
      </div>
    </aside>
  );
}
