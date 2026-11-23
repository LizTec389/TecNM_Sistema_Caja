-- ============================================================================
-- BASE DE DATOS OFICIAL: CAJADB - TECNM CAMPUS MONCLOVA
-- Sistema de Control de Caja Universitaria
-- Modelo relacional de 13 tablas maestras, vistas para reportes/corte diario,
-- índices de alto rendimiento y carga completa de datos semilla históricos.
-- ============================================================================

BEGIN;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ----------------------------------------------------------------------------
-- 1. CARRERAS (Catálogo Oficial de Oferta Académica TecNM)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS carreras (
    id_carrera      INT PRIMARY KEY,
    carrera         VARCHAR(50) NOT NULL,        -- Abreviatura oficial (ej. 'Ing. Industrial')
    nombre_carrera  VARCHAR(150) NOT NULL,       -- Nombre institucional completo
    titulo          VARCHAR(150),                -- Grado que otorga
    activo          BOOLEAN NOT NULL DEFAULT TRUE
);

INSERT INTO carreras (id_carrera, carrera, nombre_carrera, titulo) VALUES
(64, 'Lic. Informática',          'LICENCIATURA EN INFORMATICA',          'LICENCIADO EN INFORMATICA'),
(65, 'Ing. Mecánica',             'INGENIERIA MECANICA',                  'INGENIERO MECANICO'),
(67, 'Ing. Industrial',           'INGENIERIA INDUSTRIAL',                'INGENIERO INDUSTRIAL'),
(68, 'Ing. Electrónica',          'INGENIERIA ELECTRONICA',               'INGENIERO ELECTRONICO'),
(69, 'Ing. Informática',          'INGENIERIA INFORMATICA',               'INGENIERO INFORMATICO'),
(70, 'Ing. Gestión Emp.',         'INGENIERIA EN GESTION EMPRESARIAL',    'INGENIERO EN GESTION EMPRESARIAL'),
(71, 'Ing. Energías Renovables',  'INGENIERIA EN ENERGIAS RENOVABLES',    'INGENIERO EN ENERGIAS RENOVABLES'),
(74, 'Ing. Mecatrónica',          'INGENIERIA MECATRONICA',               'INGENIERO MECATRONICO')
ON CONFLICT (id_carrera) DO UPDATE SET
    carrera = EXCLUDED.carrera,
    nombre_carrera = EXCLUDED.nombre_carrera,
    titulo = EXCLUDED.titulo;

-- ----------------------------------------------------------------------------
-- 2. PERIODOS ACADÉMICOS (Ciclos para asociar cobros, adeudos y vigencias)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS periodos_academicos (
    id_periodo   SERIAL PRIMARY KEY,
    codigo       VARCHAR(30) UNIQUE NOT NULL,    -- ej. '2012-1', '2025-2', '2026-1'
    descripcion  VARCHAR(150),
    anio         SMALLINT NOT NULL,
    ciclo        VARCHAR(20) NOT NULL CHECK (ciclo IN ('PRIMAVERA', 'OTONO', 'VERANO', 'VERANILLO')),
    fecha_inicio DATE,
    fecha_fin    DATE,
    activo       BOOLEAN NOT NULL DEFAULT TRUE
);

INSERT INTO periodos_academicos (codigo, descripcion, anio, ciclo, fecha_inicio, fecha_fin, activo) VALUES
('2012-1', 'SEMESTRE ENERO-JUNIO 2012',        2012, 'PRIMAVERA', '2012-01-09', '2012-06-30', false),
('2016-1', 'SEMESTRE ENERO-JUNIO 2016',        2016, 'PRIMAVERA', '2016-01-11', '2016-06-30', false),
('2016-2', 'SEMESTRE AGOSTO-ENERO 2016-2017',  2016, 'OTONO',     '2016-08-08', '2017-01-31', false),
('2024-2', 'SEMESTRE AGOSTO-DICIEMBRE 2024',   2024, 'OTONO',     '2024-08-12', '2024-12-20', false),
('2025-1', 'SEMESTRE ENERO-JUNIO 2025',        2025, 'PRIMAVERA', '2025-01-13', '2025-06-27', false),
('2025-2', 'SEMESTRE AGOSTO-DICIEMBRE 2025',   2025, 'OTONO',     '2025-08-11', '2025-12-19', false),
('2026-1', 'SEMESTRE ENERO-JUNIO 2026',        2026, 'PRIMAVERA', '2026-01-12', '2026-06-30', true)
ON CONFLICT (codigo) DO UPDATE SET
    descripcion = EXCLUDED.descripcion,
    anio = EXCLUDED.anio,
    ciclo = EXCLUDED.ciclo,
    activo = EXCLUDED.activo;

