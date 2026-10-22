<script setup lang="ts">
import { computed, ref } from 'vue'
import { UiAlert, UiBadge, UiButton, UiCard, UiDialog, UiField } from '../../../shared/components'
import { useTheme } from '../../../shared/composables/useTheme'
import type { AlumnoCaja, ConceptoCatalogo, DetalleConceptoRecibo, EstadoRecibo, MetodoPago, ReciboCaja } from '../types/caja'

const { dark } = useTheme()

// ==========================================
// 1. Datos del Cajero y Turno Operativo (En blanco para BD)
// ==========================================
const cajeroActual = ref('')
const cajaNumero = ref('')
const turnoActual = ref('')
const fechaOperacion = ref(new Date().toISOString().split('T')[0])
const permitirEditarFecha = ref(false)

// ==========================================
// 2. Metadatos del Recibo en Pantalla
// ==========================================
const numeroRecibo = ref<number | string>('')
const folioAdministrativo = ref('')
const estadoRecibo = ref<EstadoRecibo>('REGISTRADO')
const metodoPagoSeleccionado = ref<MetodoPago>('EFECTIVO')
const referenciaBancaria = ref('')
const fechaBancaria = ref('')
const cuentaBancaria = ref('')

// ==========================================
// 3. Expediente del Estudiante (Limpio para búsqueda)
// ==========================================
const busquedaMatricula = ref('')
const alumno = ref<AlumnoCaja>({
  matricula: '',
  nombreCompleto: '',
  carrera: '',
  carreraId: 0,
  semestre: 0,
  turno: 'Matutino',
  estatus: 'Inscrito', // Estatus por defecto
  tieneBeca: false,
  porcentajeBeca: 0,
})

// Catálogo exacto de carreras del TecNM Campus Monclova
const opcionesCarreras = [
  { value: '', label: 'Seleccionar carrera...' },
  { value: 'Ing. Industrial', label: 'Ing. Industrial' },
  { value: 'Ing. Mecanica', label: 'Ing. Mecanica' },
  { value: 'Ing. Electronica', label: 'Ing. Electronica' },
  { value: 'Ing. Gestion Empresarial', label: 'Ing. Gestion Empresarial' },
  { value: 'Ing. Informatica', label: 'Ing. Informatica' },
  { value: 'Ing. Energias Renovables', label: 'Ing. Energias Renovables' },
  { value: 'Ing. Mecatronica', label: 'Ing. Mecatronica' },
]

// Catálogo para autocompletado en búsqueda con texto tenue (simulación hasta conectar PostgreSQL)
const catalogoEstudiantesMuestra = [
  { matricula: '23050243', nombreCompleto: 'ORTEGON HERNANDEZ KLEBER', carrera: 'Ing. Industrial', semestre: 4 },
  { matricula: '23050118', nombreCompleto: 'GARCIA ZAVALA LUIS FERNANDO', carrera: 'Ing. Mecatronica', semestre: 3 },
  { matricula: '22050890', nombreCompleto: 'LOPEZ SANCHEZ MARIANA', carrera: 'Ing. Gestion Empresarial', semestre: 6 },
  { matricula: '24050012', nombreCompleto: 'RODRIGUEZ MORALES CARLOS', carrera: 'Ing. Informatica', semestre: 1 },
  { matricula: '21050334', nombreCompleto: 'HERNANDEZ RAMIREZ SOFIA', carrera: 'Ing. Energias Renovables', semestre: 8 },
]

const sugerenciasEstudiantes = computed(() => {
  const q = busquedaMatricula.value.trim().toLowerCase()
  if (!q || q.length < 1) return []
  return catalogoEstudiantesMuestra.filter(
    est => est.matricula.toLowerCase().includes(q) || est.nombreCompleto.toLowerCase().includes(q)
  )
})

function seleccionarEstudiante(est: typeof catalogoEstudiantesMuestra[0]): void {
  alumno.value.matricula = est.matricula
  alumno.value.nombreCompleto = est.nombreCompleto
  alumno.value.carrera = est.carrera
  alumno.value.semestre = est.semestre
  alumno.value.estatus = 'Inscrito'
  busquedaMatricula.value = est.matricula
}

const opcionesTurnos = [
  { value: '', label: 'Seleccionar turno...' },
  { value: 'Matutino', label: 'Matutino' },
  { value: 'Vespertino', label: 'Vespertino' },
  { value: 'Nocturno', label: 'Nocturno' },
]

