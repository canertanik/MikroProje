export const PaymentType = {
  Collection: 1,
  Payment: 2
} as const;

export type PaymentType = typeof PaymentType[keyof typeof PaymentType];

export const PaymentMethod = {
  Cash: 1,
  BankTransfer: 2,
  CreditCard: 3
} as const;

export type PaymentMethod = typeof PaymentMethod[keyof typeof PaymentMethod];

export interface PaymentDto {
  id: number;
  currentAccountId: number;
  currentAccountName: string;
  currentAccountCode: string;
  amount: number;
  type: PaymentType;
  typeName: string;
  paymentMethod: PaymentMethod;
  paymentMethodName: string;
  description?: string;
  paymentDate: string;
  isDeleted: boolean;
  createdDate: string;
  updatedDate?: string;
  rowVersion: string;
}

export interface CreatePaymentCommand {
  currentAccountId: number;
  amount: number;
  type: PaymentType;
  paymentMethod: PaymentMethod;
  description?: string;
  paymentDate: string;
}

export interface UpdatePaymentCommand {
  id?: number;
  currentAccountId: number;
  amount: number;
  type: PaymentType;
  paymentMethod: PaymentMethod;
  description?: string;
  paymentDate: string;
  rowVersion: string;
}
