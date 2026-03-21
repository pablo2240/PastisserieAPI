import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      'lodash': 'lodash-es',
    },
  },
  server: {
    port: 5173, // Cambiado para evitar conflicto con el backend (5174)
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:5175', // Usamos 127.0.0.1 para evitar problemas de resolución en Windows
        changeOrigin: true,
        secure: false,
      }
    }
  }
})