// ==========================================
// 4. Catálogo y Rejilla de Conceptos
// ==========================================
const catalogoConceptos: ConceptoCatalogo[] = [
  { id: 'c1', clave: 'KARD', descripcion: 'KARDEX', cuotaBase: 150.0, aplicaBeca: false, categoria: 'Constancias' },
  { id: 'c2', clave: 'INSC', descripcion: 'Pago de Inscripción (Periodo Vigente 2026)', cuotaBase: 2600.0, aplicaBeca: true, categoria: 'Inscripción' },
  { id: 'c3', clave: 'INSC-ANT', descripcion: 'Adeudo Inscripción (Año 2025)', cuotaBase: 2450.0, aplicaBeca: false, categoria: 'Inscripción' },
  { id: 'c4', clave: 'VERA', descripcion: 'Curso de Verano (Por Asignatura)', cuotaBase: 1200.0, aplicaBeca: false, categoria: 'Cursos' },
  { id: 'c5', clave: 'CONST', descripcion: 'Constancia de Estudios con Calificaciones', cuotaBase: 100.0, aplicaBeca: false, categoria: 'Constancias' },
  { id: 'c6', clave: 'TITU', descripcion: 'Trámite de Titulación Integral y Cédula', cuotaBase: 3800.0, aplicaBeca: false, categoria: 'Otros' },
]

const conceptoSeleccionadoId = ref('c1')

// Partidas del recibo (inicia vacío para nuevo cobro)
const partidas = ref<DetalleConceptoRecibo[]>([])

function agregarConcepto(): void {
  const itemCatalogo = catalogoConceptos.find(c => c.id === conceptoSeleccionadoId.value)
  if (!itemCatalogo) return

  const descuento = alumno.value.tieneBeca && itemCatalogo.aplicaBeca
    ? (itemCatalogo.cuotaBase * alumno.value.porcentajeBeca) / 100
    : 0

  const importe = itemCatalogo.cuotaBase - descuento

  partidas.value.push({
    id: `p-${Date.now()}`,
    conceptoId: itemCatalogo.id,
    descripcion: itemCatalogo.descripcion,
    cantidad: 1,
    cuotaUnitario: itemCatalogo.cuotaBase,
    descuentoBeca: descuento,
    importeTotal: importe,
  })
}

function eliminarPartida(id: string): void {
  partidas.value = partidas.value.filter(p => p.id !== id)
}

// ==========================================
// 5. Cálculos de Totales
// ==========================================
const subtotal = computed(() =>
  partidas.value.reduce((acc, curr) => acc + curr.cuotaUnitario * curr.cantidad, 0),
)

const totalDescuentos = computed(() =>
  partidas.value.reduce((acc, curr) => acc + curr.descuentoBeca * curr.cantidad, 0),
)

const granTotal = computed(() =>
  Math.max(0, subtotal.value - totalDescuentos.value),
)

// ==========================================
// 6. Historial de Recibos Recientes del Turno
// ==========================================
const historialTurno = ref<ReciboCaja[]>([])

function cargarReciboHistorial(rec: ReciboCaja): void {
  numeroRecibo.value = rec.numeroRecibo
  folioAdministrativo.value = rec.folioAdministrativo
  fechaOperacion.value = rec.fechaOperacion
  estadoRecibo.value = rec.estado
  alumno.value = { ...rec.alumno }
  busquedaMatricula.value = rec.alumno.matricula
  partidas.value = [...rec.detalles]
  metodoPagoSeleccionado.value = rec.metodoPago
  if (rec.referenciaBancaria) referenciaBancaria.value = rec.referenciaBancaria
}

// ==========================================
// 7. Modales: Cancelación e Impresión
// ==========================================
const mostrarModalCancelar = ref(false)
const motivoCancelacion = ref('')
const errorCancelacion = ref('')

function abrirModalCancelar(): void {
  motivoCancelacion.value = ''
  errorCancelacion.value = ''
  mostrarModalCancelar.value = true
}

function confirmarCancelacion(): void {
  if (!motivoCancelacion.value.trim() || motivoCancelacion.value.length < 5) {
    errorCancelacion.value = 'Debes ingresar un motivo administrativo válido (mínimo 5 caracteres).'
    return
  }

  estadoRecibo.value = 'CANCELADO'
  mostrarModalCancelar.value = false

  // Actualizar en historial
  const rec = historialTurno.value.find(r => r.numeroRecibo === numeroRecibo.value)
  if (rec) {
    rec.estado = 'CANCELADO'
    rec.motivoCancelacion = motivoCancelacion.value
    rec.fechaCancelacion = new Date().toISOString()
  }
}

const mostrarModalImprimir = ref(false)

function abrirImpresion(): void {
  mostrarModalImprimir.value = true
}

function imprimirDocumento(): void {
  window.print()
}

function limpiarFormulario(): void {
  const num = typeof numeroRecibo.value === 'number' ? numeroRecibo.value : Number(numeroRecibo.value) || 128450
  numeroRecibo.value = num + 1
  folioAdministrativo.value = `FOL-2026-09-${numeroRecibo.value}`
  estadoRecibo.value = 'PREVIO'
  busquedaMatricula.value = ''
  alumno.value = {
    matricula: '',
    nombreCompleto: '',
    carrera: 'Ing. Industrial',
    carreraId: 104,
    semestre: 1,
    turno: 'Matutino',
    estatus: 'Inscrito',
    tieneBeca: false,
    porcentajeBeca: 0,
  }
  partidas.value = []
}

