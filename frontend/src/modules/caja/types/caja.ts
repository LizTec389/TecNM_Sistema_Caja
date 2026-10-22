export type EstadoRecibo = 'REGISTRADO' | 'PREVIO' | 'CANCELADO' | 'BLOQUEADO'

export type MetodoPago = 'EFECTIVO' | 'DEPOSITO_BANCARIO'

export interface AlumnoCaja {
  matricula: string
  nombreCompleto: string
  carrera: string
  carreraId: number
  semestre: number
  turno: 'Matutino' | 'Vespertino' | 'Nocturno'
  estatus: 'Inscrito' | 'Baja Temporal' | 'Baja Definitiva' | 'Egresado'
  tieneBeca: boolean
  porcentajeBeca: number
  motivoBeca?: string
}

export interface ConceptoCatalogo {
  id: string
  clave: string
  descripcion: string
  cuotaBase: number
  aplicaBeca: boolean
  categoria: 'Inscripción' | 'Constancias' | 'Exámenes' | 'Cursos' | 'Otros'
  periodoRequerido?: boolean
}

export interface DetalleConceptoRecibo {
  id: string
  conceptoId: string
  descripcion: string
  cantidad: number
  cuotaUnitario: number
  descuentoBeca: number
  importeTotal: number
  periodoAno?: string
}

export interface ReciboCaja {
  id: string
  numeroRecibo: number
  folioAdministrativo: string
  fechaOperacion: string
  alumno: AlumnoCaja
  detalles: DetalleConceptoRecibo[]
  metodoPago: MetodoPago
  referenciaBancaria?: string
  fechaBancaria?: string
  cuentaBancaria?: string
  subtotal: number
  descuentoTotal: number
  total: number
  estado: EstadoRecibo
  motivoCancelacion?: string
  fechaCancelacion?: string
  cajeroNombre: string
  cajaNumero: string
}

export interface ResumenCajaTurno {
  cajero: string
  caja: string
  turno: string
  fecha: string
  totalCobrado: number
  recibosEmitidos: number
  recibosCancelados: number
  recibosPrevios: number
}

