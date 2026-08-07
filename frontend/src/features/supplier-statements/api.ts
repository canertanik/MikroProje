import api from '../../api/axios';
import type { Result } from '../../types/api';
import type { SupplierStatementResponseDto, GetSupplierStatementParams } from './types';

export const getSupplierStatement = async (params: GetSupplierStatementParams) => {
  const queryParams = new URLSearchParams({
    pageNumber: params.pageNumber.toString(),
    pageSize: params.pageSize.toString(),
  });
  
  if (params.startDate) queryParams.append('startDate', params.startDate);
  if (params.endDate) queryParams.append('endDate', params.endDate);

  const response = await api.get<Result<SupplierStatementResponseDto>>(
    `/api/supplier-statements/${params.currentAccountId}?${queryParams.toString()}`
  );
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Tedarikçi ekstresi getirilirken bir hata oluştu');
};