function registrarCobro(): void {
  estadoRecibo.value = 'REGISTRADO'
  const nuevoRecibo: ReciboCaja = {
    id: `rec-${numeroRecibo.value}`,
    numeroRecibo: Number(numeroRecibo.value) || 0,
    folioAdministrativo: folioAdministrativo.value,
    fechaOperacion: fechaOperacion.value,
    alumno: { ...alumno.value },
    detalles: [...partidas.value],
    metodoPago: metodoPagoSeleccionado.value,
    referenciaBancaria: metodoPagoSeleccionado.value === 'DEPOSITO_BANCARIO' ? referenciaBancaria.value : undefined,
    subtotal: subtotal.value,
    descuentoTotal: totalDescuentos.value,
    total: granTotal.value,
    estado: 'REGISTRADO',
    cajeroNombre: cajeroActual.value,
    cajaNumero: cajaNumero.value,
  }

  historialTurno.value.unshift(nuevoRecibo)
  abrirImpresion()
}
</script>

<template>
  <div class="caja-dashboard" :class="{ 'dark-theme': dark }">
    <!-- BARRA SUPERIOR INSTITUCIONAL: Control de Caja TecNM -->
    <header class="cashier-status-bar">
      <div class="brand-zone">
        <span class="institution-pill">TecNM Monclova</span>
        <h1 class="system-title">Sistema de Control de Caja Universitaria</h1>
      </div>

      <div class="cashier-meta">
        <div class="meta-item">
          <span class="meta-label">Usuario</span>
          <strong class="meta-value">{{ cajeroActual || '—' }}</strong>
        </div>
        <div class="meta-item">
          <span class="meta-label">Fecha de Movimientos</span>
          <strong class="meta-value">{{ fechaOperacion }}</strong>
        </div>
        <div class="meta-item">
          <span class="meta-label">Estatus del Recibo</span>
          <UiBadge tone="success">
            {{ estadoRecibo }}
          </UiBadge>
        </div>
      </div>
    </header>

    <div class="caja-grid-layout">
      <!-- ÁREA PRINCIPAL DE TRABAJO -->
      <main class="caja-main-area">
        <!-- 1. Encabezado del Recibo y Búsqueda -->
        <UiCard class="receipt-header-card">
          <div class="header-card-row">
            <div class="receipt-number-group">
              <span class="receipt-number-label">Recibo No:</span>
              <span class="receipt-number-badge">{{ numeroRecibo || 'NUEVO' }}</span>
              <span class="receipt-folio-tag">{{ folioAdministrativo || 'FOL-PENDIENTE' }}</span>
            </div>

            <div class="receipt-date-group">
              <label for="fecha-caja">Fecha:</label>
              <input
                id="fecha-caja"
                v-model="fechaOperacion"
                type="date"
                class="field-input-inline"
                :disabled="!permitirEditarFecha"
              />
              <button
                type="button"
                class="btn-text-action"
                @click="permitirEditarFecha = !permitirEditarFecha"
              >
                {{ permitirEditarFecha ? 'Bloquear' : 'Modificar' }}
              </button>
            </div>
          </div>

          <!-- Búsqueda rápida de alumno -->
          <div class="search-student-row">
            <div class="search-input-wrap">
              <UiField
                v-model="busquedaMatricula"
                label="Buscar por Matrícula o Nombre"
                placeholder="Escribe para buscar (ej. 23050...)"
                hint="Al escribir se muestran sugerencias en base a matrícula y nombre"
              />
              <!-- Sugerencias de autocompletado en texto tenue / bajito -->
              <div v-if="sugerenciasEstudiantes.length > 0" class="suggestions-dropdown">
                <div
                  v-for="sug in sugerenciasEstudiantes"
                  :key="sug.matricula"
                  class="suggestion-item"
                  @click="seleccionarEstudiante(sug)"
                >
                  <span class="sug-matricula">{{ sug.matricula }}</span>
                  <span class="sug-nombre">{{ sug.nombreCompleto }}</span>
                  <span class="sug-detalle">{{ sug.carrera }} · Semestre {{ sug.semestre }}°</span>
                </div>
              </div>
            </div>
            <UiButton variant="primary" class="btn-search">
              Localizar Alumno
            </UiButton>
          </div>
        </UiCard>

        <!-- 2. Datos del Estudiante -->
        <UiCard title="Datos del Estudiante" description="Información académica y administrativa asociada a los cobros">
          <div class="student-form-grid">
            <div class="form-col col-matricula">
              <UiField
                v-model="alumno.matricula"
                label="Matrícula"
                placeholder="Ej. 23050001"
              />
            </div>

            <div class="form-col col-nombre">
              <UiField
                v-model="alumno.nombreCompleto"
                label="Nombre Completo"
                placeholder="Nombre y apellidos del estudiante..."
              />
            </div>

            <div class="form-col col-semestre">
              <UiField
                :model-value="alumno.semestre > 0 ? `Semestre ${alumno.semestre}°` : '—'"
                label="Semestre"
                disabled
              />
            </div>

            <div class="form-col col-carrera">
              <UiField
                v-model="alumno.carrera"
                as="select"
                label="Carrera"
                :options="opcionesCarreras"
              />
            </div>

            <div class="form-col col-turno">
              <UiField
                v-model="alumno.turno"
                as="select"
                label="Turno"
                :options="opcionesTurnos"
              />
            </div>

            <div class="form-col col-estatus">
              <label class="field-static-label">Estatus Escolar</label>
              <div class="estatus-badge-container">
                <UiBadge tone="success">{{ alumno.estatus }}</UiBadge>
                <UiBadge v-if="alumno.tieneBeca" tone="purple">
                  Beca {{ alumno.porcentajeBeca }}%
                </UiBadge>
              </div>
            </div>
          </div>
        </UiCard>

        <!-- 3. Conceptos de Cobro (Tabla central de la foto) -->
        <UiCard title="Conceptos a Cobrar" description="Detalle de cuotas, becas aplicadas e importes netos">
          <!-- Selector para añadir partidas -->
          <div class="add-concept-bar">
            <div class="select-concept-wrap">
              <label for="concept-picker">Seleccionar Concepto:</label>
              <select id="concept-picker" v-model="conceptoSeleccionadoId" class="concept-dropdown">
                <option v-for="c in catalogoConceptos" :key="c.id" :value="c.id">
                  {{ c.clave }} - {{ c.descripcion }} (${{ c.cuotaBase.toFixed(2) }})
                </option>
              </select>
            </div>
            <UiButton variant="accent" @click="agregarConcepto">
              + Agregar Concepto
            </UiButton>
          </div>

          <!-- Rejilla con columnas exactas de la foto -->
          <div class="receipt-grid-wrap">
            <table class="receipt-table">
              <thead>
                <tr>
                  <th class="th-concepto">Concepto</th>
                  <th class="th-num">Cantidad</th>
                  <th class="th-num">Cuota</th>
                  <th class="th-num">Adeudo / Beca</th>
                  <th class="th-num">Importe</th>
                  <th class="th-action">Acción</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="partidas.length === 0">
                  <td colspan="6" class="empty-items-cell">
                    No hay conceptos añadidos a este recibo.
                  </td>
                </tr>
                <tr v-for="item in partidas" :key="item.id" class="item-row">
                  <td class="td-concepto font-bold">
                    {{ item.descripcion }}
                  </td>
                  <td class="td-num">
                    <input
                      v-model.number="item.cantidad"
                      type="number"
                      min="1"
                      class="qty-input"
                      @input="item.importeTotal = (item.cuotaUnitario - item.descuentoBeca) * item.cantidad"
                    />
                  </td>
                  <td class="td-num">
                    ${{ item.cuotaUnitario.toFixed(4) }}
                  </td>
                  <td class="td-num text-danger">
                    {{ item.descuentoBeca > 0 ? `-$${(item.descuentoBeca * item.cantidad).toFixed(4)}` : '$0.0000' }}
                  </td>
                  <td class="td-num font-bold">
                    ${{ item.importeTotal.toFixed(4) }}
                  </td>
                  <td class="td-action">
                    <button
                      type="button"
                      class="btn-delete-row"
                      title="Eliminar partida"
                      @click="eliminarPartida(item.id)"
                    >
                      ×
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Total de Liquidación -->
          <div class="totals-section">
            <div class="amount-total-container">
              <span class="total-label">Total:</span>
              <div class="total-display-card">
                <span class="total-currency">$</span>
                <span class="total-digits">{{ granTotal.toFixed(2) }}</span>
              </div>
            </div>
          </div>
        </UiCard>

        <!-- 4. Modalidad de Pago y Referencia Bancaria (Regla 5+5) -->
        <UiCard title="Forma de Liquidación" description="Modalidad de cobro y datos bancarios">
          <div class="payment-method-selector">
            <label
              class="payment-pill-option"
              :class="{ selected: metodoPagoSeleccionado === 'EFECTIVO' }"
            >
              <input v-model="metodoPagoSeleccionado" type="radio" value="EFECTIVO" />
              <svg class="payment-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="2" y="6" width="20" height="12" rx="2" />
                <circle cx="12" cy="12" r="2" />
                <path d="M6 12h.01M18 12h.01" />
              </svg>
              <span>Efectivo en Ventanilla</span>
            </label>

            <label
              class="payment-pill-option"
              :class="{ selected: metodoPagoSeleccionado === 'DEPOSITO_BANCARIO' }"
            >
              <input v-model="metodoPagoSeleccionado" type="radio" value="DEPOSITO_BANCARIO" />
              <svg class="payment-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M3 21h18M3 10h18M5 10v11M19 10v11M9 10v11M14 10v11M12 2L2 7h20L12 2z" />
              </svg>
              <span>Depósito Bancario</span>
            </label>
          </div>

          <!-- Desglose bancario si se selecciona Depósito -->
          <div v-if="metodoPagoSeleccionado === 'DEPOSITO_BANCARIO'" class="bank-details-box">
            <div class="bank-grid">
              <UiField
                v-model="cuentaBancaria"
                label="Cuenta / Convenio"
                hint="Cuenta receptora oficial TecNM"
              />
              <UiField
                v-model="referenciaBancaria"
                label="Referencia Bancaria (Estructura 5 + 5)"
                hint="5 dígitos de alumno + 5 dígitos de concepto"
              />
              <UiField
                v-model="fechaBancaria"
                type="date"
                label="Fecha de Pago en Banco"
              />
            </div>
            <p class="reference-rule-note">
              ℹ️ Validación de referencia: Primeros 5 dígitos (<strong>{{ referenciaBancaria.slice(0, 5) }}</strong>) vinculados a la matrícula; últimos 5 dígitos (<strong>{{ referenciaBancaria.slice(5) }}</strong>) correspondientes a la cuota institucional.
            </p>
          </div>
        </UiCard>

        <!-- 5. Barra de Acciones Principales (De la foto modernizadas) -->
        <div class="caja-actions-bar">
          <div class="action-buttons-left">
            <UiButton
              variant="primary"
              :disabled="partidas.length === 0 || estadoRecibo === 'CANCELADO'"
              @click="registrarCobro"
            >
              ✓ Registrar Cobro
            </UiButton>

            <UiButton
              variant="secondary"
              :disabled="partidas.length === 0"
              @click="estadoRecibo = 'PREVIO'"
            >
              📄 Recibo Previo
            </UiButton>

            <UiButton
              variant="secondary"
              :disabled="partidas.length === 0"
              @click="abrirImpresion"
            >
              🖨️ Imprimir
            </UiButton>

            <UiButton
              variant="danger"
              :disabled="estadoRecibo === 'CANCELADO'"
              @click="abrirModalCancelar"
            >
              🚫 Cancelar Recibo
            </UiButton>
          </div>

          <div class="action-buttons-right">
            <UiButton variant="secondary" @click="limpiarFormulario">
              Limpiar
            </UiButton>
          </div>
        </div>
      </main>

      <!-- BARRA LATERAL: Recibos Recientes del Turno -->
      <aside class="caja-sidebar-panel">
        <UiCard title="Turno Actual" description="Recibos emitidos en la sesión">
          <div class="shift-summary-badges">
            <div class="summary-chip">
              <span class="chip-number">{{ historialTurno.length }}</span>
              <span class="chip-label">Emitidos</span>
            </div>
            <div class="summary-chip highlight">
              <span class="chip-number">${{ historialTurno.filter(r => r.estado === 'REGISTRADO').reduce((a, b) => a + b.total, 0).toFixed(0) }}</span>
              <span class="chip-label">Corte Caja</span>
            </div>
          </div>

          <div class="recent-receipts-list">
            <div v-if="historialTurno.length === 0" class="empty-recent-msg">
              No hay movimientos registrados en esta sesión.
            </div>
            <div
              v-for="rec in historialTurno"
              :key="rec.id"
              class="recent-receipt-card"
              :class="{ active: rec.numeroRecibo === numeroRecibo }"
              @click="cargarReciboHistorial(rec)"
            >
              <div class="recent-top">
                <strong class="rec-num">#{{ rec.numeroRecibo }}</strong>
                <UiBadge
                  :tone="rec.estado === 'REGISTRADO' ? 'success' : rec.estado === 'CANCELADO' ? 'danger' : 'warning'"
                >
                  {{ rec.estado }}
                </UiBadge>
              </div>
              <div class="rec-student">{{ rec.alumno.nombreCompleto }}</div>
              <div class="rec-bottom">
                <span class="rec-matricula">{{ rec.alumno.matricula }}</span>
                <span class="rec-total font-bold">${{ rec.total.toFixed(2) }}</span>
              </div>
            </div>
          </div>
        </UiCard>
      </aside>
    </div>

    <!-- DIÁLOGO: Cancelar Recibo (Justificación requerida) -->
    <UiDialog
      :open="mostrarModalCancelar"
      title="Cancelar Recibo Oficial"
      description="Por normativa universitaria, toda cancelación debe registrar un motivo formal y no eliminará el registro histórico."
      @close="mostrarModalCancelar = false"
    >
      <div class="cancel-modal-body">
        <UiAlert tone="danger" title="Atención">
          Estás por cancelar el recibo <strong>#{{ numeroRecibo }}</strong> por un monto de <strong>${{ granTotal.toFixed(2) }}</strong>.
        </UiAlert>

        <UiField
          v-model="motivoCancelacion"
          as="textarea"
          label="Motivo Administrativo de la Cancelación"
          placeholder="Escribe la justificación autorizada por tesorería..."
          :error="errorCancelacion"
        />

        <div class="modal-buttons-row">
          <UiButton variant="secondary" @click="mostrarModalCancelar = false">
            Regresar sin Cancelar
          </UiButton>
          <UiButton variant="danger" @click="confirmarCancelacion">
            Confirmar Cancelación
          </UiButton>
        </div>
      </div>
    </UiDialog>

    <!-- DIÁLOGO: Vista Previa de Recibo Oficial Imprimible -->
    <UiDialog
      :open="mostrarModalImprimir"
      title="Recibo Oficial de Ingresos - TecNM"
      description="Comprobante digital emitido conforme a los registros del sistema de caja"
      @close="mostrarModalImprimir = false"
    >
      <div class="receipt-printable-ticket">
        <div class="ticket-header">
          <div class="tecnm-badge-brand">TECNOLÓGICO NACIONAL DE MÉXICO</div>
          <div class="tecnm-campus-name">CAMPUS MONCLOVA</div>
          <div class="ticket-type">SISTEMA DE CONTROL DE CAJA · COMPROBANTE DE PAGO</div>
        </div>

        <div class="ticket-meta-grid">
          <div><span>Recibo No:</span> <strong>{{ numeroRecibo }}</strong></div>
          <div><span>Fecha:</span> <strong>{{ fechaOperacion }}</strong></div>
          <div><span>Folio:</span> <strong>{{ folioAdministrativo }}</strong></div>
          <div><span>Caja:</span> <strong>01 ({{ cajeroActual }})</strong></div>
        </div>

        <div class="ticket-student-info">
          <div><span>Estudiante:</span> <strong>{{ alumno.nombreCompleto }}</strong></div>
          <div><span>Matrícula:</span> <strong>{{ alumno.matricula }}</strong></div>
          <div><span>Carrera:</span> <strong>{{ alumno.carrera }}</strong></div>
        </div>

        <table class="ticket-table">
          <thead>
            <tr>
              <th>Concepto</th>
              <th class="txt-right">Cant</th>
              <th class="txt-right">Importe</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in partidas" :key="p.id">
              <td>{{ p.descripcion }}</td>
              <td class="txt-right">{{ p.cantidad }}</td>
              <td class="txt-right">${{ p.importeTotal.toFixed(2) }}</td>
            </tr>
          </tbody>
        </table>

        <div class="ticket-total-box">
          <div class="ticket-total-row">
            <span>TOTAL:</span>
            <strong>${{ granTotal.toFixed(2) }} M.N.</strong>
          </div>
        </div>

        <div class="ticket-signatures">
          <div class="sig-block">
            <div class="sig-line"></div>
            <span>Cajero Responsable</span>
          </div>
          <div class="sig-block">
            <div class="sig-line"></div>
            <span>Firma del Estudiante</span>
          </div>
        </div>
      </div>

      <div class="print-dialog-actions">
        <UiButton variant="secondary" @click="mostrarModalImprimir = false">
          Cerrar
        </UiButton>
        <UiButton variant="primary" @click="imprimirDocumento">
          🖨️ Mandar a Impresora
        </UiButton>
      </div>
    </UiDialog>
  </div>
