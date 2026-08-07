export interface ProductDto {
  id: number;
  code: string;
  name: string;
  barcode: string | null;
  purchasePrice: number;
  salePrice: number;
  vatRate: number;
  stockQuantity: number;
  criticalStockQuantity: number;
  isCriticalStock: boolean;
  createdDate: string;
  updatedDate: string | null;
}

export interface CreateProductCommand {
  code: string;
  name: string;
  barcode: string | null;
  purchasePrice: number;
  salePrice: number;
  vatRate: number;
  criticalStockQuantity: number;
  initialStockQuantity: number;
}

export interface UpdateProductCommand {
  code: string;
  name: string;
  barcode: string | null;
  purchasePrice: number;
  salePrice: number;
  vatRate: number;
  criticalStockQuantity: number;
}

export interface ProductStockDto {
  warehouseId: number;
  warehouseCode: string;
  warehouseName: string;
  quantity: number;
}
