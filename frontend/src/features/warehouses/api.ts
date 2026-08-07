import api from '../../api/axios';
import type { Result, PagedResult } from '../../types/api';
import type { 
  WarehouseListDto, 
  WarehouseDto, 
  CreateWarehouseRequestDto, 
  UpdateWarehouseRequestDto 
} from './types';

export const getWarehouses = async (
  pageNumber: number = 1,
  pageSize: number = 10,
  search?: string,
  isActive?: boolean,
  isDefault?: boolean
): Promise<PagedResult<WarehouseListDto>> => {
  const params = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  });

  if (search) {
    params.append('search', search);
  }
  
  if (isActive !== undefined) {
    params.append('isActive', isActive.toString());
  }

  if (isDefault !== undefined) {
    params.append('isDefault', isDefault.toString());
  }

  const response = await api.get<Result<PagedResult<WarehouseListDto>>>('/api/warehouses', { params });
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Depolar getirilirken bir hata oluştu');
};

export const getWarehouseById = async (id: number): Promise<WarehouseDto> => {
  const response = await api.get<Result<WarehouseDto>>(`/api/warehouses/${id}`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Depo getirilirken bir hata oluştu');
};

export const createWarehouse = async (command: CreateWarehouseRequestDto): Promise<WarehouseDto> => {
  const response = await api.post<Result<WarehouseDto>>('/api/warehouses', command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Depo oluşturulurken bir hata oluştu');
};

export const updateWarehouse = async ({ id, command }: { id: number; command: UpdateWarehouseRequestDto }): Promise<WarehouseDto> => {
  const response = await api.put<Result<WarehouseDto>>(`/api/warehouses/${id}`, command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Depo güncellenirken bir hata oluştu');
};

export const deleteWarehouse = async (id: number): Promise<void> => {
  await api.delete(`/api/warehouses/${id}`);
};

export const setDefaultWarehouse = async (id: number): Promise<WarehouseDto> => {
  const response = await api.put<Result<WarehouseDto>>(`/api/warehouses/${id}/set-default`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Varsayılan depo ayarlanırken bir hata oluştu');
};
