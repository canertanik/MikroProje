export const CurrentAccountType = {
  Customer: 1,
  Supplier: 2,
  Both: 3
} as const;

export type CurrentAccountType = typeof CurrentAccountType[keyof typeof CurrentAccountType];

export interface CurrentAccountDto {
  id: number;
  code: string;
  name: string;
  taxNumber?: string | null;
  phone?: string | null;
  email?: string | null;
  type: CurrentAccountType;
  balance: number;
  createdDate: string;
}

export interface CreateCurrentAccountCommand {
  code: string;
  name: string;
  taxNumber?: string | null;
  phone?: string | null;
  email?: string | null;
  type: CurrentAccountType;
}

export interface UpdateCurrentAccountCommand {
  id: number;
  code: string;
  name: string;
  taxNumber?: string | null;
  phone?: string | null;
  email?: string | null;
  type: CurrentAccountType;
}

export const DocumentType = {
  Sale: 1,
  Payment: 2,
  Purchase: 3,
  SupplierPayment: 4
} as const;

export type DocumentType = typeof DocumentType[keyof typeof DocumentType];

export interface StatementDto {
  date: string;
  documentType: DocumentType;
  documentTypeName: string;
  documentId: number;
  description: string;
  debit: number;
  credit: number;
  balanceAfterTransaction: number;
}

export interface GetStatementParams {
  id: number;
  pageNumber: number;
  pageSize: number;
  startDate?: string;
  endDate?: string;
}
