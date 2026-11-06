export interface Transaction {
  id: string;
  transactionDate: string;
  transactionType: 'Compra' | 'Venta';
  productId: string;
  productName?: string;
  productCode?: string;
  productStock?: number;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  detail?: string;
  createdAt: string;
}

export interface TransactionCreate {
  transactionType: 'Compra' | 'Venta';
  productId: string;
  quantity: number;
  unitPrice?: number;
  detail?: string;
}

export interface TransactionUpdate {
  transactionType: 'Compra' | 'Venta';
  quantity: number;
  unitPrice?: number;
  detail?: string;
}

export interface TransactionFilter {
  productId?: string;
  transactionType?: string;
  startDate?: string;
  endDate?: string;
  page: number;
  pageSize: number;
}
