# ==============================================================================
# Dockerfile para Frontend SPA (Vue 3 + Vite)
# Sistema de Control de Caja Universitaria - TecNM Campus Monclova
# ==============================================================================
FROM node:20-alpine

WORKDIR /app

# Copiar manifiesto de dependencias y restaurar
COPY package*.json ./
RUN npm install

# Copiar el código fuente
COPY . .

# Exponer el puerto estándar de Vite
EXPOSE 5173

# Arrancar el servidor de desarrollo en 0.0.0.0 para acceso desde el host
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

