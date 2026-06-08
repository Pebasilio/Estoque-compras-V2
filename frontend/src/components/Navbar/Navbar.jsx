import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Bell, LogOut, Search, User } from 'lucide-react';
import { authService } from '../../services/authService';
import { produtoService } from '../../services/produtoService';
import './Navbar.css';

// ==========================================
// COMPONENTE NAVBAR (Barra Superior)
// Mostra em que tela estamos e quem está logado
// ==========================================
const Navbar = () => {
  const navigate = useNavigate(); // Hook para mudar de página via código
  const location = useLocation(); // Hook para saber em qual URL estamos agora
  const [user, setUser] = useState({ name: 'Usuário', email: '' });
  const [alertCount, setAlertCount] = useState(0);

  useEffect(() => {
    const currentUser = authService.getCurrentUser();
    if (currentUser) {
      setUser(currentUser);
    }

    // Busca os alertas dinamicamente ao carregar o Navbar
    const fetchAlerts = async () => {
      try {
        const alerts = await produtoService.getAlertasReposicao();
        setAlertCount(alerts.length);
      } catch (error) {
        console.error("Erro ao buscar alertas de reposição", error);
      }
    };
    
    fetchAlerts();
  }, []);

  // Função disparada no botão de "Sair"
  const handleLogout = () => {
    authService.logout(); // Limpa o cache
    navigate('/login'); // Joga o usuário pra fora
  };

  // Switch case disfarçado para mudar o título grandão no topo da tela dinamicamente
  const getPageTitle = () => {
    const path = location.pathname;
    if (path.includes('/dashboard')) return 'Visão Geral';
    if (path.includes('/produtos/novo')) return 'Cadastro de Produto';
    if (path.includes('/produtos/editar')) return 'Editar Produto';
    if (path.includes('/produtos')) return 'Produtos';
    if (path.includes('/estoque')) return 'Controle de Estoque';
    if (path.includes('/compras')) return 'Gestão de Compras';
    return '';
  };

  return (
    <header className="navbar">
      <div className="navbar-left">
        <h1 className="page-title">{getPageTitle()}</h1>
      </div>
      
      <div className="navbar-right">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Pesquisar..." className="search-input" />
        </div>
        
        <button className="icon-btn notification-btn" onClick={() => navigate('/compras')} title="Ver Alertas de Reposição">
          <Bell size={20} />
          {alertCount > 0 && <span className="badge">{alertCount}</span>}
        </button>
        
        <div className="user-profile">
          <div className="avatar">
            <User size={18} />
          </div>
          <div className="user-info">
            <span className="user-name">{user.name || user.email?.split('@')[0] || 'Admin'}</span>
          </div>
        </div>
        
        <button className="icon-btn logout-btn" onClick={handleLogout} title="Sair">
          <LogOut size={20} />
        </button>
      </div>
    </header>
  );
};

export default Navbar;
