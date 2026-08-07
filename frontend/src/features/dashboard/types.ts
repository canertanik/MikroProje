export interface DashboardSummaryDto {
  totalCustomerCount: number;
  totalSupplierCount: number;
  activeProductCount: number;
  criticalStockProductCount: number;
  totalCustomerReceivable: number;
  totalSupplierPayable: number;
  salesTotal: number;
  purchasesTotal: number;
  customerPaymentTotal: number;
  supplierPaymentTotal: number;
  netCashFlow: number;
  pendingPurchaseCount: number;
  draftTransferCount: number;
  customerWithDebtCount: number;
  supplierWithDebtCount: number;
}

export interface DashboardTrendsDto {
  dateLabel: string;
  salesTotal: number;
  purchasesTotal: number;
}

export interface DashboardRecentActivityDto {
  activityType: string;
  documentNumber: string;
  relatedEntityName: string;
  amountOrQuantity: number;
  status: string;
  date: string;
}

export interface DashboardCriticalStockDto {
  productId: number;
  productCode: string;
  productName: string;
  currentStock: number;
  criticalStock: number;
  status: string;
}

export interface TopCustomerDto {
  customerName: string;
  totalSales: number;
}

export interface TopProductDto {
  productName: string;
  totalQuantitySold: number;
}

export interface TopDebtorDto {
  customerName: string;
  debtAmount: number;
}

export interface TopCreditorDto {
  supplierName: string;
  creditAmount: number;
}

export interface DashboardTopRecordsDto {
  topCustomersBySales: TopCustomerDto[];
  topProductsBySales: TopProductDto[];
  topDebtors: TopDebtorDto[];
  topCreditors: TopCreditorDto[];
}
