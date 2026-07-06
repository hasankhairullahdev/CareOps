import { cn } from "@/lib/utils";

type Status =
  | "Scheduled" | "InProgress" | "Completed" | "Cancelled"
  | "Pending" | "Dispensed"
  | "Draft" | "Issued" | "Paid";

const map: Record<string, string> = {
  Scheduled: "bg-blue-100 text-blue-700",
  InProgress: "bg-yellow-100 text-yellow-700",
  Completed: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-700",
  Pending: "bg-yellow-100 text-yellow-700",
  Dispensed: "bg-green-100 text-green-700",
  Draft: "bg-gray-100 text-gray-600",
  Issued: "bg-blue-100 text-blue-700",
  Paid: "bg-green-100 text-green-700",
};

export default function StatusBadge({ status }: { status: string }) {
  return (
    <span className={cn("px-2 py-0.5 rounded-full text-xs font-medium", map[status] ?? "bg-gray-100 text-gray-600")}>
      {status}
    </span>
  );
}
