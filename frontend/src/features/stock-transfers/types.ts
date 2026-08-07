export const StockTransferStatus = {
  Draft: 1,
  Completed: 2,
  Cancelled: 3,
} as const;

export type StockTransferStatus = typeof StockTransferStatus[keyof typeof StockTransferStatus];

export interface StockTransferListDto {
  id: number;
  transferNumber: string;
  sourceWarehouseCode: string;
  destinationWarehouseCode: string;
  transferDate: string;
  status: StockTransferStatus;
  createdDate: string;
  rowVersion: string;
}

export interface StockTransferItemDto {
  id: number;
  productId: number;
  productCode: string;
  productName: string;
  quantity: number;
}

export interface StockTransferDto {
  id: number;
  transferNumber: string;
  sourceWarehouseId: number;
  sourceWarehouseCode: string;
  sourceWarehouseName: string;
  destinationWarehouseId: number;
  destinationWarehouseCode: string;
  destinationWarehouseName: string;
  transferDate: string;
  description?: string | null;
  status: StockTransferStatus;
  items: StockTransferItemDto[];
  createdDate: string;
  updatedDate?: string | null;
  rowVersion: string;
}

export interface CreateStockTransferItemRequestDto {
  productId: number;
  quantity: number;
}

export interface CreateStockTransferRequestDto {
  sourceWarehouseId: number;
  destinationWarehouseId: number;
  transferDate?: string | null;
  description?: string | null;
  items: CreateStockTransferItemRequestDto[];
}
