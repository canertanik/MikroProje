import type { PagedResult } from '../../types/api';

export const PurchaseStatus = {
  Pending: 1,
  Received: 2,
  Cancelled: 3
} as const;

export type PurchaseStatus = typeof PurchaseStatus[keyof typeof PurchaseStatus];

export interface PurchaseItemDto {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  vatRate: number;
  lineSubtotal: number;
  vatAmount: number;
  lineTotal: number;
}

export interface PurchaseDto {
  id: number;
  currentAccountId: number;
  currentAccountName: string;
  warehouseId: number;
  warehouseName: string;
  purchaseDate: string;
  subtotal: number;
  vatAmount: number;
  grandTotal: number;
  description?: string;
  status: PurchaseStatus;
  receivedDate?: string;
  items: PurchaseItemDto[];
}

export interface PurchaseListDto {
  id: number;
  currentAccountName: string;
  purchaseDate: string;
  grandTotal: number;
  description?: string;
  status: PurchaseStatus;
}

export interface CreatePurchaseItemRequest {
  productId: number;
  quantity: number;
  unitPrice?: number;
}

export interface CreatePurchaseCommand {
  currentAccountId: number;
  warehouseId: number;
  purchaseDate?: string;
  description?: string;
  items: CreatePurchaseItemRequest[];
}

export type PurchaseListResponse = PagedResult<PurchaseListDto>;
