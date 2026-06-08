import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import AppRoutes from './routes';
import './styles/index.css';

// ==========================================
// COMPONENTE PRINCIPAL (App)
// Configura o sistema de navegação (URLs) do site antes de renderizar qualquer página
// ==========================================
function App() {
  return (
    // BrowserRouter: Diz para o React interpretar a barra de endereços (ex: meusaas.com/login)
    <BrowserRouter>
      {/* AppRoutes é quem contém a lista de TODAS as páginas possíveis */}
      <AppRoutes />
    </BrowserRouter>
  );
}

export default App;
