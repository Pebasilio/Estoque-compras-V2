import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';

// Protected Route Wrapper
import ProtectedRoute from '../components/ProtectedRoute/ProtectedRoute';

// Pages
import Login from '../pages/Login/Login';
import Dashboard from '../pages/Dashboard/Dashboard';
import Produtos from '../pages/Produtos/Produtos';
import CadastroProduto from '../pages/CadastroProduto/CadastroProduto';
import EditarProduto from '../pages/EditarProduto/EditarProduto';
import Estoque from '../pages/Estoque/Estoque';
import Compras from '../pages/Compras/Compras';

// ==========================================
// GERENCIADOR DE ROTAS
// ==========================================
const AppRoutes = () => {
  return (
    <Routes>
      {/* Rota Pública: Qualquer um pode acessar (A tela de Login) */}
      <Route path="/login" element={<Login />} />
      
      {/* Rotas Protegidas: O "ProtectedRoute" é um guardião. 
          Se o cara tentar acessar /produtos sem login, é expulso de volta para o /login */}
      
      <Route element={<ProtectedRoute />}>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<Dashboard />} />
        
        <Route path="/produtos" element={<Produtos />} />
        <Route path="/produtos/novo" element={<CadastroProduto />} />
        <Route path="/produtos/editar/:id" element={<EditarProduto />} />
        
        <Route path="/estoque" element={<Estoque />} />
        <Route path="/compras" element={<Compras />} />
      </Route>
      
      {/* Catch all: Se digitar uma URL maluca (ex: /batata), joga direto pro dashboard de forma segura */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};

export default AppRoutes;
