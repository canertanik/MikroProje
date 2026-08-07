import api from '../../api/axios';
import type { Result, PagedResult } from '../../types/api';
import type { PurchaseDto, PurchaseListDto, CreatePurchaseCommand } from './types';

export interface GetPurchasesParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  startDate?: string;
  endDate?: string;
  status?: number;
}

export const getPurchases = async (params: GetPurchasesParams = { pageNumber: 1, pageSize: 20 }) => {
  const { data } = await api.get<Result<PagedResult<PurchaseListDto>>>('/api/purchases', {
    params,
  });
  return data;
};

export const getPurchaseById = async (id: number) => {
  const { data } = await api.get<Result<PurchaseDto>>(`/api/purchases/${id}`);
  return data;
};

export const createPurchase = async (command: CreatePurchaseCommand) => {
  const { data } = await api.post<Result<PurchaseDto>>('/api/purchases', command);
  return data;
};

export const receivePurchase = async (id: number) => {
  const { data } = await api.post<Result<PurchaseDto>>(`/api/purchases/${id}/receive`);
  return data;
};

export const cancelPurchase = async (id: number) => {
  const { data } = await api.post<Result<boolean>>(`/api/purchases/${id}/cancel`);
  return data;
};

export const deletePurchase = async (id: number) => {
  const { data } = await api.delete<Result<boolean>>(`/api/purchases/${id}`);
  return data;
};

export const exportPurchasesExcel = async () => {
  const response = await api.get('/api/purchases/export', {
    responseType: 'blob',
  });
  return response.data;
};

export const exportPurchasesPdf = async () => {
  const response = await api.get('/api/purchases/export/pdf', {
    responseType: 'blob',
  });
  return response.data;
};
