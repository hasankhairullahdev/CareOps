// ── Patient ───────────────────────────────────────────────────────────────────
export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber: string;
  email: string;
  address: string;
  medicalRecordNumber: string;
  createdAt: string;
  updatedAt?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ── Appointment ───────────────────────────────────────────────────────────────
export type AppointmentStatus = "Scheduled" | "InProgress" | "Completed" | "Cancelled";

export interface Appointment {
  id: string;
  patientId: string;
  doctorId: string;
  doctorName: string;
  scheduledAt: string;
  status: AppointmentStatus;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Doctor {
  id: string;
  name: string;
  specialization: string;
  licenseNumber: string;
  schedule: string;
}

// ── Pharmacy ──────────────────────────────────────────────────────────────────
export type PrescriptionStatus = "Pending" | "Dispensed" | "Cancelled";

export interface Medicine {
  id: string;
  name: string;
  genericName: string;
  category: string;
  unit: string;
  stockQuantity: number;
  minimumStock: number;
  price: number;
  expiryDate: string;
  isLowStock: boolean;
  isExpiringSoon: boolean;
}

export interface Prescription {
  id: string;
  externalPrescriptionId: string;
  patientId: string;
  appointmentId: string;
  status: PrescriptionStatus;
  createdAt: string;
  dispensedAt?: string;
  items: PrescriptionItem[];
}

export interface PrescriptionItem {
  id: string;
  medicineName: string;
  quantity: number;
  dosage: string;
  instructions: string;
}

export interface InventoryResult {
  items: Medicine[];
  totalCount: number;
  lowStockCount: number;
}

// ── Billing ───────────────────────────────────────────────────────────────────
export type BillStatus = "Draft" | "Issued" | "Paid" | "Cancelled";

export interface Bill {
  id: string;
  patientId: string;
  appointmentId: string;
  status: BillStatus;
  totalAmount: number;
  createdAt: string;
  issuedAt?: string;
  paidAt?: string;
  lineItems: BillLineItem[];
}

export interface BillLineItem {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
}

export interface BillsSummary {
  pendingCount: number;
  pendingAmount: number;
  paidTodayCount: number;
  paidTodayAmount: number;
  totalTodayCount: number;
}
