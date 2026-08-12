import api from '../../api/axios';
import type { Result } from '../../types/api';
import type { CurrentAccountDto, CreateCurrentAccountCommand, UpdateCurrentAccountCommand, CurrentAccountStatementResponseDto, GetStatementParams } from './types';

export const getCurrentAccounts = async (): Promise<CurrentAccountDto[]> => {
  const response = await api.get<Result<CurrentAccountDto[]>>('/api/current-accounts');
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Cari hesaplar getirilirken bir hata oluştu');
};

export const getCurrentAccountById = async (id: number): Promise<CurrentAccountDto> => {
  const response = await api.get<Result<CurrentAccountDto>>(`/api/current-accounts/${id}`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Cari hesap getirilirken bir hata oluştu');
};

export const createCurrentAccount = async (command: CreateCurrentAccountCommand): Promise<CurrentAccountDto> => {
  const response = await api.post<Result<CurrentAccountDto>>('/api/current-accounts', command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Cari hesap oluşturulurken bir hata oluştu');
};

export const updateCurrentAccount = async ({ id, command }: { id: number; command: UpdateCurrentAccountCommand }): Promise<CurrentAccountDto> => {
  const response = await api.put<Result<CurrentAccountDto>>(`/api/current-accounts/${id}`, command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Cari hesap güncellenirken bir hata oluştu');
};

export const deleteCurrentAccount = async (id: number): Promise<void> => {
  await api.delete(`/api/current-accounts/${id}`);
};

export const getStatement = async (params: GetStatementParams) => {
  const queryParams = new URLSearchParams({
    pageNumber: params.pageNumber.toString(),
    pageSize: params.pageSize.toString(),
  });
  
  if (params.startDate) queryParams.append('startDate', params.startDate);
  if (params.endDate) queryParams.append('endDate', params.endDate);

  const response = await api.get<Result<CurrentAccountStatementResponseDto>>(
    `/api/current-accounts/${params.id}/statement?${queryParams.toString()}`
  );
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Ekstre getirilirken bir hata oluştu');
};
