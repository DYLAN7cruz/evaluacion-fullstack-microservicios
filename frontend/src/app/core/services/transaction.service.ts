import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../models/paged-result.model';
import { Transaction, TransactionCreate, TransactionFilter } from '../models/transaction.model';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly apiUrl = 'http://localhost:5002/api/transactions';

  constructor(private http: HttpClient) {}

  // Obtener transacciones con filtros dinámicos y paginación
  getTransactions(filter: TransactionFilter): Observable<PagedResult<Transaction>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.productId) {
      params = params.set('productId', filter.productId);
    }

    if (filter.transactionType?.trim()) {
      params = params.set('transactionType', filter.transactionType.trim());
    }

    if (filter.startDate) {
      params = params.set('startDate', filter.startDate);
    }

    if (filter.endDate) {
      params = params.set('endDate', filter.endDate);
    }

    return this.http.get<PagedResult<Transaction>>(this.apiUrl, { params });
  }

  // Obtener transacción por Id
  getTransactionById(id: string): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.apiUrl}/${id}`);
  }

  // Registrar nueva transacción (inmutable)
  createTransaction(dto: TransactionCreate): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl, dto);
  }
}
