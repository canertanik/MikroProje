import type { DocumentType } from '../current-accounts/types';
import type { PagedResult } from '../../types/api';

export interface SupplierStatementItemDto {
  date: string;
  documentType: DocumentType;
  documentTypeName: string;
  documentNumber: string;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

export interface SupplierStatementResponseDto {
  currentAccountId: number;
  currentAccountName: string;
  supplierBalance: number;
  items: PagedResult<SupplierStatementItemDto>;
}

export interface GetSupplierStatementParams {
  currentAccountId: number;
  pageNumber: number;
  pageSize: number;
  startDate?: string;
  endDate?: string;
}
