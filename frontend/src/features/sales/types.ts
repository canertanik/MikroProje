export interface SaleDetailDto {
  id: number;
  productId: number;
  productName: string;
  productCode: string;
  quantity: number;
  unitPrice: number;
  vatRate: number;
  discount: number;
  lineTotal: number;
}

export interface SaleDto {
  id: number;
  currentAccountId: number;
  currentAccountName: string;
  currentAccountCode: string;
  warehouseId: number;
  warehouseName: string;
  saleDate: string;
  totalAmount: number;
  vatAmount: number;
  grandTotal: number;
  description: string | null;
  isDeleted: boolean;
  createdDate: string;
  updatedDate: string | null;
  items: SaleDetailDto[];
}

export interface SaleItemDto {
  productId: number;
  quantity: number;
  discount: number;
  unitPrice?: number | null;
}

export interface CreateSaleCommand {
  currentAccountId: number;
  warehouseId: number;
  saleDate?: string;
  description?: string | null;
  items: SaleItemDto[];
}

export interface UpdateSaleCommand {
  description?: string | null;
}
