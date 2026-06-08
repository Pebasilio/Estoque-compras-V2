import React from 'react';
import { AlertOctagon, TrendingUp } from 'lucide-react';
import './AlertCard.css';

// ==========================================
// COMPONENTE ALERTCARD (Cartão de Alerta de Estoque)
// Exibe um aviso visual quando um produto está com estoque baixo ou zerado.
// Recebe o alerta (produto + sugestão de compra) e um callback para simular reposição.
// ==========================================
const AlertCard = ({ alert, onRestockClick }) => {
  // Desestrutura o alerta para extrair o produto e a quantidade sugerida de compra
  const { product, suggestedQuantity } = alert;
  // Define se o alerta é crítico (quantidade = 0) para aplicar estilo vermelho
  const isCritical = product.quantity === 0;

  return (
    // Aplica a classe 'critical-alert' (vermelho) ou 'warning-alert' (amarelo) dinamicamente
    <div className={`alert-card glass-card ${isCritical ? 'critical-alert' : 'warning-alert'}`}>
      {/* Ícone de alerta dentro de um container circular colorido */}
      <div className="alert-icon-container">
        <AlertOctagon size={28} />
      </div>
      
      {/* Conteúdo textual do alerta: título + descrição com dados do produto */}
      <div className="alert-content">
        <h4 className="alert-title">
          {isCritical ? 'ESTOQUE ZERADO' : '⚠ Estoque Baixo'}
        </h4>
        <p className="alert-desc">
          <strong>{product.name}</strong> está com {product.quantity} unidades (limite: {product.reorderThreshold}).
        </p>
      </div>

      {/* Área de ação: mostra a sugestão de compra e o botão de reposição */}
      <div className="alert-action">
        <div className="suggestion">
          <span className="suggestion-label">Sugestão de Compra</span>
          <span className="suggestion-value">
            <TrendingUp size={16} /> {suggestedQuantity} un.
          </span>
        </div>
        {/* O botão de reposição só aparece se a prop onRestockClick foi passada */}
        {onRestockClick && (
          <button className="btn-primary btn-sm mt-2 w-100" onClick={() => onRestockClick(alert)}>
            Simular Reposição
          </button>
        )}
      </div>
    </div>
  );
};

export default AlertCard;
