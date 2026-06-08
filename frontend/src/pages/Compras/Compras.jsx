import React, { useState, useEffect } from 'react';
import { ShoppingCart, CheckCircle } from 'lucide-react';
import { produtoService } from '../../services/produtoService';
import './Compras.css';

// ==========================================
// TELA DE COMPRAS / REPOSIÇÃO
// A tela inteligente que sugere o que comprar baseado nos cálculos lá do C#
// ==========================================
const Compras = () => {
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [buyingStatus, setBuyingStatus] = useState({}); // tracking compra por item

  useEffect(() => {
    loadAlerts();
  }, []);

  const loadAlerts = async () => {
    setLoading(true);
    try {
      const data = await produtoService.getAlertasReposicao();
      setAlerts(data);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // Função que simula que a loja ligou para o fornecedor e a roupa chegou instantaneamente
  const handleSimulatePurchase = async (productId, suggestedQty) => {
    // Altera o botão específico desse produto para "Carregando" (pra ficar mais Premium)
    setBuyingStatus(prev => ({ ...prev, [productId]: 'loading' }));
    
    try {
      // Usamos a API de Entrada (a mesma da tela Estoque) para dar a entrada "invisível" desses itens
      await produtoService.registraEntrada(productId, suggestedQty, 'Compra Automática Reposição', 'Sistema');
      
      // Muda o botão para "Sucesso!" verdinho
      setBuyingStatus(prev => ({ ...prev, [productId]: 'success' }));
      
      // Mágica de UI: Dá 2 segundos para o gestor ler o "Comprado" antes de remover o card da tela sozinho
      setTimeout(() => {
        setAlerts(prev => prev.filter(a => a.product.id !== productId));
      }, 2000);
      
    } catch (error) {
      // Se der pau, volta o botão normal
      setBuyingStatus(prev => ({ ...prev, [productId]: 'error' }));
      alert('Falha ao simular compra.');
    }
  };

  if (loading) return <div className="loading-state">Calculando necessidades de reposição...</div>;

  return (
    <div className="page-wrapper">
      <div className="compras-header glass-card">
        <div>
          <h2>Gestão de Compras (Reposição)</h2>
          <p className="text-muted">Itens que precisam de reposição com base no cálculo `(Limite * 3) - Estoque`</p>
        </div>
      </div>

      <div className="compras-list">
        {alerts.length === 0 ? (
          <div className="empty-state glass-card">
            <ShoppingCart size={48} color="var(--success-color)" style={{ marginBottom: '16px', opacity: 0.5 }} />
            <h3>Nenhuma compra necessária</h3>
            <p>Seu estoque está saudável. Todos os itens estão acima do limite mínimo.</p>
          </div>
        ) : (
          alerts.map(alert => (
            <div key={alert.product.id} className="compra-card glass-card">
              <div className="compra-info">
                <h3 className="compra-product-name">{alert.product.name}</h3>
                <div className="compra-details">
                  <span className="text-muted">Estoque Atual: </span>
                  <strong className={alert.product.quantity === 0 ? 'text-critical' : 'text-warning'}>
                    {alert.product.quantity} un.
                  </strong>
                  <span className="divider">|</span>
                  <span className="text-muted">Mínimo Ideal: </span>
                  <strong>{alert.product.reorderThreshold} un.</strong>
                </div>
              </div>
              
              <div className="compra-action-area">
                <div className="sugestao-box">
                  <span className="text-muted text-sm uppercase">Sugerido Comprar</span>
                  <span className="sugestao-valor">{alert.suggestedQuantity} un.</span>
                </div>
                
                {buyingStatus[alert.product.id] === 'success' ? (
                  <button className="btn-success btn-purchased" disabled>
                    <CheckCircle size={20} /> Comprado
                  </button>
                ) : (
                  <button 
                    className="btn-primary" 
                    onClick={() => handleSimulatePurchase(alert.product.id, alert.suggestedQuantity)}
                    disabled={buyingStatus[alert.product.id] === 'loading'}
                  >
                    <ShoppingCart size={18} /> 
                    {buyingStatus[alert.product.id] === 'loading' ? 'Processando...' : 'Comprar Agora'}
                  </button>
                )}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default Compras;
