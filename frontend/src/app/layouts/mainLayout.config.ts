import type { BrandOptions, NavigationItem } from '../../shared/components/navigation'

export const mainLayoutConfig = {
  showHeader: false,
  showSidebar: true,
  header: { title: 'TecNM Caja', subtitle: 'Campus Monclova', showTitle: true, showLogo: false } satisfies BrandOptions,
  sidebar: { title: 'Control de Caja', subtitle: 'TecNM Monclova', showTitle: true, showLogo: false } satisfies BrandOptions,
}

export const headerNavigation: NavigationItem[] = [
  { label: 'Caja Ventanilla', to: '/' },
  { label: 'Póliza de Ingresos', to: '/#poliza' },
  { label: 'Corte por Empleado', to: '/#corte' },
]

export const sidebarNavigation: NavigationItem[] = [
  {
    label: 'Control de Caja',
    children: [
      { label: 'Emisión de Cobro', to: '/' },
      { label: 'Consulta y Cancelación de Recibos', to: '/#cancelacion' },
      { label: 'Corte por Empleado', to: '/#corte' },
    ],
  },
  {
    label: 'Catálogos',
    children: [
      { label: 'Empleados', to: '/#empleados' },
      { label: 'Cuentas', to: '/#cuentas' },
    ],
  },
  {
    label: 'Impresión y Reportes',
    children: [
      { label: 'Recibo Oficial', to: '/#recibo' },
      { label: 'Póliza de Ingresos', to: '/#poliza' },
      { label: 'Reporte de Ingresos', to: '/#reporte-ingresos' },
      { label: 'Póliza de Ingreso por Periodo', to: '/#poliza-periodo' },
      { label: 'Reporte de Ingresos por Periodo', to: '/#ingresos-periodo' },
      { label: 'Reportes Varios', to: '/#reportes-varios' },
      { label: 'Reporte Deudores', to: '/#deudores' },
      { label: 'Reporte Deudores por Fecha de Corte', to: '/#deudores-corte' },
      { label: 'Reporte Deudores por Fecha y Cuenta', to: '/#deudores-cuenta' },
      { label: 'Reporte Deudores Inscritos', to: '/#deudores-inscritos' },
      { label: 'Depósito Bancario', to: '/#deposito-bancario' },
      { label: 'Depósito Bancario - Propedéutico', to: '/#deposito-propedeutico' },
      { label: 'Recibo Provisional', to: '/#recibo-provisional' },
      { label: 'Recibo Provisional Cancelar', to: '/#recibo-provisional-cancelar' },
      { label: 'Imprime Verano', to: '/#imprime-verano' },
    ],
  },
  {
    label: 'Utilerías',
    children: [
      { label: 'Crear Usuario', to: '/#crear-usuario' },
      { label: 'Cambio de Contraseña', to: '/#cambio-password' },
      { label: 'Cierre de Semestre', to: '/#cierre-semestre' },
    ],
  },
]
