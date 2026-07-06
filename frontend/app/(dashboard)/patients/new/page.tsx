"use client";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { patientsApi } from "@/lib/api";

const schema = z.object({
  firstName: z.string().min(1, "Wajib diisi"),
  lastName: z.string().min(1, "Wajib diisi"),
  dateOfBirth: z.string().min(1, "Wajib diisi"),
  gender: z.enum(["Male", "Female", "Other"]),
  phoneNumber: z.string().min(1, "Wajib diisi"),
  email: z.string().email("Email tidak valid"),
  address: z.string().min(1, "Wajib diisi"),
});

type FormData = z.infer<typeof schema>;

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      {children}
      {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
    </div>
  );
}

export default function NewPatientPage() {
  const { data: session } = useSession();
  const router = useRouter();
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  async function onSubmit(data: FormData) {
    try {
      await patientsApi.create(session?.accessToken ?? "", data);
      router.push("/patients");
    } catch (e) {
      alert((e as Error).message);
    }
  }

  return (
    <div className="max-w-2xl">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Daftar Pasien Baru</h1>
      <form onSubmit={handleSubmit(onSubmit)} className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <Field label="Nama Depan" error={errors.firstName?.message}>
            <input {...register("firstName")} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </Field>
          <Field label="Nama Belakang" error={errors.lastName?.message}>
            <input {...register("lastName")} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </Field>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <Field label="Tanggal Lahir" error={errors.dateOfBirth?.message}>
            <input type="date" {...register("dateOfBirth")} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </Field>
          <Field label="Jenis Kelamin" error={errors.gender?.message}>
            <select {...register("gender")} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option value="Male">Laki-laki</option>
              <option value="Female">Perempuan</option>
              <option value="Other">Lainnya</option>
            </select>
          </Field>
        </div>
        <Field label="No. Telepon" error={errors.phoneNumber?.message}>
          <input {...register("phoneNumber")} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </Field>
        <Field label="Email" error={errors.email?.message}>
          <input type="email" {...register("email")} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </Field>
        <Field label="Alamat" error={errors.address?.message}>
          <textarea {...register("address")} rows={3} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </Field>

        <div className="flex gap-3 pt-2">
          <button type="submit" disabled={isSubmitting}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors">
            {isSubmitting ? "Menyimpan..." : "Simpan"}
          </button>
          <button type="button" onClick={() => router.back()}
            className="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-50 transition-colors">
            Batal
          </button>
        </div>
      </form>
    </div>
  );
}
