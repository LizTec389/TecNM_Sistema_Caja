import type { RouteRecordRaw } from 'vue-router'

export const cajaRoutes: RouteRecordRaw[] = [
  {
    path: 'caja',
    name: 'caja-principal',
    component: () => import('./views/CajaPrincipalView.vue'),
    meta: { requiresAuth: true },
  },
]

