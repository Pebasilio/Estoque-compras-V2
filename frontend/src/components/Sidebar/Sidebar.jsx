import React from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Package, ArchiveRestore, ShoppingCart } from 'lucide-react';
import './Sidebar.css';

// ==========================================
// COMPONENTE SIDEBAR (Barra Lateral)
// Aparece em todas as telas protegidas para garantir a navegação rápida.
// ==========================================
const Sidebar = () => {
  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <div className="logo-icon">
          <Package size={24} color="#0EA5E9" />
        </div>
        <h2>StockPro</h2>
      </div>
      
      <nav className="sidebar-nav">
        {/* NavLink é especial do React Router: ele sabe automaticamente quando 
            estamos na página dele e ativa a classe 'active' para ficar aceso */}
        <NavLink to="/dashboard" className={({isActive}) => isActive ? "nav-item active" : "nav-item"}>
          <LayoutDashboard size={20} />
          <span>Dashboard</span>
        </NavLink>
        
        <NavLink to="/produtos" className={({isActive}) => isActive ? "nav-item active" : "nav-item"}>
          <Package size={20} />
          <span>Produtos</span>
        </NavLink>
        
        <NavLink to="/estoque" className={({isActive}) => isActive ? "nav-item active" : "nav-item"}>
          <ArchiveRestore size={20} />
          <span>Estoque</span>
        </NavLink>
        
        <NavLink to="/compras" className={({isActive}) => isActive ? "nav-item active" : "nav-item"}>
          <ShoppingCart size={20} />
          <span>Compras</span>
        </NavLink>
      </nav>
      
      <div className="sidebar-footer">
        <p className="version">v1.0.0</p>
      </div>
    </aside>
  );
};

export default Sidebar;