-- ----------------------------------------------------------------------------
-- 3. USUARIOS Y EMPLEADOS DEL SISTEMA (Cuentas de Caja y Administración)
-- Password unificado predeterminado para todos: Password123!
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username    VARCHAR(64) UNIQUE NOT NULL,
    email       VARCHAR(254) UNIQUE NOT NULL,
    password    VARCHAR(512) NOT NULL,
    name        VARCHAR(160) NOT NULL,
    role        VARCHAR(20) NOT NULL DEFAULT 'Cajero' CHECK (role IN ('Admin', 'User', 'Cajero', 'Auditor')),
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO users (id, username, email, password, name, role, is_active) VALUES
('10000000-0000-4000-8000-000000000001', 'admin',   'admin@tecnm.local',   'AQAAAAIAAYagAAAAEPjbneKeORd2reFIG62af0ufOMPtwf64j0jpBznNBo3Y8K/BUoUW+IhJOm0b5iqWOg==', 'Administrador General', 'Admin',  true),
('10000000-0000-4000-8000-000000000002', 'cajero1', 'cajero1@tecnm.local', 'AQAAAAIAAYagAAAAECBhXWISKf+YayCz0xB8E4vyjO6chAfRLTq+f2eA4ztdtdDqA99jr+VsfZXUoKbfdQ==', 'Cajero Turno Matutino', 'Cajero', true),
('10000000-0000-4000-8000-000000000003', 'cajero2', 'cajero2@tecnm.local', 'AQAAAAIAAYagAAAAECBhXWISKf+YayCz0xB8E4vyjO6chAfRLTq+f2eA4ztdtdDqA99jr+VsfZXUoKbfdQ==', 'Cajero Turno Nocturno', 'Cajero', true),
('10000000-0000-4000-8000-000000000004', 'usuario', 'usuario@tecnm.local', 'AQAAAAIAAYagAAAAECBhXWISKf+YayCz0xB8E4vyjO6chAfRLTq+f2eA4ztdtdDqA99jr+VsfZXUoKbfdQ==', 'Usuario Operativo Demo', 'User',  true)
ON CONFLICT (username) DO UPDATE SET
    email = EXCLUDED.email,
    password = EXCLUDED.password,
    name = EXCLUDED.name,
    role = EXCLUDED.role,
    is_active = EXCLUDED.is_active;

