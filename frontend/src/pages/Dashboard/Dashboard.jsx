import React, { useState, useEffect } from 'react';
import { Package, TrendingDown, DollarSign, Activity, ShoppingCart } from 'lucide-react';
import { produtoService } from '../../services/produtoService';
import AlertCard from '../../components/AlertCard/AlertCard';
import ProductTable from '../../components/ProductTable/ProductTable';
import { useNavigate } from 'react-router-dom';
import './Dashboard.css';

// ==========================================
// TELA DE PAINEL DE CONTROLE (Visão Geral)
// Aqui concentramos todas as informações numéricas essenciais do negócio
// ==========================================
const Dashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState({
    totalProducts: 0,
    lowStockCount: 0,
    totalValue: 0,
    recentMovements: 0
  });
  const [alerts, setAlerts] = useState([]);
  const [recentProducts, setRecentProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  // O useEffect roda magicamente 1 ÚNICA VEZ assim que a tela abre, graças ao array vazio [] no final
  useEffect(() => {
    loadDashboardData();
  }, []);

  // Função assíncrona para buscar os dados de dois lugares da API de uma só vez
  const loadDashboardData = async () => {
    setLoading(true);
    try {
      // Requisição 1: Puxa a lista de produtos
      const produtos = await produtoService.getProdutos();
      // Requisição 2: Puxa os alertas inteligentes de itens que acabaram
      const lowStockAlerts = await produtoService.getAlertasReposicao();
      
      // Requisição 3: Puxa o histórico completo de movimentações da rota GET /api/stock/movements
      const movements = await produtoService.getMovements();
      // Extrai apenas a parte da data ("2026-06-08") do ISO string para comparar com as movimentações
      const today = new Date().toISOString().split('T')[0];
      // Filtra apenas as movimentações cuja data começa com a data de hoje e conta quantas são
      // Isso substitui o antigo placeholder fixo de "12" por um valor real e dinâmico
      const todayCount = movements.filter(m => m.date.startsWith(today)).length;
      
      // Cálculo JS: Descobre quanto dinheiro tem parado no estoque
      const valTotal = produtos.reduce((acc, p) => acc + (p.price * p.quantity), 0);
      
      // Atualiza os blocos superiores visuais
      setStats({
        totalProducts: produtos.length,
        lowStockCount: lowStockAlerts.length,
        totalValue: valTotal,
        recentMovements: todayCount
      });
      
      setAlerts(lowStockAlerts.slice(0, 3)); // Pega os 3 piores alertas (Fila de Prioridade)
      setRecentProducts(produtos.slice(0, 5)); // Pega apenas 5 produtos para a tabelinha rápida
    } catch (error) {
      console.error('Erro ao carregar dashboard', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSimulateRestock = (alert) => {
    navigate('/compras');
  };

  if (loading) {
    return <div className="loading-state">Carregando painel de controle...</div>;
  }

  return (
    <div className="dashboard-wrapper">
      
      {/* KPI Cards */}
      <div className="kpi-grid">
        <div className="kpi-card glass-card">
          <div className="kpi-icon i-primary"><Package size={24} /></div>
          <div className="kpi-info">
            <span className="kpi-label">Total de Produtos</span>
            <span className="kpi-value">{stats.totalProducts}</span>
          </div>
        </div>
        
        <div className="kpi-card glass-card">
          <div className="kpi-icon i-danger"><TrendingDown size={24} /></div>
          <div className="kpi-info">
            <span className="kpi-label">Alertas de Estoque</span>
            <span className="kpi-value">{stats.lowStockCount}</span>
          </div>
        </div>

        <div className="kpi-card glass-card">
          <div className="kpi-icon i-success"><DollarSign size={24} /></div>
          <div className="kpi-info">
            <span className="kpi-label">Valor em Estoque</span>
            <span className="kpi-value">{stats.totalValue.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</span>
          </div>
        </div>

        <div className="kpi-card glass-card">
          <div className="kpi-icon i-warning"><Activity size={24} /></div>
          <div className="kpi-info">
            <span className="kpi-label">Movimentações Hoje</span>
            <span className="kpi-value">{stats.recentMovements}</span>
          </div>
        </div>
      </div>

      <div className="dashboard-columns">
        {/* Main Column */}
        <div className="main-col">
          <div className="section-header">
            <h3>Visão Geral Rápida</h3>
            <button className="btn-secondary btn-sm" onClick={() => navigate('/produtos')}>Ver Todos</button>
          </div>
          <ProductTable 
            products={recentProducts} 
          />
        </div>

        {/* Side Column for Alerts */}
        <div className="side-col">
          <div className="section-header">
            <h3>Atenção Necessária</h3>
            {alerts.length > 0 && (
              <button className="icon-btn-small primary-hover" title="Ir para Compras" onClick={() => navigate('/compras')}>
                <ShoppingCart size={18} />
              </button>
            )}
          </div>
          
          <div className="alerts-container">
            {alerts.length === 0 ? (
              <div className="glass-card text-center p-4">
                <span className="text-muted">Nenhum alerta crítico no momento. Estoque saudável!</span>
              </div>
            ) : (
              alerts.map((alert, idx) => (
                <AlertCard 
                  key={idx} 
                  alert={alert} 
                  onRestockClick={handleSimulateRestock} 
                />
              ))
            )}
          </div>
        </div>
      </div>
      
    </div>
  );
};

export default Dashboard;
