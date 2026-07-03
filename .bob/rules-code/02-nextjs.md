# Next.js Rules (Code Mode)

## Project Setup
- Next.js 14 App Router — TIDAK pakai Pages Router
- TypeScript strict mode
- Tailwind CSS + Shadcn/ui
- TanStack Query untuk server state (client components)
- React Hook Form + Zod untuk forms
- `next-auth` v5 untuk Keycloak integration

## Keycloak Auth (next-auth)
- Provider: Keycloak via `next-auth`
- Session strategy: JWT
- Extract roles dari token: `token.realm_access?.roles`
- Middleware: protect semua route kecuali `/login`
- Role-based redirect: login → redirect ke halaman sesuai role

## Route Structure
```
app/
├── (auth)/login/page.tsx          # Login page (redirect ke Keycloak)
├── (dashboard)/
│   ├── layout.tsx                 # Sidebar + topbar, role-aware navigation
│   ├── patients/page.tsx          # Receptionist, Admin
│   ├── appointments/page.tsx      # Receptionist, Doctor, Admin
│   ├── pharmacy/page.tsx          # Pharmacist, Admin
│   ├── billing/page.tsx           # Cashier, Admin
│   └── admin/page.tsx             # Admin only
```

## Role-based UI
- Sidebar navigation hanya tampilkan menu yang boleh diakses role tersebut
- Gunakan `useSession()` untuk ambil roles dari session
- Buat helper: `hasRole(session, 'doctor')` → boolean
- Sembunyikan tombol/action yang tidak boleh diakses — jangan hanya disable

## Component Structure
- Satu komponen per file
- Props interface dengan suffix `Props`
- `"use client"` hanya kalau butuh interaktivitas / hooks
- Server Components untuk data fetching awal

## Data Fetching
- Server Components: fetch langsung dengan Authorization header
- Client Components: TanStack Query dengan token dari `useSession()`
- API base URL: `process.env.NEXT_PUBLIC_API_URL` (mengarah ke API Gateway)

## Forms
- React Hook Form + Zod validation
- Error per field
- Disable submit saat `isSubmitting`
- Toast notification setelah sukses/gagal (gunakan Shadcn `useToast`)

## Styling
- Tailwind utility classes
- Shadcn/ui komponen: Button, Input, Select, Dialog, Table, Badge, Card, Tabs
- Konsisten dengan tema Shadcn — jangan mix custom CSS
- Warna status: green=active/paid, yellow=pending, red=cancelled/overdue
- Responsive: mobile-first

## TypeScript
- Semua props typed — tidak ada `any`
- Buat types di `lib/types.ts` per domain
- API response types harus match dengan backend DTO