-- ----------------------------------------------------------------------------
-- 4. ALUMNOS (Padrón Maestro de Estudiantes y Antecedentes Académicos)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS alumnos (
    matricula       VARCHAR(20) PRIMARY KEY,
    nombre_completo VARCHAR(160) NOT NULL,
    nombres         VARCHAR(100),
    apellidos       VARCHAR(100),
    sexo            CHAR(1),
    id_carrera      INT REFERENCES carreras(id_carrera),
    carrera         VARCHAR(100) NOT NULL,
    semestre        INTEGER NOT NULL,
    turno           VARCHAR(20) NOT NULL CHECK (turno IN ('Matutino', 'Nocturno', 'Vespertino', 'Sabatino')),
    numero_ficha    VARCHAR(30),                 -- Número de ficha de admisión
    cuenta_a        INT,                         -- Cuenta administrativa escolar
    estatus         VARCHAR(30) NOT NULL DEFAULT 'Inscrito' CHECK (estatus IN ('Inscrito', 'No Inscrito', 'Baja Temporal', 'Baja Definitiva', 'Egresado')),
    tiene_beca      BOOLEAN NOT NULL DEFAULT FALSE,
    porcentaje_beca NUMERIC(5, 2) NOT NULL DEFAULT 0.00,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_alumnos_nombre ON alumnos(nombre_completo);
CREATE INDEX IF NOT EXISTS idx_alumnos_carrera ON alumnos(id_carrera);
CREATE INDEX IF NOT EXISTS idx_alumnos_estatus ON alumnos(estatus);

INSERT INTO alumnos (matricula, nombre_completo, nombres, apellidos, sexo, id_carrera, carrera, semestre, turno, numero_ficha, cuenta_a, estatus, tiene_beca, porcentaje_beca)
VALUES
    ('G2605048',  'MARTINEZ SILVA MARIA JOSE',           'MARIA JOSE',           'MARTINEZ SILVA',          'F', 70, 'Ing. Gestión Emp.',         1, 'Matutino',   'F-2026-001', 24468, 'Inscrito', false, 0.00),
    ('M2505057',  'PALOS DIAZ MARIA FERNADA',            'MARIA FERNANDA',       'PALOS DIAZ',              'F', 67, 'Ing. Industrial',           1, 'Matutino',   'F-2025-089', 23877, 'Inscrito', false, 0.00),
    ('N2305000',  'CHAPOY LIMON CARLOS',                 'CARLOS',               'CHAPOY LIMON',            'M', 67, 'Ing. Industrial',           7, 'Matutino',   'F-2023-014', 11670, 'Inscrito', false, 0.00),
    ('120050011', 'DE LA TORRE CAMPOS DANIEL ARTURO',    'DANIEL ARTURO',        'DE LA TORRE CAMPOS',      'M', 69, 'Ing. Informática',          8, 'Matutino',   'F-2020-050', 12121, 'Inscrito', false, 0.00),
    ('120050012', 'DEL TORO LIRA FERNANDA',              'FERNANDA',             'DEL TORO LIRA',           'F', 69, 'Ing. Informática',          8, 'Matutino',   'F-2020-051', 11671, 'Inscrito', false, 0.00),
    ('120050013', 'DIAZ COSTILLA ANGEL GABRIEL',         'ANGEL GABRIEL',        'DIAZ COSTILLA',           'M', 69, 'Ing. Informática',          8, 'Matutino',   'F-2020-052', 12122, 'Inscrito', false, 0.00),
    ('N13050045', 'PEREZ SIFUENTES ALDO ULISES',         'ALDO ULISES',          'PEREZ SIFUENTES',         'M', 67, 'Ing. Industrial',           6, 'Matutino',   'F-2019-112', NULL,  'Inscrito', false, 0.00),
    ('N13050041', 'ORTIZ DURON LUIS MANUEL',             'LUIS MANUEL',          'ORTIZ DURON',             'M', 67, 'Ing. Industrial',           6, 'Matutino',   'F-2019-110', NULL,  'Inscrito', false, 0.00),
    ('G13050156', 'DAVILA HERNANDEZ ANA PATRICIA',       'ANA PATRICIA',         'DAVILA HERNANDEZ',        'F', 70, 'Ing. Gestión Emp.',         5, 'Matutino',   'F-2019-215', NULL,  'Inscrito', false, 0.00),
    ('E15050004', 'REQUENA RODRIGUEZ NESTOR OMAR',       'NESTOR OMAR',          'REQUENA RODRIGUEZ',       'M', 68, 'Ing. Electrónica',          3, 'Nocturno',   'F-2021-004', NULL,  'Inscrito', false, 0.00),
    ('N12050104', 'BARRERA TOVAR JESUS ENRIQUE',         'JESUS ENRIQUE',        'BARRERA TOVAR',           'M', 67, 'Ing. Industrial',           8, 'Nocturno',   'F-2018-090', NULL,  'Inscrito', false, 0.00),
    ('I11050186', 'GUZMAN GUAJARDO BRANDO ISRAEL',       'BRANDO ISRAEL',        'GUZMAN GUAJARDO',         'M', 69, 'Ing. Informática',          2, 'Matutino',   'F-2017-040', NULL,  'Inscrito', false, 0.00),
    ('I11050180', 'DELGADILLO GELACIO IRMA BERENICE',    'IRMA BERENICE',        'DELGADILLO GELACIO',      'F', 69, 'Ing. Informática',          1, 'Nocturno',   'F-2017-038', NULL,  'Inscrito', true,  50.00),
    ('I11050199', 'OROZCO ALVARADO KAREN',               'KAREN',                'OROZCO ALVARADO',         'F', 69, 'Ing. Informática',          1, 'Matutino',   'F-2017-062', NULL,  'Inscrito', true,  25.00),
    ('E11050286', 'MARTINEZ ALFARO LUIS ANGEL',          'LUIS ANGEL',           'MARTINEZ ALFARO',         'M', 68, 'Ing. Electrónica',          1, 'Matutino',   'F-2017-080', NULL,  'Baja Temporal', false, 0.00),
    ('23050243',  'ORTEGON HERNANDEZ KLEBER',            'KLEBER',               'ORTEGON HERNANDEZ',       'M', 67, 'Ing. Industrial',           4, 'Matutino',   'F-2023-243', 23050, 'Inscrito', false, 0.00),
    ('23050118',  'GARCIA ZAVALA LUIS FERNANDO',         'LUIS FERNANDO',        'GARCIA ZAVALA',           'M', 74, 'Ing. Mecatrónica',          3, 'Matutino',   'F-2023-118', 23051, 'Inscrito', false, 0.00),
    ('22050890',  'LOPEZ SANCHEZ MARIANA',               'MARIANA',              'LOPEZ SANCHEZ',           'F', 70, 'Ing. Gestión Emp.',         6, 'Matutino',   'F-2022-890', 22050, 'Inscrito', true,  50.00),
    ('24050012',  'RODRIGUEZ MORALES CARLOS',            'CARLOS',               'RODRIGUEZ MORALES',       'M', 69, 'Ing. Informática',          1, 'Vespertino', 'F-2024-012', 24050, 'Inscrito', false, 0.00),
    ('21050334',  'HERNANDEZ RAMIREZ SOFIA',             'SOFIA',                'HERNANDEZ RAMIREZ',       'F', 71, 'Ing. Energías Renovables',  8, 'Matutino',   'F-2021-334', 21050, 'Egresado', false, 0.00)
ON CONFLICT (matricula) DO UPDATE SET
    nombre_completo = EXCLUDED.nombre_completo,
    id_carrera = EXCLUDED.id_carrera,
    carrera = EXCLUDED.carrera,
    semestre = EXCLUDED.semestre,
    turno = EXCLUDED.turno,
    estatus = EXCLUDED.estatus,
    tiene_beca = EXCLUDED.tiene_beca,
    porcentaje_beca = EXCLUDED.porcentaje_beca;

-- ----------------------------------------------------------------------------
-- 5. HISTORIAL DE CAMBIO DE MATRÍCULAS (Protección de Trazabilidad Histórica)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS alumno_matriculas_historial (
    id_historial      BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    matricula_alumno  VARCHAR(20) NOT NULL REFERENCES alumnos(matricula) ON DELETE CASCADE,
    matricula_previa  VARCHAR(30) NOT NULL,
    id_periodo        INT REFERENCES periodos_academicos(id_periodo),
    fecha_registro    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    motivo_cambio     TEXT
);

-- ----------------------------------------------------------------------------
-- 6. BECAS Y DESCUENTOS (Reglas y Asignaciones Económicas)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS becas_descuentos (
    id_beca             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    matricula           VARCHAR(20) NOT NULL REFERENCES alumnos(matricula) ON DELETE CASCADE,
    id_periodo          INT NOT NULL REFERENCES periodos_academicos(id_periodo),
    tipo_beca           VARCHAR(50) NOT NULL DEFAULT 'ACADEMICA',
    porcentaje          NUMERIC(5, 2) DEFAULT 0.00 CHECK (porcentaje BETWEEN 0 AND 100),
    monto_fijo          NUMERIC(12, 4) DEFAULT 0.00,
    motivo              VARCHAR(250),
    activa              BOOLEAN NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO becas_descuentos (matricula, id_periodo, tipo_beca, porcentaje, monto_fijo, motivo, activa)
VALUES
    ('22050890', (SELECT id_periodo FROM periodos_academicos WHERE codigo = '2026-1'), 'ACADEMICA', 50.00, 0.00, 'Promedio superior a 95', true),
    ('I11050180', (SELECT id_periodo FROM periodos_academicos WHERE codigo = '2012-1'), 'APOYO_INSTITUCIONAL', 5.88, 100.00, 'Convenio sindical', true)
ON CONFLICT DO NOTHING;

-- ----------------------------------------------------------------------------
-- 7. ADEUDOS DE ESTUDIANTES (Permite deudas multianuales bajo el mismo concepto)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS adeudos (
    id_adeudo           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    matricula           VARCHAR(20) NOT NULL REFERENCES alumnos(matricula) ON DELETE CASCADE,
    concepto_cobro      VARCHAR(100) NOT NULL,       -- Denominación uniforme (ej. 'Pago de Inscripción')
    id_periodo          INT NOT NULL REFERENCES periodos_academicos(id_periodo),
    monto_original      NUMERIC(12, 4) NOT NULL,
    saldo_pendiente     NUMERIC(12, 4) NOT NULL,
    fecha_limite        DATE,
    estado              VARCHAR(20) NOT NULL DEFAULT 'PENDIENTE' CHECK (estado IN ('PENDIENTE', 'PARCIAL', 'LIQUIDADO', 'CONDONADO')),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_adeudos_alumno ON adeudos(matricula);
CREATE INDEX IF NOT EXISTS idx_adeudos_periodo ON adeudos(id_periodo);

INSERT INTO adeudos (matricula, concepto_cobro, id_periodo, monto_original, saldo_pendiente, fecha_limite, estado)
VALUES
    ('23050243', 'Pago de Inscripción', (SELECT id_periodo FROM periodos_academicos WHERE codigo = '2025-2'), 2600.00, 2600.00, '2025-10-30', 'PENDIENTE'),
    ('23050243', 'Pago de Inscripción', (SELECT id_periodo FROM periodos_academicos WHERE codigo = '2025-1'), 2450.00, 1000.00, '2025-04-30', 'PARCIAL'),
    ('N2305000', 'Curso de Verano',     (SELECT id_periodo FROM periodos_academicos WHERE codigo = '2024-2'), 1200.00, 1200.00, '2024-07-15', 'PENDIENTE')
ON CONFLICT DO NOTHING;

-- ----------------------------------------------------------------------------
-- 8. CONCEPTOS DE COBRO (Catálogo Maestro de Aranceles Institucionales)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS conceptos_cobro (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    clave             VARCHAR(30) UNIQUE NOT NULL,  -- Clave o cuenta contable ('5200-...', 'KARD')
    descripcion       VARCHAR(200) NOT NULL,
    cuota_base        NUMERIC(12, 4) NOT NULL,
    orden             INT,
    cuenta_referencia VARCHAR(30),
    aplica_beca       BOOLEAN NOT NULL DEFAULT FALSE,
    categoria         VARCHAR(50) NOT NULL DEFAULT 'Inscripción',
    is_active         BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_conceptos_clave ON conceptos_cobro(clave);
CREATE INDEX IF NOT EXISTS idx_conceptos_categoria ON conceptos_cobro(categoria);

INSERT INTO conceptos_cobro (clave, descripcion, cuota_base, orden, cuenta_referencia, aplica_beca, categoria, is_active) VALUES
-- Cuentas de Inscripción y Reinscripción
('5200-001-002-004', 'INSCRIPCION SEM. AGO 2012-ENERO 2013',        1700.00, 1, NULL, true,  'Inscripción', true),
('5200-001-002-021', 'REINSCRIPCION SEM. AGOSTO 2022-ENERO 2023',    500.00, 2, NULL, true,  'Inscripción', true),
('5200-001-002-051', 'REINSCRIPCION SEM. AGOSTO 2025-ENERO 2026',    500.00, 3, NULL, true,  'Inscripción', true),
('5200-001-002-082', 'REINSCRIPCION SEM FEB-JUL 2023',               500.00, 4, NULL, true,  'Inscripción', true),
('5200-002-001-001', 'INSCRIPCIONES GENERALES',                      1350.00, 5, NULL, true,  'Inscripción', true),
('INSC-VIG',         'Pago de Inscripción (Periodo Vigente 2026)',   2600.00, 6, NULL, true,  'Inscripción', true),
('INSC-ANT',         'Adeudo Inscripción Periodos Anteriores',       2450.00, 7, NULL, false, 'Inscripción', true),
-- Tarifas de Reinscripción Hoja PREVIO
('5400-001-003-0001', 'REINSCRIPCION SEMESTRAL TARIFA 1',             520.00, 10, NULL, false, 'Inscripción', true),
('5400-001-003-0002', 'REINSCRIPCION SEMESTRAL TARIFA 2',             620.00, 11, NULL, false, 'Inscripción', true),
('5400-001-003-0003', 'REINSCRIPCION SEMESTRAL TARIFA 3',             620.00, 12, NULL, false, 'Inscripción', true),
('5400-001-003-0005', 'REINSCRIPCION SEMESTRAL TARIFA 5',             700.00, 13, NULL, false, 'Inscripción', true),
('5400-001-003-0006', 'REINSCRIPCION SEMESTRAL TARIFA 6',             700.00, 14, NULL, false, 'Inscripción', true),
-- Tarifas Hoja RECIBO
('5400-001-002-0037', 'INSCRIPCION SEMESTRAL 2012',                  1700.00, 20, NULL, true,  'Inscripción', true),
('5400-001-002-0038', 'DESCUENTO DE INSCRIPCION',                    -100.00, 21, NULL, false, 'Descuentos',  true),
-- Servicios Universitarios y Cursos
('KARD',              'KARDEX OFICIAL',                               150.00, 30, NULL, false, 'Constancias',  true),
('CONST',             'Constancia de Estudios con Calificaciones',    100.00, 31, NULL, false, 'Constancias',  true),
('VERA',              'Curso de Verano (Por Asignatura)',            1200.00, 32, NULL, false, 'Cursos',       true),
('TITU',              'Trámite de Titulación Integral y Cédula',     3800.00, 33, NULL, false, 'Otros',        true)
ON CONFLICT (clave) DO UPDATE SET
    descripcion = EXCLUDED.descripcion,
    cuota_base = EXCLUDED.cuota_base,
    aplica_beca = EXCLUDED.aplica_beca,
    categoria = EXCLUDED.categoria,
    is_active = EXCLUDED.is_active;

-- ----------------------------------------------------------------------------
-- 9. FOLIOS ADMINISTRATIVOS (Relación 1 Folio -> N Recibos)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS folios (
    numero_folio    BIGINT PRIMARY KEY,
    serie           VARCHAR(10) NOT NULL DEFAULT 'A',
    fecha_creacion  TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    observaciones   TEXT
);

INSERT INTO folios (numero_folio, serie) VALUES
(1, 'A'), (2, 'A'), (3, 'A'), (4, 'A'), (5, 'A'), (9, 'A'), (10, 'A'), (12, 'A'), (14, 'A'), (18, 'A'),
(2922, 'B'), (3354, 'B'), (3661, 'B'), (3662, 'B'), (3692, 'B'),
(5001, 'C'), (5002, 'C'), (5003, 'C')
ON CONFLICT (numero_folio) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 10. PÓLIZAS DE INGRESO (Cortes diarios, Arqueos y Concentración Contable)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS polizas_ingreso (
    id_poliza       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    numero_poliza   VARCHAR(50) UNIQUE NOT NULL,
    fecha_poliza    DATE NOT NULL DEFAULT CURRENT_DATE,
    total_recaudado NUMERIC(12, 4) NOT NULL DEFAULT 0.00,
    cajero_usuario  VARCHAR(64) NOT NULL,
    estado          VARCHAR(20) NOT NULL DEFAULT 'ABIERTA' CHECK (estado IN ('ABIERTA', 'CONSOLIDADA', 'CANCELADA')),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO polizas_ingreso (numero_poliza, fecha_poliza, total_recaudado, cajero_usuario, estado) VALUES
('POL-2016-06-001', '2016-06-07', 2880.00, 'cajero1', 'CONSOLIDADA'),
('POL-2016-06-002', '2016-06-08', 3260.00, 'cajero1', 'CONSOLIDADA'),
('POL-2012-01-001', '2012-01-23', 6500.00, 'cajero2', 'CONSOLIDADA')
ON CONFLICT (numero_poliza) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 11. RECIBOS DE CAJA (Encabezado con Snapshot Histórico Inmutable)
-- ----------------------------------------------------------------------------
CREATE SEQUENCE IF NOT EXISTS recibos_numero_recibo_seq START WITH 128450;

CREATE TABLE IF NOT EXISTS recibos (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    numero_recibo        BIGINT NOT NULL DEFAULT nextval('recibos_numero_recibo_seq') UNIQUE,
    folio_administrativo VARCHAR(50) NOT NULL,      -- Soporta que varios recibos compartan folio
    fecha_operacion      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_efectiva_pago  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    alumno_matricula     VARCHAR(20) NOT NULL REFERENCES alumnos(matricula),
    alumno_nombre        VARCHAR(160) NOT NULL,      -- Snapshot histórico
    alumno_carrera       VARCHAR(100) NOT NULL,      -- Snapshot histórico
    alumno_semestre      INTEGER NOT NULL,           -- Snapshot histórico
    alumno_turno         VARCHAR(20) NOT NULL,       -- Snapshot histórico
    metodo_pago          VARCHAR(30) NOT NULL,       -- EFECTIVO, DEPOSITO_BANCARIO, etc.
    referencia_bancaria  VARCHAR(50),                -- No es UNIQUE, puede repetirse
    cuenta_bancaria      VARCHAR(50),
    fecha_bancaria       TIMESTAMPTZ,
    subtotal             NUMERIC(12, 4) NOT NULL,
    descuento_total      NUMERIC(12, 4) NOT NULL,
    total                NUMERIC(12, 4) NOT NULL,
    status               SMALLINT NOT NULL DEFAULT 1,-- 1=Activo, 0=Cancelado
    estado               VARCHAR(20) NOT NULL DEFAULT 'REGISTRADO', -- 'REGISTRADO', 'Vigente', 'PREVIO', 'CANCELADO', 'BLOQUEADO'
    tipo_recibo          VARCHAR(20) NOT NULL DEFAULT 'DEFINITIVO' CHECK (tipo_recibo IN ('PREVIO', 'DEFINITIVO')),
    is_locked            BOOLEAN NOT NULL DEFAULT FALSE,
    poliza_id            BIGINT REFERENCES polizas_ingreso(id_poliza),
    motivo_cancelacion   VARCHAR(500),
    fecha_cancelacion    TIMESTAMPTZ,
    cajero_usuario       VARCHAR(64) NOT NULL,
    cajero_nombre        VARCHAR(160) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_recibos_numero ON recibos(numero_recibo);
CREATE INDEX IF NOT EXISTS idx_recibos_folio ON recibos(folio_administrativo);
CREATE INDEX IF NOT EXISTS idx_recibos_matricula ON recibos(alumno_matricula);
CREATE INDEX IF NOT EXISTS idx_recibos_referencia ON recibos(referencia_bancaria);
CREATE INDEX IF NOT EXISTS idx_recibos_fecha ON recibos(fecha_operacion);
CREATE INDEX IF NOT EXISTS idx_recibos_estado ON recibos(estado);

-- Poblado de Recibos PREVIOS (Turno 2016)
INSERT INTO recibos (
    id, folio_administrativo, fecha_operacion, alumno_matricula, alumno_nombre, alumno_carrera, alumno_semestre, alumno_turno,
    metodo_pago, referencia_bancaria, subtotal, descuento_total, total, status, estado, tipo_recibo, cajero_usuario, cajero_nombre
) VALUES
('b0000000-0000-4000-8000-000000000001', '1',  '2016-06-07 08:30:00-06', 'N13050045', 'PEREZ SIFUENTES ALDO ULISES',         'Ing. Industrial',          6, 'Matutino', 'EFECTIVO', '0', 620.00, 0.00, 620.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000002', '2',  '2016-06-07 09:15:00-06', 'N13050041', 'ORTIZ DURON LUIS MANUEL',             'Ing. Industrial',          6, 'Matutino', 'EFECTIVO', '0', 620.00, 0.00, 620.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000003', '3',  '2016-06-07 10:00:00-06', 'G13050156', 'DAVILA HERNANDEZ ANA PATRICIA',       'Ing. Gestión Emp.',         5, 'Matutino', 'EFECTIVO', '0', 620.00, 0.00, 620.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000004', '4',  '2016-06-07 11:20:00-06', 'E15050004', 'REQUENA RODRIGUEZ NESTOR OMAR',       'Ing. Electrónica',         3, 'Nocturno', 'EFECTIVO', '0', 520.00, 0.00, 520.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000005', '5',  '2016-06-07 12:45:00-06', 'N12050104', 'BARRERA TOVAR JESUS ENRIQUE',         'Ing. Industrial',          8, 'Nocturno', 'EFECTIVO', '0', 520.00, 0.00, 520.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000006', '9',  '2016-06-08 08:45:00-06', 'G13050219', 'HERNANDEZ CASTILLO ARELY SARAHI',     'Ing. Gestión Emp.',         6, 'Matutino', 'EFECTIVO', '0', 620.00, 0.00, 620.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000007', '10', '2016-06-08 09:30:00-06', 'G14050252', 'MUÑOZ DE LEON ILSE ARACELI',          'Ing. Gestión Emp.',         4, 'Matutino', 'EFECTIVO', '0', 620.00, 0.00, 620.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000008', '12', '2016-06-08 10:15:00-06', 'N15050011', 'VILLASTRIGO GARCIA SABINO EMMANUEL',  'Ing. Industrial',          2, 'Matutino', 'EFECTIVO', '0', 700.00, 0.00, 700.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000009', '14', '2016-06-08 11:10:00-06', 'G13050164', 'GUTIERREZ SALAZAR CLAUDIA MONSERRAT', 'Ing. Gestión Emp.',         6, 'Matutino', 'EFECTIVO', '0', 620.00, 0.00, 620.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino'),
('b0000000-0000-4000-8000-000000000010', '18', '2016-06-08 12:00:00-06', 'N12050487', 'IBARRA MARTINEZ JULIO CESAR',         'Ing. Industrial',          8, 'Nocturno', 'EFECTIVO', '0', 700.00, 0.00, 700.00, 1, 'PREVIO', 'PREVIO', 'cajero1', 'Cajero Turno Matutino')
ON CONFLICT (id) DO NOTHING;

-- Poblado de Recibos DEFINITIVOS (Turno 2012 e inscripción actual)
INSERT INTO recibos (
    id, folio_administrativo, fecha_operacion, alumno_matricula, alumno_nombre, alumno_carrera, alumno_semestre, alumno_turno,
    metodo_pago, referencia_bancaria, subtotal, descuento_total, total, status, estado, tipo_recibo, cajero_usuario, cajero_nombre
) VALUES
('c0000000-0000-4000-8000-000000000001', '3692', '2012-01-23 10:00:00-06', 'I11050186', 'GUZMAN GUAJARDO BRANDO ISRAEL',        'Ing. Informática', 2, 'Matutino', 'DEPOSITO_BANCARIO', 'REF-2012-001', 1700.00,   0.00, 1700.00, 1, 'REGISTRADO', 'DEFINITIVO', 'cajero2', 'Cajero Turno Nocturno'),
('c0000000-0000-4000-8000-000000000002', '3354', '2012-01-23 10:30:00-06', 'I11050180', 'DELGADILLO GELACIO IRMA BERENICE',     'Ing. Informática', 1, 'Nocturno', 'DEPOSITO_BANCARIO', 'REF-2012-002', 1700.00, 100.00, 1600.00, 1, 'REGISTRADO', 'DEFINITIVO', 'cajero2', 'Cajero Turno Nocturno'),
('c0000000-0000-4000-8000-000000000003', '3661', '2012-01-23 11:00:00-06', 'I11050199', 'OROZCO ALVARADO KAREN',                'Ing. Informática', 1, 'Matutino', 'DEPOSITO_BANCARIO', 'REF-2012-003', 1700.00, 100.00, 1600.00, 1, 'REGISTRADO', 'DEFINITIVO', 'cajero2', 'Cajero Turno Nocturno'),
('c0000000-0000-4000-8000-000000000004', '2922', '2012-01-23 11:30:00-06', 'E11050286', 'MARTINEZ ALFARO LUIS ANGEL',           'Ing. Electrónica', 1, 'Matutino', 'DEPOSITO_BANCARIO', 'REF-2012-004', 1700.00, 100.00, 1600.00, 0, 'CANCELADO',  'DEFINITIVO', 'cajero1', 'Cajero Turno Matutino'),
('c0000000-0000-4000-8000-000000000005', '3662', '2012-01-23 12:00:00-06', 'E11050559', 'FLORES EVANGELISTA ESTEBAN DAMIAN',    'Ing. Electrónica', 1, 'Nocturno', 'DEPOSITO_BANCARIO', 'REF-2012-005', 1700.00, 100.00, 1600.00, 1, 'REGISTRADO', 'DEFINITIVO', 'cajero2', 'Cajero Turno Nocturno')
ON CONFLICT (id) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 12. DETALLES DE RECIBO (Desglose de Partidas y Conceptos Facturados)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS detalles_recibo (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    recibo_id       UUID NOT NULL REFERENCES recibos(id) ON DELETE CASCADE,
    concepto_id     UUID REFERENCES conceptos_cobro(id) ON DELETE SET NULL,
    descripcion     VARCHAR(200) NOT NULL,
    cantidad        INTEGER NOT NULL,
    cuota_unitario  NUMERIC(12, 4) NOT NULL,
    descuento_beca  NUMERIC(12, 4) NOT NULL,
    importe_total   NUMERIC(12, 4) NOT NULL,
    periodo_ano     VARCHAR(20)
);

CREATE INDEX IF NOT EXISTS idx_detalles_recibo ON detalles_recibo(recibo_id);
CREATE INDEX IF NOT EXISTS idx_detalles_concepto ON detalles_recibo(concepto_id);

-- Cargar detalles de los recibos previos
INSERT INTO detalles_recibo (recibo_id, concepto_id, descripcion, cantidad, cuota_unitario, descuento_beca, importe_total, periodo_ano)
SELECT r.id, c.id, c.descripcion, 1, c.cuota_base, 0.00, c.cuota_base, '2016-1'
FROM recibos r
JOIN conceptos_cobro c ON (
    (r.folio_administrativo IN ('1','2','3','9','10','14') AND c.clave = '5400-001-003-0003') OR
    (r.folio_administrativo = '4' AND c.clave = '5400-001-003-0002') OR
    (r.folio_administrativo = '5' AND c.clave = '5400-001-003-0001') OR
    (r.folio_administrativo = '12' AND c.clave = '5400-001-003-0006') OR
    (r.folio_administrativo = '18' AND c.clave = '5400-001-003-0005')
)
WHERE r.tipo_recibo = 'PREVIO'
ON CONFLICT DO NOTHING;

-- Cargar detalles de los recibos definitivos (Inscripción base)
INSERT INTO detalles_recibo (recibo_id, concepto_id, descripcion, cantidad, cuota_unitario, descuento_beca, importe_total, periodo_ano)
SELECT r.id, c.id, c.descripcion, 1, 1700.00, 0.00, 1700.00, '2012-1'
FROM recibos r
CROSS JOIN conceptos_cobro c
WHERE r.tipo_recibo = 'DEFINITIVO' AND c.clave = '5400-001-002-0037'
ON CONFLICT DO NOTHING;

-- Cargar descuento en recibos donde aplica
INSERT INTO detalles_recibo (recibo_id, concepto_id, descripcion, cantidad, cuota_unitario, descuento_beca, importe_total, periodo_ano)
SELECT r.id, c.id, c.descripcion, 1, -100.00, 0.00, -100.00, '2012-1'
FROM recibos r
CROSS JOIN conceptos_cobro c
WHERE r.folio_administrativo IN ('3354','3661','2922','3662') AND c.clave = '5400-001-002-0038'
ON CONFLICT DO NOTHING;

-- ----------------------------------------------------------------------------
-- 13. MOVIMIENTOS DE PAGO (Auditoría Financiera, Medios de Pago y Conciliación)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS movimientos_pago (
    id_movimiento       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    recibo_id           UUID NOT NULL REFERENCES recibos(id) ON DELETE CASCADE,
    forma_pago          VARCHAR(30) NOT NULL CHECK (forma_pago IN ('EFECTIVO', 'DEPOSITO_BANCARIO', 'TRANSFERENCIA', 'TARJETA_DEBITO', 'TARJETA_CREDITO')),
    referencia_bancaria VARCHAR(100),
    monto               NUMERIC(12, 4) NOT NULL CHECK (monto > 0),
    fecha_movimiento    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    cajero_usuario      VARCHAR(64) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_movimientos_recibo ON movimientos_pago(recibo_id);
CREATE INDEX IF NOT EXISTS idx_movimientos_forma ON movimientos_pago(forma_pago);

INSERT INTO movimientos_pago (recibo_id, forma_pago, referencia_bancaria, monto, fecha_movimiento, cajero_usuario)
SELECT r.id, 'EFECTIVO', '0', r.total, r.fecha_operacion, r.cajero_usuario
FROM recibos r
WHERE r.tipo_recibo = 'PREVIO'
ON CONFLICT DO NOTHING;

INSERT INTO movimientos_pago (recibo_id, forma_pago, referencia_bancaria, monto, fecha_movimiento, cajero_usuario)
SELECT r.id, 'DEPOSITO_BANCARIO', r.referencia_bancaria, r.total, r.fecha_operacion, r.cajero_usuario
FROM recibos r
WHERE r.tipo_recibo = 'DEFINITIVO'
ON CONFLICT DO NOTHING;

-- ============================================================================
-- VISTAS INTELIGENTES PARA REPORTES Y EXPORTACIÓN A EXCEL
-- ============================================================================

-- Vista 1: Corte diario de Caja y Arqueo por Cajero
CREATE OR REPLACE VIEW vw_corte_diario_caja AS
SELECT 
    r.fecha_operacion::DATE AS fecha,
    r.cajero_usuario,
    r.cajero_nombre,
    m.forma_pago,
    COUNT(DISTINCT r.id) AS total_recibos_emitidos,
    COUNT(DISTINCT CASE WHEN r.estado = 'CANCELADO' THEN r.id END) AS recibos_cancelados,
    SUM(CASE WHEN r.estado != 'CANCELADO' THEN m.monto ELSE 0 END) AS total_recaudado
FROM recibos r
LEFT JOIN movimientos_pago m ON r.id = m.recibo_id
GROUP BY r.fecha_operacion::DATE, r.cajero_usuario, r.cajero_nombre, m.forma_pago;

-- Vista 2: Reporte detallado para filtros del Frontend y exportación a Excel
CREATE OR REPLACE VIEW vw_reporte_recibos_detallado AS
SELECT 
    r.id AS recibo_id,
    r.numero_recibo,
    r.folio_administrativo,
    r.fecha_operacion,
    r.fecha_efectiva_pago,
    r.alumno_matricula,
    r.alumno_nombre,
    r.alumno_carrera,
    r.alumno_semestre,
    r.alumno_turno,
    d.descripcion AS concepto_detalle,
    d.cantidad,
    d.cuota_unitario,
    d.descuento_beca,
    d.importe_total AS importe_renglon,
    r.total AS total_recibo,
    r.metodo_pago,
    r.referencia_bancaria,
    r.tipo_recibo,
    r.estado AS estado_recibo,
    r.cajero_usuario,
    r.cajero_nombre
FROM recibos r
JOIN detalles_recibo d ON r.id = d.recibo_id;

-- Vista 3: Consulta de adeudos de alumnos por ciclo escolar
CREATE OR REPLACE VIEW vw_adeudos_estudiantes AS
SELECT 
    a.id_adeudo,
    a.matricula,
    al.nombre_completo AS alumno_nombre,
    al.carrera,
    a.concepto_cobro,
    p.codigo AS periodo_codigo,
    p.anio AS periodo_anio,
    p.ciclo AS periodo_ciclo,
    a.monto_original,
    a.saldo_pendiente,
    a.fecha_limite,
    a.estado AS estado_adeudo
FROM adeudos a
JOIN alumnos al ON a.matricula = al.matricula
JOIN periodos_academicos p ON a.id_periodo = p.id_periodo;

COMMIT;
