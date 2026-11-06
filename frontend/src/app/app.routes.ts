import { Routes } from '@angular/router';
import { ProductsComponent } from './features/products/products.component';
import { TransactionsComponent } from './features/transactions/transactions.component';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: ProductsComponent },
  { path: 'transactions', component: TransactionsComponent },
  { path: '**', redirectTo: 'products' }
];
