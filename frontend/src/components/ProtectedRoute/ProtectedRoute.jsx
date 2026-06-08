import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { authService } from '../../services/authService';
import Sidebar from '../Sidebar/Sidebar';
import Navbar from '../Navbar/Navbar';

// ==========================================
// COMPONENTE DE ROTA PROTEGIDA (Guardião)
// Verifica se o usuário tem permissão para ver as páginas internas.
// Também monta a "Moldura" visual (Barra Lateral + Barra Superior)
// ==========================================
const ProtectedRoute = () => {
  // Checa se o token existe no cache (localStorage)
  const isAuth = authService.isAuthenticated();

  // Se não estiver logado, redireciona violentamente para a tela de login
  if (!isAuth) {
    return <Navigate to="/login" replace />;
  }

  // Se estiver logado, renderiza o layout principal que envolve as páginas filhas
  // A mágica acontece no <Outlet /> -> É ali no meio que as telas (Dashboard, Produtos) vão ser "injetadas"
  return (
    <div className="app-container">
      {/* Barra Lateral Esquerda Fixa */}
      <Sidebar />
      <div className="main-content">
        {/* Barra Superior Fixa */}
        <Navbar />
        {/* O Miolo da página (O conteúdo dinâmico) */}
        <div className="page-container">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default ProtectedRoute;
