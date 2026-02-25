export interface TenantSupplier {
  id: string;
  tenantId: string;
  supplierId: string;
  status: 'Active' | 'Suspended' | 'Terminated';
  contractReference?: string;
  notes?: string;
  agreementStartDate?: string;
  agreementEndDate?: string;
  createdAt: string;
  updatedAt: string;
  supplier?: Supplier;
}

export interface Supplier {
  id: string;
  nameEn: string;
  nameAr?: string;
  country: string;
  city?: string;
  licenseNumber?: string;
  phone?: string;
  email?: string;
  website?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  contacts?: SupplierContact[];
}

export interface SupplierContact {
  id: string;
  supplierId: string;
  userId?: string;
  fullName: string;
  email?: string;
  phone?: string;
  jobTitle?: string;
  isPrimary: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSupplierRequest {
  nameEn: string;
  nameAr?: string;
  country: string;
  city?: string;
  licenseNumber?: string;
  phone?: string;
  email?: string;
  website?: string;
  notes?: string;
}

export interface LinkSupplierRequest {
  supplierId: string;
  contractReference?: string;
  notes?: string;
  agreementStartDate?: string;
  agreementEndDate?: string;
}

export interface UpdateTenantSupplierRequest {
  status?: string;
  contractReference?: string;
  notes?: string;
  agreementStartDate?: string;
  agreementEndDate?: string;
}
