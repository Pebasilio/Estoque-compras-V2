import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { authService } from '../../services/authService';
import Sidebar from '../Sidebar/Sidebar';
import Navbar from '../Navbar/Navbar';

const ProtectedRoute = () => {
  const isAuth = authService.isAuthenticated();

  // Se não estiver logado, redireciona para login
  if (!isAuth) {
    return <Navigate to="/login" replace />;
  }

  // Se estiver logado, renderiza o layout principal que envolve as páginas filhas
  return (
    <div className="app-container">
      <Sidebar />
      <div className="main-content">
        <Navbar />
        <div className="page-container">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default ProtectedRoute;
