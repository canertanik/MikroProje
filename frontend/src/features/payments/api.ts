import api from '../../api/axios';
import type { Result, PagedResult } from '../../types/api';
import type { 
  PaymentDto, 
  CreatePaymentCommand, 
  UpdatePaymentCommand 
} from './types';

export interface GetPaymentsParams {
  pageNumber?: number;
  pageSize?: number;
  currentAccountId?: number;
  searchTerm?: string;
  startDate?: string;
  endDate?: string;
}

export const getPayments = async (params: GetPaymentsParams): Promise<PagedResult<PaymentDto>> => {
  const queryParams = new URLSearchParams({
    pageNumber: (params.pageNumber || 1).toString(),
    pageSize: (params.pageSize || 20).toString(),
  });

  if (params.currentAccountId) {
    queryParams.append('currentAccountId', params.currentAccountId.toString());
  }
  if (params.searchTerm) {
    queryParams.append('searchTerm', params.searchTerm);
  }
  if (params.startDate) {
    queryParams.append('startDate', params.startDate);
  }
  if (params.endDate) {
    queryParams.append('endDate', params.endDate);
  }

  const response = await api.get<Result<PagedResult<PaymentDto>>>('/api/payments', { params: queryParams });
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Tahsilatlar getirilirken bir hata oluştu');
};

export const getPaymentById = async (id: number): Promise<PaymentDto> => {
  const response = await api.get<Result<PaymentDto>>(`/api/payments/${id}`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Tahsilat getirilirken bir hata oluştu');
};

export const createPayment = async (command: CreatePaymentCommand): Promise<PaymentDto> => {
  const response = await api.post<Result<PaymentDto>>('/api/payments', command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Tahsilat oluşturulurken bir hata oluştu');
};

export const updatePayment = async ({ id, command }: { id: number; command: UpdatePaymentCommand }): Promise<PaymentDto> => {
  const response = await api.put<Result<PaymentDto>>(`/api/payments/${id}`, command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Tahsilat güncellenirken bir hata oluştu');
};

export const deletePayment = async (id: number): Promise<void> => {
  const response = await api.delete(`/api/payments/${id}`);
  if (response.data && response.data.success === false) {
    throw new Error(response.data.message || 'Tahsilat silinirken bir hata oluştu');
  }
};
