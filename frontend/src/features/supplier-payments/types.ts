export const PaymentMethod = {
  Cash: 1,
  BankTransfer: 2,
  CreditCard: 3
} as const;

export type PaymentMethod = typeof PaymentMethod[keyof typeof PaymentMethod];

export interface SupplierPaymentListDto {
  id: number;
  currentAccountId: number;
  currentAccountName: string;
  amount: number;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  paymentDate: string;
}

export interface SupplierPaymentDto {
  id: number;
  currentAccountId: number;
  currentAccountName: string;
  amount: number;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  description?: string;
  paymentDate: string;
  createdDate: string;
}

export interface CreateSupplierPaymentCommand {
  currentAccountId: number;
  amount: number;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  description?: string;
  paymentDate?: string;
}
