-- Extensión para generación de UUIDs aleatorios
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Esquema para el Microservicio de Productos
CREATE SCHEMA IF NOT EXISTS catalog;

-- Esquema para el Microservicio de Transacciones
CREATE SCHEMA IF NOT EXISTS inventory;


-- 2. TABLA: catalog.products (Microservicio de Productos)
CREATE TABLE IF NOT EXISTS catalog.products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    category VARCHAR(70) NOT NULL,
    image_url TEXT,
    price NUMERIC(12, 2) NOT NULL CHECK (price >= 0),
    stock INT NOT NULL DEFAULT 0 CHECK (stock >= 0),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para optimizar búsquedas y filtros dinámicos en Productos
CREATE INDEX IF NOT EXISTS idx_products_code ON catalog.products(code);
CREATE INDEX IF NOT EXISTS idx_products_name ON catalog.products(name);
CREATE INDEX IF NOT EXISTS idx_products_category ON catalog.products(category);
CREATE INDEX IF NOT EXISTS idx_products_is_active ON catalog.products(is_active);


-- 3. TABLA: inventory.transactions (Microservicio de Transacciones)
CREATE TABLE IF NOT EXISTS inventory.transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    transaction_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    transaction_type VARCHAR(20) NOT NULL CHECK (transaction_type IN ('Compra', 'Venta')),
    product_id UUID NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price NUMERIC(12, 2) NOT NULL CHECK (unit_price >= 0),
    total_price NUMERIC(12, 2) NOT NULL CHECK (total_price >= 0),
    detail TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_transactions_product 
        FOREIGN KEY (product_id) 
        REFERENCES catalog.products(id) 
        ON DELETE RESTRICT
);

-- Índices para optimizar filtros dinámicos en Transacciones (fechas, tipo y producto)
CREATE INDEX IF NOT EXISTS idx_transactions_product_id ON inventory.transactions(product_id);
CREATE INDEX IF NOT EXISTS idx_transactions_date ON inventory.transactions(transaction_date);
CREATE INDEX IF NOT EXISTS idx_transactions_type ON inventory.transactions(transaction_type);

-- 4. DATOS INICIALES DE PRUEBA (SEEDS)
INSERT INTO catalog.products (id, code, name, description, category, image_url, price, stock, is_active)
VALUES
    ('a0000000-0000-0000-0000-000000000001', 'PROD-001', 'Laptop Lenovo', 'Laptop básica para oficina', 'Tecnología', 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500', 650.00, 10, TRUE),
    ('a0000000-0000-0000-0000-000000000002', 'PROD-002', 'Mouse Óptico USB', 'Mouse negro estándar con cable', 'Accesorios', 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=500', 12.50, 45, TRUE),
    ('a0000000-0000-0000-0000-000000000003', 'PROD-003', 'Teclado Estándar', 'Teclado multimedia en español', 'Accesorios', 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=500', 20.00, 25, TRUE),
    ('a0000000-0000-0000-0000-000000000004', 'PROD-004', 'Monitor 24 Pulgadas', 'Monitor LED Full HD para escritorio', 'Monitores', 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=500', 175.00, 8, TRUE)
ON CONFLICT (id) DO NOTHING;

INSERT INTO inventory.transactions (id, transaction_date, transaction_type, product_id, quantity, unit_price, total_price, detail)
VALUES
    ('b0000000-0000-0000-0000-000000000001', CURRENT_TIMESTAMP - INTERVAL '4 days', 'Compra', 'a0000000-0000-0000-0000-000000000001', 12, 650.00, 7800.00, 'Compra inicial a proveedor'),
    ('b0000000-0000-0000-0000-000000000002', CURRENT_TIMESTAMP - INTERVAL '2 days', 'Venta',  'a0000000-0000-0000-0000-000000000001', 2, 650.00, 1300.00, 'Venta en local - Factura 001'),
    ('b0000000-0000-0000-0000-000000000003', CURRENT_TIMESTAMP - INTERVAL '3 days', 'Compra', 'a0000000-0000-0000-0000-000000000002', 50, 12.50, 625.00, 'Ingreso de stock de lote'),
    ('b0000000-0000-0000-0000-000000000004', CURRENT_TIMESTAMP - INTERVAL '1 day',  'Venta',  'a0000000-0000-0000-0000-000000000002', 5, 12.50, 62.50, 'Venta a cliente final')
ON CONFLICT (id) DO NOTHING;