</template>

<style scoped>
/* ==========================================================================
   Estilos Institucionales TecNM (Diseño Dual Light/Dark conforme a DESIGN.md)
   ========================================================================== */

.caja-dashboard {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 16px;
  background-color: var(--color-bg-main, #F4F6F9);
  min-height: 100vh;
}

/* 1. Barra de estado superior */
.cashier-status-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  background: var(--color-primary, #1B396A);
  color: #FFFFFF;
  padding: 14px 24px;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(27, 57, 106, 0.15);
}

.brand-zone {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.institution-pill {
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--color-accent, #C5A059);
}

.system-title {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 700;
  color: #FFFFFF;
}

.cashier-meta {
  display: flex;
  align-items: center;
  gap: 20px;
  flex-wrap: wrap;
}

.meta-item {
  display: flex;
  flex-direction: column;
  font-size: 0.85rem;
}

.meta-label {
  font-size: 0.7rem;
  text-transform: uppercase;
  opacity: 0.8;
  letter-spacing: 0.05em;
}

.meta-value {
  color: #FFFFFF;
  font-weight: 600;
}

/* 2. Disposición principal en rejilla */
.caja-grid-layout {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 20px;
}

@media (max-width: 1080px) {
  .caja-grid-layout {
    grid-template-columns: 1fr;
  }
}

.caja-main-area {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

/* 3. Encabezado de recibo */
.receipt-header-card {
  padding: 16px 20px;
}

.header-card-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  border-bottom: 1px solid var(--color-border, #E2E8F0);
  padding-bottom: 14px;
  margin-bottom: 14px;
}

.receipt-number-group {
  display: flex;
  align-items: center;
  gap: 12px;
}

.receipt-number-label {
  font-weight: 700;
  font-size: 1.1rem;
  color: var(--color-text-primary, #1E293B);
}

.receipt-number-badge {
  background-color: var(--color-primary, #1B396A);
  color: #FFFFFF;
  font-size: 1.35rem;
  font-weight: 800;
  padding: 4px 14px;
  border-radius: 8px;
  letter-spacing: 0.05em;
}

.receipt-folio-tag {
  font-size: 0.8rem;
  color: var(--color-text-secondary, #64748B);
  background: var(--color-gray-100, #F1F5F9);
  padding: 4px 8px;
  border-radius: 4px;
}

.receipt-date-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.field-input-inline {
  padding: 6px 10px;
  border: 1px solid var(--color-border-medium, #CBD5E1);
  border-radius: 6px;
  font-weight: 600;
  background: var(--color-surface-white, #FFFFFF);
  color: var(--color-text-primary, #1E293B);
}

.btn-text-action {
  background: none;
  border: none;
  color: var(--color-primary, #1B396A);
  text-decoration: underline;
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 600;
}

.search-student-row {
  display: flex;
  align-items: flex-end;
  gap: 12px;
}

.search-input-wrap {
  flex: 1;
  position: relative;
}

.suggestions-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  z-index: 25;
  background: var(--color-surface-white, #FFFFFF);
  border: 1px solid var(--color-border-medium, #CBD5E1);
  border-radius: 8px;
  box-shadow: 0 10px 15px -3px rgba(15, 37, 72, 0.12);
  margin-top: 4px;
  max-height: 220px;
  overflow-y: auto;
}

.suggestion-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  cursor: pointer;
  border-bottom: 1px solid var(--color-border-light, #F1F5F9);
  transition: background 0.15s ease;
}

.suggestion-item:last-child {
  border-bottom: none;
}

.suggestion-item:hover {
  background: var(--color-primary-subtle, #EBF3FF);
}

.sug-matricula {
  font-weight: 700;
  font-size: 0.88rem;
  color: var(--color-primary, #1B396A);
}

.sug-nombre {
  font-weight: 500;
  font-size: 0.85rem;
  color: var(--color-text-secondary, #64748B);
}

.sug-detalle {
  margin-left: auto;
  font-size: 0.78rem;
  color: var(--color-text-muted, #94A3B8);
  font-style: italic;
}

.btn-search {
  height: 44px;
  margin-bottom: 2px;
}

/* 4. Formulario del estudiante */
.student-form-grid {
  display: grid;
  grid-template-columns: 160px 1fr 140px;
  gap: 14px;
}

@media (max-width: 768px) {
  .student-form-grid {
    grid-template-columns: 1fr;
  }
}

.col-carrera {
  grid-column: span 2;
}

.field-static-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  margin-bottom: 6px;
  color: var(--color-text-secondary, #64748B);
}

.estatus-badge-container {
  display: flex;
  gap: 8px;
  align-items: center;
  height: 42px;
}

/* 5. Rejilla de conceptos */
.add-concept-bar {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 16px;
  margin-bottom: 16px;
  padding: 12px;
  background: var(--color-gray-50, #F8FAFC);
  border: 1px solid var(--color-border, #E2E8F0);
  border-radius: 8px;
}

.select-concept-wrap {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.select-concept-wrap label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-text-secondary, #64748B);
}

.concept-dropdown {
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid var(--color-border-medium, #CBD5E1);
  background: var(--color-surface-white, #FFFFFF);
  color: var(--color-text-primary, #1E293B);
  font-size: 0.95rem;
}

.receipt-grid-wrap {
  overflow-x: auto;
  border: 1px solid var(--color-border, #E2E8F0);
  border-radius: 8px;
}

.receipt-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.receipt-table th {
  background: var(--color-gray-100, #F1F5F9);
  padding: 10px 14px;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-secondary, #64748B);
  border-bottom: 2px solid var(--color-border, #E2E8F0);
}

.receipt-table td {
  padding: 10px 14px;
  border-bottom: 1px solid var(--color-border-light, #F1F5F9);
  font-size: 0.92rem;
}

.th-num, .td-num {
  text-align: right;
}

.th-action, .td-action {
  text-align: center;
  width: 60px;
}

.qty-input {
  width: 60px;
  text-align: center;
  padding: 4px;
  border: 1px solid var(--color-border-medium, #CBD5E1);
  border-radius: 4px;
}

.btn-delete-row {
  background: none;
  border: none;
  color: #DC2626;
  font-size: 1.3rem;
  font-weight: bold;
  cursor: pointer;
  padding: 0 6px;
  border-radius: 4px;
}

.btn-delete-row:hover {
  background: #FEE2E2;
}

.empty-items-cell {
  text-align: center;
  padding: 30px;
  color: var(--color-text-muted, #94A3B8);
  font-style: italic;
}

/* 6. Totales de Liquidación */
.totals-section {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 20px;
  margin-top: 18px;
}

.amount-total-container {
  display: flex;
  align-items: center;
  gap: 12px;
}

.total-label {
  font-size: 1.3rem;
  font-weight: 700;
  color: var(--color-text-primary, #1E293B);
}

.total-display-card {
  display: flex;
  align-items: baseline;
  background: #1B396A;
  color: #FFFFFF;
  padding: 8px 24px;
  border-radius: 10px;
  box-shadow: 0 4px 10px rgba(27, 57, 106, 0.2);
}

.total-currency {
  font-size: 1.4rem;
  font-weight: 600;
  margin-right: 4px;
  color: var(--color-accent, #C5A059);
}

.total-digits {
  font-size: 2rem;
  font-weight: 800;
  letter-spacing: 0.02em;
}

/* 7. Métodos de pago */
.payment-method-selector {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.payment-pill-option {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 18px;
  border: 2px solid var(--color-border-medium, #CBD5E1);
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s ease;
  background: var(--color-surface-white, #FFFFFF);
}

.payment-pill-option.selected {
  border-color: var(--color-primary, #1B396A);
  background: var(--color-primary-subtle, #EBF3FF);
  color: var(--color-primary, #1B396A);
}

.payment-icon {
  width: 20px;
  height: 20px;
  stroke: currentColor;
}

.bank-details-box {
  margin-top: 14px;
  padding: 14px;
  border-radius: 8px;
  background: var(--color-gray-50, #F8FAFC);
  border: 1px dashed var(--color-border-medium, #CBD5E1);
}

.bank-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 14px;
}

.reference-rule-note {
  margin: 10px 0 0 0;
  font-size: 0.8rem;
  color: var(--color-text-secondary, #64748B);
}

/* 8. Barra inferior de acciones */
.caja-actions-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
  padding: 14px 20px;
  background: var(--color-surface-white, #FFFFFF);
  border: 1px solid var(--color-border, #E2E8F0);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(15, 37, 72, 0.05);
}

.action-buttons-left {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

/* 9. Historial lateral de recibos */
.shift-summary-badges {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-bottom: 14px;
}

.summary-chip {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 10px;
  background: var(--color-gray-50, #F8FAFC);
  border: 1px solid var(--color-border, #E2E8F0);
  border-radius: 8px;
}

.summary-chip.highlight {
  background: var(--color-primary-subtle, #EBF3FF);
  border-color: var(--color-primary, #1B396A);
}

.chip-number {
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--color-primary, #1B396A);
}

.chip-label {
  font-size: 0.72rem;
  text-transform: uppercase;
  color: var(--color-text-secondary, #64748B);
}

.recent-receipts-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: 520px;
  overflow-y: auto;
}

.empty-recent-msg {
  text-align: center;
  padding: 24px 12px;
  color: var(--color-text-muted, #94A3B8);
  font-size: 0.85rem;
  font-style: italic;
  border: 1px dashed var(--color-border, #E2E8F0);
  border-radius: 8px;
}

.recent-receipt-card {
  padding: 10px 12px;
  border: 1px solid var(--color-border, #E2E8F0);
  border-radius: 8px;
  cursor: pointer;
  background: var(--color-surface-white, #FFFFFF);
  transition: border-color 0.15s ease;
}

.recent-receipt-card:hover {
  border-color: var(--color-primary, #1B396A);
}

.recent-receipt-card.active {
  border-color: var(--color-primary, #1B396A);
  background: var(--color-primary-subtle, #EBF3FF);
}

.recent-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.rec-student {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--color-text-primary, #1E293B);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.rec-bottom {
  display: flex;
  justify-content: space-between;
  font-size: 0.78rem;
  color: var(--color-text-secondary, #64748B);
  margin-top: 4px;
}

/* 10. Modales de Cancelación e Impresión */
.cancel-modal-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 10px 0;
}

.modal-buttons-row {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 10px;
}

/* Formato de Ticket Impreso */
.receipt-printable-ticket {
  border: 2px dashed #94A3B8;
  padding: 24px;
  border-radius: 8px;
  background: #FFFFFF;
  color: #000000;
  font-family: 'Courier New', Courier, monospace;
}

.ticket-header {
  text-align: center;
  border-bottom: 2px solid #000000;
  padding-bottom: 12px;
  margin-bottom: 12px;
}

.tecnm-badge-brand {
  font-weight: 800;
  font-size: 1rem;
}

.tecnm-campus-name {
  font-size: 0.9rem;
  font-weight: 700;
}

.ticket-type {
  font-size: 0.75rem;
  margin-top: 4px;
}

.ticket-meta-grid, .ticket-student-info {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px;
  font-size: 0.85rem;
  margin-bottom: 12px;
}

.ticket-table {
  width: 100%;
  border-top: 1px solid #000000;
  border-bottom: 1px solid #000000;
  margin: 12px 0;
  border-collapse: collapse;
}

.ticket-table th, .ticket-table td {
  padding: 6px;
  font-size: 0.85rem;
}

.ticket-total-box {
  margin: 14px 0;
  text-align: right;
}

.ticket-total-row {
  font-size: 1.2rem;
  font-weight: 900;
}

.ticket-words {
  font-size: 0.75rem;
  font-style: italic;
  margin-top: 4px;
}

.ticket-signatures {
  display: flex;
  justify-content: space-around;
  margin-top: 36px;
  padding-top: 10px;
}

.sig-block {
  text-align: center;
  font-size: 0.75rem;
}

.sig-line {
  width: 160px;
  border-top: 1px solid #000000;
  margin-bottom: 4px;
}

.print-dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 18px;
}

.txt-right {
  text-align: right;
}

.font-bold {
  font-weight: 700;
}

.text-danger {
  color: #DC2626;
}
</style>

