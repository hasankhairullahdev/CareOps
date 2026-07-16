const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

async function apiFetch<T>(
  path: string,
  token: string,
  options?: RequestInit
): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...options?.headers,
    },
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({ title: res.statusText }));
    throw new Error(err.detail ?? err.title ?? `HTTP ${res.status}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

// ── Patients ──────────────────────────────────────────────────────────────────
export const patientsApi = {
  list: (token: string, page = 1, pageSize = 20, search?: string) =>
    apiFetch(`/api/patients?page=${page}&pageSize=${pageSize}${search ? `&search=${encodeURIComponent(search)}` : ""}`, token),

  get: (token: string, id: string) =>
    apiFetch(`/api/patients/${id}`, token),

  create: (token: string, data: unknown) =>
    apiFetch(`/api/patients`, token, { method: "POST", body: JSON.stringify(data) }),

  update: (token: string, id: string, data: unknown) =>
    apiFetch(`/api/patients/${id}`, token, { method: "PUT", body: JSON.stringify(data) }),
};

// ── Appointments ──────────────────────────────────────────────────────────────
export const appointmentsApi = {
  list: (token: string, params?: Record<string, string>) => {
    const qs = params ? "?" + new URLSearchParams(params).toString() : "";
    return apiFetch(`/api/appointments${qs}`, token);
  },

  get: (token: string, id: string) =>
    apiFetch(`/api/appointments/${id}`, token),

  create: (token: string, data: unknown) =>
    apiFetch(`/api/appointments`, token, { method: "POST", body: JSON.stringify(data) }),

  cancel: (token: string, id: string, reason?: string) =>
    apiFetch(`/api/appointments/${id}/cancel`, token, { method: "POST", body: JSON.stringify({ reason }) }),

  complete: (token: string, id: string) =>
    apiFetch(`/api/appointments/${id}/complete`, token, { method: "POST", body: "" }),

  createPrescription: (token: string, id: string, data: unknown) =>
    apiFetch(`/api/appointments/${id}/prescriptions`, token, { method: "POST", body: JSON.stringify(data) }),

  getDoctorSchedule: (token: string, doctorId: string, date: string) =>
    apiFetch(`/api/doctors/${doctorId}/schedule?date=${date}`, token),
};

// ── Doctors ───────────────────────────────────────────────────────────────────
export const doctorsApi = {
  list: (token: string, params?: Record<string, string>) => {
    const qs = params ? "?" + new URLSearchParams(params).toString() : "";
    return apiFetch(`/api/doctors${qs}`, token);
  },

  get: (token: string, id: string) =>
    apiFetch(`/api/doctors/${id}`, token),

  create: (token: string, data: unknown) =>
    apiFetch(`/api/doctors`, token, { method: "POST", body: JSON.stringify(data) }),

  update: (token: string, id: string, data: unknown) =>
    apiFetch(`/api/doctors/${id}`, token, { method: "PUT", body: JSON.stringify(data) }),

  delete: (token: string, id: string) =>
    apiFetch(`/api/doctors/${id}`, token, { method: "DELETE" }),
};

// ── ServiceTariffs ────────────────────────────────────────────────────────────
export const tariffsApi = {
  list: (token: string, params?: Record<string, string>) => {
    const qs = params ? "?" + new URLSearchParams(params).toString() : "";
    return apiFetch(`/api/billing/tariffs${qs}`, token);
  },

  get: (token: string, id: string) =>
    apiFetch(`/api/billing/tariffs/${id}`, token),

  create: (token: string, data: unknown) =>
    apiFetch(`/api/billing/tariffs`, token, { method: "POST", body: JSON.stringify(data) }),

  update: (token: string, id: string, data: unknown) =>
    apiFetch(`/api/billing/tariffs/${id}`, token, { method: "PUT", body: JSON.stringify(data) }),

  delete: (token: string, id: string) =>
    apiFetch(`/api/billing/tariffs/${id}`, token, { method: "DELETE" }),
};

// ── Pharmacy ──────────────────────────────────────────────────────────────────
export const pharmacyApi = {
  inventory: (token: string, params?: Record<string, string>) => {
    const qs = params ? "?" + new URLSearchParams(params).toString() : "";
    return apiFetch(`/api/pharmacy/inventory${qs}`, token);
  },

  pendingPrescriptions: (token: string, page = 1) =>
    apiFetch(`/api/pharmacy/prescriptions/pending?page=${page}`, token),

  getPrescription: (token: string, id: string) =>
    apiFetch(`/api/pharmacy/prescriptions/${id}`, token),

  dispense: (token: string, prescriptionId: string) =>
    apiFetch(`/api/pharmacy/dispense`, token, { method: "POST", body: JSON.stringify({ prescriptionId }) }),

  addStock: (token: string, medicineId: string, quantity: number, reason: string) =>
    apiFetch(`/api/pharmacy/stock`, token, { method: "POST", body: JSON.stringify({ medicineId, quantity, reason }) }),
};

// ── Lookups (patient-service) ─────────────────────────────────────────────────
export const lookupsApi = {
  bloodTypes: (token: string) =>
    apiFetch(`/api/blood-types`, token),

  allergyTypes: (token: string, isActive?: boolean) => {
    const qs = isActive !== undefined ? `?isActive=${isActive}` : "";
    return apiFetch(`/api/allergy-types${qs}`, token);
  },

  createAllergyType: (token: string, data: unknown) =>
    apiFetch(`/api/allergy-types`, token, { method: "POST", body: JSON.stringify(data) }),

  updateAllergyType: (token: string, id: string, data: unknown) =>
    apiFetch(`/api/allergy-types/${id}`, token, { method: "PUT", body: JSON.stringify(data) }),

  deleteAllergyType: (token: string, id: string) =>
    apiFetch(`/api/allergy-types/${id}`, token, { method: "DELETE" }),
};

// ── Suppliers (pharmacy-service) ──────────────────────────────────────────────
export const suppliersApi = {
  list: (token: string, params?: Record<string, string>) => {
    const qs = params ? "?" + new URLSearchParams(params).toString() : "";
    return apiFetch(`/api/pharmacy/suppliers${qs}`, token);
  },

  create: (token: string, data: unknown) =>
    apiFetch(`/api/pharmacy/suppliers`, token, { method: "POST", body: JSON.stringify(data) }),

  update: (token: string, id: string, data: unknown) =>
    apiFetch(`/api/pharmacy/suppliers/${id}`, token, { method: "PUT", body: JSON.stringify(data) }),

  delete: (token: string, id: string) =>
    apiFetch(`/api/pharmacy/suppliers/${id}`, token, { method: "DELETE" }),
};

// ── PaymentMethods (billing-service) ──────────────────────────────────────────
export const paymentMethodsApi = {
  list: (token: string, isActive?: boolean) => {
    const qs = isActive !== undefined ? `?isActive=${isActive}` : "";
    return apiFetch(`/api/billing/payment-methods${qs}`, token);
  },

  create: (token: string, data: unknown) =>
    apiFetch(`/api/billing/payment-methods`, token, { method: "POST", body: JSON.stringify(data) }),

  update: (token: string, id: string, data: unknown) =>
    apiFetch(`/api/billing/payment-methods/${id}`, token, { method: "PUT", body: JSON.stringify(data) }),

  delete: (token: string, id: string) =>
    apiFetch(`/api/billing/payment-methods/${id}`, token, { method: "DELETE" }),
};

// ── Billing ───────────────────────────────────────────────────────────────────
export const billingApi = {
  list: (token: string, patientId: string, page = 1) =>
    apiFetch(`/api/billing?patientId=${patientId}&page=${page}`, token),

  get: (token: string, id: string) =>
    apiFetch(`/api/billing/${id}`, token),

  summary: (token: string) =>
    apiFetch(`/api/billing/summary`, token),

  issue: (token: string, id: string) =>
    apiFetch(`/api/billing/${id}/issue`, token, { method: "POST", body: "" }),

  pay: (token: string, id: string) =>
    apiFetch(`/api/billing/${id}/pay`, token, { method: "POST", body: "" }),

  cancel: (token: string, id: string) =>
    apiFetch(`/api/billing/${id}/cancel`, token, { method: "POST", body: "" }),
};
