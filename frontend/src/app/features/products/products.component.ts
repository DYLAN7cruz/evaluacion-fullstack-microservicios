import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductService } from '../../core/services/product.service';
import { NotificationService } from '../../core/services/notification.service';
import { Product, ProductCreate, ProductFilter, ProductUpdate } from '../../core/models/product.model';
import { PagedResult } from '../../core/models/paged-result.model';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css']
})
export class ProductsComponent implements OnInit {
  // Lista y paginación
  pagedResult: PagedResult<Product> = {
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
  categories: string[] = ['Tecnología', 'Oficina', 'Periféricos', 'Audio', 'Accesorios', 'General'];

  // Modal Crear / Editar
  isModalOpen = false;
  isEditing = false;
  selectedProductId: string | null = null;
  productForm!: FormGroup;
  isSaving = false;

  // Modal Confirmar Eliminación
  isDeleteModalOpen = false;
  productToDelete: Product | null = null;
  isDeleting = false;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private notify: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.initFilterForm();
    this.initProductForm();
    this.loadProducts();
  }

  // Inicializar formulario de filtros avanzados
  private initFilterForm(): void {
    this.filterForm = this.fb.group({
      searchTerm: [''],
      category: [''],
      isActive: ['']
    });
  }

  // Inicializar formulario reactivo con validaciones estrictas
  private initProductForm(): void {
    this.productForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(150)]],
      description: [''],
      category: ['', Validators.required],
      imageUrl: [''],
      price: [null, [Validators.required, Validators.min(0.01)]],
      stock: [0, [Validators.required, Validators.min(0)]],
      isActive: [true]
    });
  }

  // Cargar productos con filtros actuales
  loadProducts(page = 1): void {
    this.isLoading = true;
    this.cdr.detectChanges();
    const formVals = this.filterForm.value;

    const filter: ProductFilter = {
      page: page,
      pageSize: 10,
      searchTerm: formVals.searchTerm,
      category: formVals.category,
      isActive: formVals.isActive === '' ? undefined : formVals.isActive === 'true'
    };

    this.productService.getProducts(filter).subscribe({
      next: (result) => {
        this.pagedResult = result;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
        const msg = err.error?.message || (err.status === 0 
          ? 'No se pudo conectar con el microservicio de productos (puerto 5001). Verifica que esté corriendo.' 
          : 'Error al cargar el listado de productos.');
        this.notify.error(msg);
      }
    });
  }

  // Aplicar filtros de búsqueda
  applyFilters(): void {
    this.loadProducts(1);
  }

  // Limpiar filtros
  resetFilters(): void {
    this.filterForm.reset({
      searchTerm: '',
      category: '',
      isActive: ''
    });
    this.loadProducts(1);
  }

  // Abrir modal para crear
  openCreateModal(): void {
    this.isEditing = false;
    this.selectedProductId = null;
    this.productForm.reset({
      code: '',
      name: '',
      description: '',
      category: '',
      imageUrl: '',
      price: null,
      stock: 0,
      isActive: true
    });
    this.productForm.get('code')?.enable();
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  // Abrir modal para editar
  openEditModal(product: Product): void {
    this.isEditing = true;
    this.selectedProductId = product.id;
    this.productForm.patchValue({
      code: product.code,
      name: product.name,
      description: product.description || '',
      category: product.category,
      imageUrl: product.imageUrl || '',
      price: product.price,
      stock: product.stock,
      isActive: product.isActive
    });
    // El código no se edita en modo edición
    this.productForm.get('code')?.disable();
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  // Cerrar modal
  closeModal(): void {
    this.isModalOpen = false;
    this.cdr.detectChanges();
  }

  // Guardar (Crear o Actualizar) con validación reactiva
  saveProduct(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      this.notify.error('Por favor complete todos los campos obligatorios correctamente.');
      this.cdr.detectChanges();
      return;
    }

    this.isSaving = true;
    this.cdr.detectChanges();
    const formVals = this.productForm.getRawValue();

    if (this.isEditing && this.selectedProductId) {
      const updateDto: ProductUpdate = {
        code: formVals.code.trim().toUpperCase(),
        name: formVals.name.trim(),
        description: formVals.description?.trim(),
        category: formVals.category,
        imageUrl: formVals.imageUrl?.trim() || undefined,
        price: Number(formVals.price),
        stock: Number(formVals.stock),
        isActive: Boolean(formVals.isActive)
      };

      this.productService.updateProduct(this.selectedProductId, updateDto).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.notify.success('Producto actualizado exitosamente.');
          this.loadProducts(this.pagedResult.page);
        },
        error: (err) => {
          this.isSaving = false;
          this.cdr.detectChanges();
          const msg = this.extractErrorMessage(err, 'Error al actualizar el producto.');
          this.notify.error(msg);
        }
      });
    } else {
      const createDto: ProductCreate = {
        code: formVals.code.trim().toUpperCase(),
        name: formVals.name.trim(),
        description: formVals.description?.trim(),
        category: formVals.category,
        imageUrl: formVals.imageUrl?.trim(),
        price: Number(formVals.price),
        stock: Number(formVals.stock)
      };

      this.productService.createProduct(createDto).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.notify.success('Producto creado exitosamente.');
          this.loadProducts(1);
        },
        error: (err) => {
          this.isSaving = false;
          this.cdr.detectChanges();
          const msg = this.extractErrorMessage(err, 'Error al crear el producto.');
          this.notify.error(msg);
        }
      });
    }
  }

  // Abrir confirmación de eliminación
  confirmDelete(product: Product): void {
    this.productToDelete = product;
    this.isDeleteModalOpen = true;
    this.cdr.detectChanges();
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.productToDelete = null;
    this.cdr.detectChanges();
  }

  // Ejecutar eliminación
  deleteProduct(): void {
    if (!this.productToDelete) return;

    this.isDeleting = true;
    this.cdr.detectChanges();
    this.productService.deleteProduct(this.productToDelete.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.notify.success(`Producto '${this.productToDelete?.name}' eliminado correctamente.`);
        this.closeDeleteModal();
        this.loadProducts(this.pagedResult.page);
      },
      error: (err) => {
        this.isDeleting = false;
        this.cdr.detectChanges();
        const msg = err.error?.message || 'No se puede eliminar el producto porque tiene transacciones históricas asociadas.';
        this.notify.error(msg);
        this.closeDeleteModal();
      }
    });
  }

  // Helpers para validación en template
  isFieldInvalid(fieldName: string): boolean {
    const field = this.productForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  // Extraer mensaje de error del backend
  private extractErrorMessage(err: any, defaultMsg: string): string {
    if (err.status === 0) {
      return 'No se pudo conectar con el microservicio de productos (puerto 5001).';
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
