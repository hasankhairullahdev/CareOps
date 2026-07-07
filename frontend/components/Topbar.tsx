"use client";
import { useSession } from "next-auth/react";
import { Bell, Search, Settings } from "lucide-react";

export default function Topbar() {
  const { data: session } = useSession();
  return (
    <header className="h-16 bg-white border-b border-gray-100 flex items-center px-6 gap-4 shrink-0">
      {/* Search */}
      <div className="flex items-center gap-2 bg-gray-50 rounded-lg px-3 py-2 w-64">
        <Search size={15} className="text-gray-400 shrink-0" />
        <input
          type="text"
          placeholder="Search..."
          className="bg-transparent text-sm text-gray-600 placeholder-gray-400 outline-none w-full"
        />
      </div>

      <div className="flex-1" />

      {/* Actions */}
      <div className="flex items-center gap-2">
        <button className="relative w-9 h-9 rounded-lg bg-gray-50 flex items-center justify-center hover:bg-gray-100 transition-colors">
          <Bell size={16} className="text-gray-500" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-danger rounded-full" />
        </button>
        <button className="w-9 h-9 rounded-lg bg-gray-50 flex items-center justify-center hover:bg-gray-100 transition-colors">
          <Settings size={16} className="text-gray-500" />
        </button>
        {/* Avatar */}
        <div className="flex items-center gap-2.5 ml-2 pl-3 border-l border-gray-100">
          <div className="w-8 h-8 rounded-full bg-primary flex items-center justify-center text-white text-xs font-bold">
            {session?.user?.name?.charAt(0)?.toUpperCase() ?? "U"}
          </div>
          <div className="hidden sm:block">
            <p className="text-xs font-semibold text-gray-800 leading-tight">{session?.user?.name ?? "User"}</p>
            <p className="text-[10px] text-gray-400 leading-tight">
              {session?.roles?.filter(r => !r.startsWith("default-") && r !== "offline_access" && r !== "uma_authorization")[0] ?? "user"}
            </p>
          </div>
        </div>
      </div>
    </header>
  );
}
