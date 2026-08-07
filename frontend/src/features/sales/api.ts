import api from '../../api/axios';
import type { Result, PagedResult } from '../../types/api';
import type { 
  SaleDto, 
  CreateSaleCommand, 
  UpdateSaleCommand 
} from './types';

export const getSales = async (
  pageNumber: number = 1,
  pageSize: number = 20,
  search?: string
): Promise<PagedResult<SaleDto>> => {
  const params = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  });

  if (search) {
    params.append('search', search);
  }

  const response = await api.get<Result<PagedResult<SaleDto>>>('/api/sales', { params });
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Satışlar getirilirken bir hata oluştu');
};

export const getSaleById = async (id: number): Promise<SaleDto> => {
  const response = await api.get<Result<SaleDto>>(`/api/sales/${id}`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Satış getirilirken bir hata oluştu');
};

export const getSalesByCurrentAccount = async (
  currentAccountId: number,
  pageNumber: number = 1,
  pageSize: number = 20
): Promise<PagedResult<SaleDto>> => {
  const params = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  });

  const response = await api.get<Result<PagedResult<SaleDto>>>(`/api/sales/current-account/${currentAccountId}`, { params });
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Cariye ait satışlar getirilirken bir hata oluştu');
};

export const createSale = async (command: CreateSaleCommand): Promise<SaleDto> => {
  const response = await api.post<Result<SaleDto>>('/api/sales', command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Satış oluşturulurken bir hata oluştu');
};

export const updateSale = async ({ id, command }: { id: number; command: UpdateSaleCommand }): Promise<SaleDto> => {
  const response = await api.put<Result<SaleDto>>(`/api/sales/${id}`, command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Satış güncellenirken bir hata oluştu');
};

export const cancelSale = async (id: number): Promise<void> => {
  await api.delete(`/api/sales/${id}`);
};

export const exportSalesPdf = async (): Promise<Blob> => {
  const response = await api.get('/api/sales/export/pdf', {
    responseType: 'blob'
  });
  return response.data;
};

export const exportSalesExcel = async (): Promise<Blob> => {
  const response = await api.get('/api/sales/export', {
    responseType: 'blob'
  });
  return response.data;
};
