export interface ClientDto {
  id: string;
  nameEn: string;
  nameAr?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  address?: string;
  city?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ClientListDto {
  id: string;
  nameEn: string;
  nameAr?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  city?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateClientRequest {
  nameEn: string;
  nameAr?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  address?: string;
  city?: string;
  notes?: string;
}

export interface UpdateClientRequest {
  nameEn?: string;
  nameAr?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  address?: string;
  city?: string;
  notes?: string;
  isActive?: boolean;
}
