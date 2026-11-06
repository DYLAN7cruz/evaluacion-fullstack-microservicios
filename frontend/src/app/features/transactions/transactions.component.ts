import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TransactionService } from '../../core/services/transaction.service';
import { ProductService } from '../../core/services/product.service';
import { NotificationService } from '../../core/services/notification.service';
import { Transaction, TransactionCreate, TransactionFilter } from '../../core/models/transaction.model';
import { Product } from '../../core/models/product.model';
import { PagedResult } from '../../core/models/paged-result.model';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './transactions.component.html',
  styleUrls: ['./transactions.component.css']
})
export class TransactionsComponent implements OnInit {
  // Lista y paginación
  pagedResult: PagedResult<Transaction> = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false
  };

  isLoading = false;

  // Filtros avanzados
  filterForm!: FormGroup;
  availableProducts: Product[] = [];

  // Modal Nueva Transacción
  isModalOpen = false;
  transactionForm!: FormGroup;
  selectedProduct: Product | null = null;
  isSaving = false;

  constructor(
    private fb: FormBuilder,
    private transactionService: TransactionService,
    private productService: ProductService,
    private notify: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.initFilterForm();
    this.initTransactionForm();
    this.loadActiveProducts();
    this.loadTransactions();
  }

  // Inicializar formulario de filtros
  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      productId: [''],
      transactionType: [''],
      startDate: [''],
      endDate: ['']
    });
  }

  // Inicializar formulario reactivo de transacción
  private initTransactionForm(): void {
    this.transactionForm = this.fb.group({
      transactionType: ['Venta', Validators.required],
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      detail: ['']
    });

    // Reaccionar al cambio de producto para autocompletar precio y stock
    this.transactionForm.get('productId')?.valueChanges.subscribe((prodId: string) => {
      this.selectedProduct = this.availableProducts.find(p => p.id === prodId) || null;
      if (this.selectedProduct) {
        // Autocompletar precio unitario sugerido del catálogo
        this.transactionForm.patchValue({
          unitPrice: this.selectedProduct.price
        }, { emitEvent: false });
      }
      this.cdr.detectChanges();
    });

    // Reaccionar a cambios en cualquier campo para cálculos dinámicos
    this.transactionForm.valueChanges.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  // Cargar productos activos para selects
  private loadActiveProducts(): void {
    this.productService.getProducts({ page: 1, pageSize: 100, isActive: true }).subscribe({
      next: (res) => {
        this.availableProducts = res.items;
        this.cdr.detectChanges();
      },
      error: () => {
        this.notify.error('No se pudo cargar la lista de productos para los filtros.');
        this.cdr.detectChanges();
      }
    });
  }

  // Cargar transacciones con filtros
  loadTransactions(page = 1): void {
    this.isLoading = true;
    this.cdr.detectChanges();
    const formVals = this.filterForm.value;

    const filter: TransactionFilter = {
      page: page,
      pageSize: 10,
      productId: formVals.productId || undefined,
      transactionType: formVals.transactionType || undefined,
      startDate: formVals.startDate || undefined,
      endDate: formVals.endDate || undefined
    };

    this.transactionService.getTransactions(filter).subscribe({
      next: (res) => {
        this.pagedResult = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
        const msg = err.error?.message || (err.status === 0
          ? 'No se pudo conectar con el microservicio de transacciones (puerto 5002). Verifica que esté corriendo.'
          : 'Error al cargar las transacciones.');
        this.notify.error(msg);
      }
    });
  }

  // Aplicar filtros
  applyFilters(): void {
    this.loadTransactions(1);
  }

  // Limpiar filtros
  resetFilters(): void {
    this.filterForm.reset({
      productId: '',
      transactionType: '',
      startDate: '',
      endDate: ''
    });
    this.loadTransactions(1);
  }

  // Abrir modal de nueva transacción
  openCreateModal(): void {
    this.selectedProduct = null;
    this.transactionForm.reset({
      transactionType: 'Venta',
      productId: '',
      quantity: 1,
      unitPrice: 0,
      detail: ''
    });
    this.loadActiveProducts();
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  // Cerrar modal
  closeModal(): void {
    this.isModalOpen = false;
    this.cdr.detectChanges();
  }

  // =========================================================================
  // REGLA COMPLEJA: Validación de stock insuficiente en Ventas
  // =========================================================================
  get hasInsufficientStock(): boolean {
    const type = this.transactionForm.get('transactionType')?.value;
    const qty = Number(this.transactionForm.get('quantity')?.value || 0);

    if (type === 'Venta' && this.selectedProduct) {
      return qty > this.selectedProduct.stock;
    }
    return false;
  }

  // Total calculado dinámico para vista previa
  get calculatedTotal(): number {
    const qty = Number(this.transactionForm.get('quantity')?.value || 0);
    const price = Number(this.transactionForm.get('unitPrice')?.value || 0);
    return qty * price;
  }

  // Registrar transacción
  saveTransaction(): void {
    if (this.transactionForm.invalid) {
      this.transactionForm.markAllAsTouched();
      this.notify.error('Complete los campos obligatorios del formulario.');
      this.cdr.detectChanges();
      return;
    }

    // Validación compleja de negocio: Stock disponible
    if (this.hasInsufficientStock) {
      this.notify.error(
        `Operación rechazada: No se puede vender más stock del disponible (Máximo: ${this.selectedProduct?.stock} uds).`
      );
      this.cdr.detectChanges();
      return;
    }

    this.isSaving = true;
    this.cdr.detectChanges();
    const vals = this.transactionForm.value;

    const dto: TransactionCreate = {
      transactionType: vals.transactionType,
      productId: vals.productId,
      quantity: Number(vals.quantity),
      unitPrice: Number(vals.unitPrice),
      detail: vals.detail?.trim() || undefined
    };

    this.transactionService.createTransaction(dto).subscribe({
      next: (created) => {
        this.isSaving = false;
        this.closeModal();
        this.notify.success(
          `Transacción de ${created.transactionType} por $${created.totalPrice.toFixed(2)} registrada exitosamente.`
        );
        this.loadTransactions(1);
        this.loadActiveProducts();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isSaving = false;
        this.cdr.detectChanges();
        const msg = err.error?.message || 'Error al procesar la transacción.';
        this.notify.error(msg);
      }
    });
  }

  // Helpers para formulario
  isFieldInvalid(name: string): boolean {
    const control = this.transactionForm.get(name);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }
}
