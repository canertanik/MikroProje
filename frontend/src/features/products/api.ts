import api from '../../api/axios';
import type { PagedResult, Result } from '../../types/api';
import type { ProductDto, CreateProductCommand, UpdateProductCommand, ProductStockDto } from './types';

export const getProducts = async (
  pageNumber: number = 1,
  pageSize: number = 20,
  search?: string,
  criticalOnly?: boolean
): Promise<PagedResult<ProductDto>> => {
  const params = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  });

  if (search) {
    params.append('search', search);
  }
  
  if (criticalOnly !== undefined) {
    params.append('criticalOnly', criticalOnly.toString());
  }

  const response = await api.get<Result<PagedResult<ProductDto>>>('/api/products', { params });
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Ürünler getirilirken bir hata oluştu');
};

export const getProductById = async (id: number): Promise<ProductDto> => {
  const response = await api.get<Result<ProductDto>>(`/api/products/${id}`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Ürün getirilirken bir hata oluştu');
};

export const createProduct = async (command: CreateProductCommand): Promise<ProductDto> => {
  const response = await api.post<Result<ProductDto>>('/api/products', command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Ürün oluşturulurken bir hata oluştu');
};

export const updateProduct = async ({ id, command }: { id: number; command: UpdateProductCommand }): Promise<ProductDto> => {
  const response = await api.put<Result<ProductDto>>(`/api/products/${id}`, command);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Ürün güncellenirken bir hata oluştu');
};

export const deleteProduct = async (id: number): Promise<void> => {
  await api.delete(`/api/products/${id}`);
};

export const getProductStocks = async (id: number): Promise<ProductStockDto[]> => {
  const response = await api.get<Result<ProductStockDto[]>>(`/api/products/${id}/stocks`);
  
  if (response.data.success && response.data.data) {
    return response.data.data;
  }
  
  throw new Error(response.data.message || 'Ürün stokları getirilirken bir hata oluştu');
};
