# Especificación de Requerimiento: Paginación en Frontend

## 1. Observación / Problema detectado
- La vista del listado de datos realiza peticiones HTTP sin parámetros de limite ni página, intentando cargar la totalidad de los registros de un solo llamado.
- Esto genera tiempos de respuesta elevados, consumo innecesario de memoria en el navegador y una mala experiencia de usuario a medida que crece la base de datos.
- Falta la interfaz y la lógica de control para navegar secuencialmente entre páginas.

## 2. Corrección / Solución propuesta
- **Interfaz de Usuario (UI):** Agregar un componente de paginación que incluya botones para "Anterior", "Siguiente" e indicador de la página actual / total de páginas.
- **Integración con API:** Actualizar la capa de peticiones HTTP en el frontend (`api/`) para enviar los query params `page` y `limit` en cada solicitud GET.
- **Tipado TypeScript:** Extender las interfaces de modelo/DTO en `types/` para soportar la respuesta paginada devuelta por el servidor (ej. `{ items: [], total: number, page: number, totalPages: number }`).

## 3. Criterios de Aceptación
- La vista debe solicitar por defecto la página `1` con un límite de registros definido (ej. `10`).
- Los botones de navegación deben deshabilitarse adecuadamente (ej. deshabilitar "Anterior" en la primera página).
- El estado de la paginación debe reaccionar correctamente al cambiar de vista o aplicar filtros.