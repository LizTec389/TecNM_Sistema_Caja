# Spec backend: Módulo de Control de Caja

## 1. Alcance y Capacidades
- Búsqueda y consulta de estudiantes (matrícula, nombre, carrera, semestre, beca).
- Catálogo de conceptos de cobro vigentes e importes base.
- Emisión de recibos y cobros (efectivo y depósito bancario 5+5).
- Cancelación administrativa justificada de recibos.
- Consulta de corte diario y recibos por turno de cajero.

## 2. Endpoints
- `GET /api/caja/alumnos/buscar?q={query}` -> `200 OK [AlumnoDTO]`
- `GET /api/caja/alumnos/{matricula}` -> `200 OK AlumnoDTO` | `404 Not Found`
- `GET /api/caja/conceptos` -> `200 OK [ConceptoDTO]`
- `POST /api/caja/recibos` -> `201 Created ReciboResponse` | `400 Bad Request`
- `POST /api/caja/recibos/previo` -> `201 Created ReciboResponse` | `400 Bad Request`
- `POST /api/caja/recibos/{id}/cancelar` -> `200 OK ReciboResponse` | `400 Bad Request` | `404 Not Found`
- `GET /api/caja/recibos/{numero}` -> `200 OK ReciboResponse` | `404 Not Found`
- `GET /api/caja/recibos/recientes` -> `200 OK [ReciboResponse]`
- `GET /api/caja/corte` -> `200 OK CorteCajaResponse`

## 3. Reglas de Negocio
- Folio administrativo con formato: `FOL-{Año}-{Mes}-{NumeroRecibo}`.
- Validación de cancelación: motivo formal obligatorio de al menos 5 caracteres.
- Preservación histórica: las cancelaciones cambian el estado a `CANCELADO` y registran timestamp y usuario; nunca eliminan físicamente el registro.
- Autorización: Requiere autenticación con JWT mediante cookie `auth_token`.

