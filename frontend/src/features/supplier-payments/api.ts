import api from '../../api/axios';
import type { Result, PagedResult } from '../../types/api';
import type {
  SupplierPaymentListDto,
  SupplierPaymentDto,
  CreateSupplierPaymentCommand,
} from './types';

export interface GetSupplierPaymentsParams {
  pageNumber: number;
  pageSize: number;
  currentAccountId?: number;
  searchTerm?: string;
  startDate?: string;
  endDate?: string;
}

export const getSupplierPayments = async (params: GetSupplierPaymentsParams) => {
  const { data } = await api.get<Result<PagedResult<SupplierPaymentListDto>>>('/api/supplier-payments', { params });
  return data.data;
};

export const getSupplierPaymentById = async (id: number) => {
  const { data } = await api.get<Result<SupplierPaymentDto>>(`/api/supplier-payments/${id}`);
  return data.data;
};

export const createSupplierPayment = async (command: CreateSupplierPaymentCommand) => {
  const { data } = await api.post<Result<SupplierPaymentDto>>('/api/supplier-payments', command);
  return data.data;
};
