import { auth } from "@/lib/auth";
import { patientsApi } from "@/lib/api";
import type { PaginatedResult, Patient } from "@/lib/types";
import { formatDate } from "@/lib/utils";
import Link from "next/link";

export default async function PatientsPage({
  searchParams,
}: {
  searchParams: { page?: string; search?: string };
}) {
  const session = await auth();
  const token = session?.accessToken ?? "";
  const page = Number(searchParams.page ?? 1);
  const search = searchParams.search;

  let result: PaginatedResult<Patient> | null = null;
  let error: string | null = null;

  try {
    result = await patientsApi.list(token, page, 20, search) as PaginatedResult<Patient>;
  } catch (e) {
    error = (e as Error).message;
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Patients</h1>
        <Link href="/patients/new"
          className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 transition-colors">
          + Daftar Pasien
        </Link>
      </div>

      {/* Search */}
      <form method="GET" className="mb-6">
        <input
          name="search"
          defaultValue={search}
          placeholder="Cari nama, email, MRN, telepon..."
          className="w-full max-w-md px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </form>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg p-4 mb-4 text-sm">{error}</div>
      )}

      {result && (
        <>
          <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200">
                  <th className="text-left px-4 py-3 font-medium text-gray-600">No. RM</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Nama</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">TTL</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Telepon</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Email</th>
                  <th className="text-left px-4 py-3 font-medium text-gray-600">Terdaftar</th>
                  <th className="px-4 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {result.items.map((p) => (
                  <tr key={p.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-mono text-xs text-gray-600">{p.medicalRecordNumber}</td>
                    <td className="px-4 py-3 font-medium">{p.firstName} {p.lastName}</td>
                    <td className="px-4 py-3 text-gray-600">{formatDate(p.dateOfBirth)}</td>
                    <td className="px-4 py-3 text-gray-600">{p.phoneNumber}</td>
                    <td className="px-4 py-3 text-gray-600">{p.email}</td>
                    <td className="px-4 py-3 text-gray-500 text-xs">{formatDate(p.createdAt)}</td>
                    <td className="px-4 py-3">
                      <Link href={`/patients/${p.id}`} className="text-blue-600 hover:underline text-xs">
                        Detail
                      </Link>
                    </td>
                  </tr>
                ))}
                {result.items.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-4 py-8 text-center text-gray-400">
                      {search ? `Tidak ada hasil untuk "${search}"` : "Belum ada pasien."}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="flex items-center justify-between mt-4 text-sm text-gray-600">
            <span>{result.totalCount} pasien total</span>
            <div className="flex gap-2">
              {page > 1 && (
                <Link href={`/patients?page=${page - 1}${search ? `&search=${search}` : ""}`}
                  className="px-3 py-1 border rounded hover:bg-gray-50">← Prev</Link>
              )}
              {page * 20 < result.totalCount && (
                <Link href={`/patients?page=${page + 1}${search ? `&search=${search}` : ""}`}
                  className="px-3 py-1 border rounded hover:bg-gray-50">Next →</Link>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
