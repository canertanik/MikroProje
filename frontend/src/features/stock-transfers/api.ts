import api from '../../api/axios';
import type { Result, PagedResult } from '../../types/api';
import type { 
  StockTransferListDto, 
  StockTransferDto, 
  CreateStockTransferRequestDto,
  StockTransferStatus
} from './types';

const getApiErrorMessage = (error: unknown, fallback: string) => {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const response = (error as { response?: { data?: { message?: string } } }).response;
    return response?.data?.message || fallback;
  }

  return error instanceof Error && error.message ? error.message : fallback;
};

export const getStockTransfers = async (
  pageNumber: number = 1,
  pageSize: number = 10,
  search?: string,
  sourceWarehouseId?: number,
  destinationWarehouseId?: number,
  status?: StockTransferStatus,
  startDate?: string,
  endDate?: string
): Promise<PagedResult<StockTransferListDto>> => {
  const params = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  });

  if (search) params.append('search', search);
  if (sourceWarehouseId) params.append('sourceWarehouseId', sourceWarehouseId.toString());
  if (destinationWarehouseId) params.append('destinationWarehouseId', destinationWarehouseId.toString());
  if (status) params.append('status', status.toString());
  if (startDate) params.append('startDate', startDate);
  if (endDate) params.append('endDate', endDate);

  const response = await api.get<Result<PagedResult<StockTransferListDto>>>('/api/stock-transfers', { params });
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Stok transferleri getirilirken bir hata oluştu');
};

export const getStockTransferById = async (id: number): Promise<StockTransferDto> => {
  const response = await api.get<Result<StockTransferDto>>(`/api/stock-transfers/${id}`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Stok transferi getirilirken bir hata oluştu');
};

export const createStockTransfer = async (command: CreateStockTransferRequestDto): Promise<StockTransferDto> => {
  const response = await api.post<Result<StockTransferDto>>('/api/stock-transfers', command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Stok transferi oluşturulurken bir hata oluştu');
};

export const completeStockTransfer = async ({ id, rowVersion }: { id: number, rowVersion: string }): Promise<boolean> => {
  try {
    const response = await api.post<Result<boolean>>(`/api/stock-transfers/${id}/complete`, { rowVersion });

    if (response.data.success && response.data.data !== undefined && response.data.data !== null) {
      return response.data.data;
    }

    throw new Error(response.data.message || 'Transfer tamamlanırken bir hata oluştu');
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Transfer tamamlanırken bir hata oluştu'));
  }
};

export const cancelStockTransfer = async ({ id, rowVersion }: { id: number, rowVersion: string }): Promise<boolean> => {
  try {
    const response = await api.post<Result<boolean>>(`/api/stock-transfers/${id}/cancel`, { rowVersion });

    if (response.data.success && response.data.data !== undefined && response.data.data !== null) {
      return response.data.data;
    }

    throw new Error(response.data.message || 'Transfer iptal edilirken bir hata oluştu');
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Transfer iptal edilirken bir hata oluştu'));
  }
};
