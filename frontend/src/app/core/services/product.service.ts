import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../models/paged-result.model';
import { Product, ProductCreate, ProductFilter, ProductUpdate } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly apiUrl = 'http://localhost:5001/api/products';

  constructor(private http: HttpClient) {}

  // Obtener productos con filtros dinámicos y paginación
  getProducts(filter: ProductFilter): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.searchTerm?.trim()) {
      params = params.set('searchTerm', filter.searchTerm.trim());
    }

    if (filter.category?.trim()) {
      params = params.set('category', filter.category.trim());
    }

    if (filter.isActive !== undefined && filter.isActive !== null) {
      params = params.set('isActive', filter.isActive.toString());
    }

    return this.http.get<PagedResult<Product>>(this.apiUrl, { params });
  }

  // Obtener un producto por Id
  getProductById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  // Crear nuevo producto
  createProduct(dto: ProductCreate): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, dto);
  }

  // Actualizar producto existente
  updateProduct(id: string, dto: ProductUpdate): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, dto);
  }

  // Eliminar producto
  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
