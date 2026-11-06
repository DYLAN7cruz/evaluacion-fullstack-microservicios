import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TransactionService } from '../../core/services/transaction.service';
import { ProductService } from '../../core/services/product.service';
import { NotificationService } from '../../core/services/notification.service';
import { Transaction, TransactionCreate, TransactionFilter, TransactionUpdate } from '../../core/models/transaction.model';
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

  // Modal Crear / Editar Transacción
  isModalOpen = false;
  isEditing = false;
  selectedTransactionId: string | null = null;
  transactionForm!: FormGroup;
  selectedProduct: Product | null = null;
  isSaving = false;

  // Modal Ver Detalle (Consulta de Información - Extra)
  isDetailModalOpen = false;
  transactionDetail: Transaction | null = null;

  // Modal Confirmar Eliminación
  isDeleteModalOpen = false;
  transactionToDelete: Transaction | null = null;
  isDeleting = false;

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
      if (this.selectedProduct && !this.isEditing) {
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
    this.isEditing = false;
    this.selectedTransactionId = null;
    this.selectedProduct = null;
    this.transactionForm.reset({
      transactionType: 'Venta',
      productId: '',
      quantity: 1,
      unitPrice: 0,
      detail: ''
    });
    this.transactionForm.get('productId')?.enable();
    this.loadActiveProducts();
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  // Abrir modal para editar transacción
  openEditModal(t: Transaction): void {
    this.isEditing = true;
    this.selectedTransactionId = t.id;
    this.selectedProduct = this.availableProducts.find(p => p.id === t.productId) || null;

    this.transactionForm.patchValue({
      transactionType: t.transactionType,
      productId: t.productId,
      quantity: t.quantity,
      unitPrice: t.unitPrice,
      detail: t.detail || ''
    });

    this.transactionForm.get('productId')?.disable();
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  // Abrir modal de consulta de información (pantalla extra para evidencias)
  openDetailModal(t: Transaction): void {
    this.transactionDetail = t;
    this.isDetailModalOpen = true;
    this.cdr.detectChanges();
  }

  closeDetailModal(): void {
    this.isDetailModalOpen = false;
    this.transactionDetail = null;
    this.cdr.detectChanges();
  }

  // Abrir modal de confirmación de eliminación
  confirmDelete(t: Transaction): void {
    this.transactionToDelete = t;
    this.isDeleteModalOpen = true;
    this.cdr.detectChanges();
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.transactionToDelete = null;
    this.cdr.detectChanges();
  }

  // Cerrar modal de creación/edición
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
      if (!this.isEditing) {
        return qty > this.selectedProduct.stock;
      }
    }
    return false;
  }

  // Total calculado dinámico para vista previa
  get calculatedTotal(): number {
    const qty = Number(this.transactionForm.get('quantity')?.value || 0);
    const price = Number(this.transactionForm.get('unitPrice')?.value || 0);
    return qty * price;
  }

  // Registrar o Actualizar transacción
  saveTransaction(): void {
    if (this.transactionForm.invalid) {
      this.transactionForm.markAllAsTouched();
      this.notify.error('Complete los campos obligatorios del formulario.');
      this.cdr.detectChanges();
      return;
    }

    if (this.hasInsufficientStock) {
      this.notify.error(
        `Operación rechazada: No se puede vender más stock del disponible (Máximo: ${this.selectedProduct?.stock} uds).`
      );
      this.cdr.detectChanges();
      return;
    }

    this.isSaving = true;
    this.cdr.detectChanges();
    const vals = this.transactionForm.getRawValue();

    if (this.isEditing && this.selectedTransactionId) {
      const updateDto: TransactionUpdate = {
        transactionType: vals.transactionType,
        quantity: Number(vals.quantity),
        unitPrice: Number(vals.unitPrice),
        detail: vals.detail?.trim() || undefined
      };

      this.transactionService.updateTransaction(this.selectedTransactionId, updateDto).subscribe({
        next: (updated) => {
          this.isSaving = false;
          this.closeModal();
          this.notify.success(`Transacción actualizada exitosamente. Stock recalculado.`);
          this.loadTransactions(this.pagedResult.page);
          this.loadActiveProducts();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.isSaving = false;
          this.cdr.detectChanges();
          const msg = this.extractErrorMessage(err, 'Error al actualizar la transacción.');
          this.notify.error(msg);
        }
      });
    } else {
      const createDto: TransactionCreate = {
        transactionType: vals.transactionType,
        productId: vals.productId,
        quantity: Number(vals.quantity),
        unitPrice: Number(vals.unitPrice),
        detail: vals.detail?.trim() || undefined
      };

      this.transactionService.createTransaction(createDto).subscribe({
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
          const msg = this.extractErrorMessage(err, 'Error al procesar la transacción.');
          this.notify.error(msg);
        }
      });
    }
  }

  // Eliminar transacción
  deleteTransaction(): void {
    if (!this.transactionToDelete) return;

    this.isDeleting = true;
    this.cdr.detectChanges();

    this.transactionService.deleteTransaction(this.transactionToDelete.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.notify.success('Transacción eliminada exitosamente y stock revertido.');
        this.closeDeleteModal();
        this.loadTransactions(this.pagedResult.page);
        this.loadActiveProducts();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isDeleting = false;
        this.cdr.detectChanges();
        const msg = this.extractErrorMessage(err, 'Error al eliminar la transacción.');
        this.notify.error(msg);
        this.closeDeleteModal();
      }
    });
  }

  // Helpers para formulario
  isFieldInvalid(name: string): boolean {
    const control = this.transactionForm.get(name);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  private extractErrorMessage(err: any, defaultMsg: string): string {
    if (err.status === 0) {
      return 'No se pudo conectar con el microservicio de transacciones (puerto 5002).';
    }
    if (err.error?.message) {
      return err.error.message;
    }
    if (err.error?.errors) {
      const firstKey = Object.keys(err.error.errors)[0];
      if (firstKey && err.error.errors[firstKey].length > 0) {
        return err.error.errors[firstKey][0];
      }
    }
    return defaultMsg;
  }
}
