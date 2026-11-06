export interface Product {
  id: string;
  code: string;
  name: string;
  description?: string;
  category: string;
  imageUrl?: string;
  price: number;
  stock: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ProductCreate {
  code: string;
  name: string;
  description?: string;
  category: string;
  imageUrl?: string;
  price: number;
  stock: number;
}

export interface ProductUpdate {
  code: string;
  name: string;
  description?: string;
  category: string;
  imageUrl?: string;
  price: number;
  stock: number;
  isActive: boolean;
}

export interface ProductFilter {
  searchTerm?: string;
  category?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
}
