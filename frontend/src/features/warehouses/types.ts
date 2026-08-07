export interface WarehouseListDto {
  id: number;
  code: string;
  name: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface WarehouseDto {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  isDefault: boolean;
  isActive: boolean;
  createdDate: string;
  rowVersion: string;
}

export interface CreateWarehouseRequestDto {
  code: string;
  name: string;
  description?: string | null;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdateWarehouseRequestDto {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  rowVersion: string;
}
