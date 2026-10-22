BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- 1. Usuarios del sistema
CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    username varchar(64) NOT NULL,
    email varchar(254) NOT NULL,
    password varchar(512) NOT NULL,
    name varchar(160) NOT NULL,
    role varchar(16) NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT ck_users_role CHECK (role IN ('Admin', 'User'))
);

INSERT INTO users (id, username, email, password, name, role, is_active)
VALUES
    (
        '10000000-0000-4000-8000-000000000001',
        'admin',
        'admin@tecnm.local',
        'AQAAAAIAAYagAAAAEPjbneKeORd2reFIG62af0ufOMPtwf64j0jpBznNBo3Y8K/BUoUW+IhJOm0b5iqWOg==',
        'Administrador Demo',
        'Admin',
        true
    ),
    (
        '10000000-0000-4000-8000-000000000002',
        'usuario',
        'usuario@tecnm.local',
        'AQAAAAIAAYagAAAAECBhXWISKf+YayCz0xB8E4vyjO6chAfRLTq+f2eA4ztdtdDqA99jr+VsfZXUoKbfdQ==',
        'Usuario Demo',
        'User',
        true
    )
ON CONFLICT (username) DO UPDATE SET
    email = EXCLUDED.email,
    password = EXCLUDED.password,
    name = EXCLUDED.name,
    role = EXCLUDED.role,
    is_active = EXCLUDED.is_active;

-- 2. Alumnos de Control Escolar
CREATE TABLE IF NOT EXISTS alumnos (
    matricula varchar(20) PRIMARY KEY,
    nombre_completo varchar(160) NOT NULL,
    carrera varchar(100) NOT NULL,
    semestre integer NOT NULL,
    turno varchar(20) NOT NULL,
    estatus varchar(30) NOT NULL,
    tiene_beca boolean NOT NULL DEFAULT false,
    porcentaje_beca numeric(5, 2) NOT NULL DEFAULT 0.00,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO alumnos (matricula, nombre_completo, carrera, semestre, turno, estatus, tiene_beca, porcentaje_beca)
VALUES
    ('23050243', 'ORTEGON HERNANDEZ KLEBER', 'Ing. Industrial', 4, 'Matutino', 'Inscrito', false, 0.00),
    ('23050118', 'GARCIA ZAVALA LUIS FERNANDO', 'Ing. Mecatronica', 3, 'Matutino', 'Inscrito', false, 0.00),
    ('22050890', 'LOPEZ SANCHEZ MARIANA', 'Ing. Gestion Empresarial', 6, 'Matutino', 'Inscrito', true, 50.00),
    ('24050012', 'RODRIGUEZ MORALES CARLOS', 'Ing. Informatica', 1, 'Vespertino', 'Inscrito', false, 0.00),
    ('21050334', 'HERNANDEZ RAMIREZ SOFIA', 'Ing. Energias Renovables', 8, 'Matutino', 'Inscrito', false, 0.00)
ON CONFLICT (matricula) DO NOTHING;

-- 3. Catálogo de Conceptos de Cobro
CREATE TABLE IF NOT EXISTS conceptos_cobro (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    clave varchar(30) NOT NULL,
    descripcion varchar(200) NOT NULL,
    cuota_base numeric(12, 4) NOT NULL,
    aplica_beca boolean NOT NULL DEFAULT false,
    categoria varchar(50) NOT NULL,
    is_active boolean NOT NULL DEFAULT true
);

INSERT INTO conceptos_cobro (clave, descripcion, cuota_base, aplica_beca, categoria, is_active)
VALUES
    ('KARD', 'KARDEX', 150.00, false, 'Constancias', true),
    ('INSC', 'Pago de Inscripción (Periodo Vigente 2026)', 2600.00, true, 'Inscripción', true),
    ('INSC-ANT', 'Adeudo Inscripción (Año 2025)', 2450.00, false, 'Inscripción', true),
    ('VERA', 'Curso de Verano (Por Asignatura)', 1200.00, false, 'Cursos', true),
    ('CONST', 'Constancia de Estudios con Calificaciones', 100.00, false, 'Constancias', true),
    ('TITU', 'Trámite de Titulación Integral y Cédula', 3800.00, false, 'Otros', true)
ON CONFLICT DO NOTHING;

-- 4. Secuencia y Tabla de Recibos
CREATE SEQUENCE IF NOT EXISTS recibos_numero_recibo_seq START WITH 128450;

CREATE TABLE IF NOT EXISTS recibos (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    numero_recibo bigint NOT NULL DEFAULT nextval('recibos_numero_recibo_seq'),
    folio_administrativo varchar(50) NOT NULL,
    fecha_operacion timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    alumno_matricula varchar(20) NOT NULL,
    alumno_nombre varchar(160) NOT NULL,
    alumno_carrera varchar(100) NOT NULL,
    alumno_semestre integer NOT NULL,
    alumno_turno varchar(20) NOT NULL,
    metodo_pago varchar(30) NOT NULL,
    referencia_bancaria varchar(50),
    cuenta_bancaria varchar(50),
    fecha_bancaria timestamptz,
    subtotal numeric(12, 4) NOT NULL,
    descuento_total numeric(12, 4) NOT NULL,
    total numeric(12, 4) NOT NULL,
    estado varchar(20) NOT NULL,
    motivo_cancelacion varchar(500),
    fecha_cancelacion timestamptz,
    cajero_usuario varchar(64) NOT NULL,
    cajero_nombre varchar(160) NOT NULL,
    CONSTRAINT uq_recibos_numero UNIQUE (numero_recibo)
);

-- 5. Detalles de Recibo
CREATE TABLE IF NOT EXISTS detalles_recibo (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recibo_id uuid NOT NULL REFERENCES recibos(id) ON DELETE CASCADE,
    concepto_id uuid REFERENCES conceptos_cobro(id) ON DELETE SET NULL,
    descripcion varchar(200) NOT NULL,
    cantidad integer NOT NULL,
    cuota_unitario numeric(12, 4) NOT NULL,
    descuento_beca numeric(12, 4) NOT NULL,
    importe_total numeric(12, 4) NOT NULL,
    periodo_ano varchar(20)
);

COMMIT;
