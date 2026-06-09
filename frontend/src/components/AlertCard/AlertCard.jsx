import React from 'react';
import { AlertOctagon, TrendingUp } from 'lucide-react';
import './AlertCard.css';

const AlertCard = ({ alert, onRestockClick }) => {
  const { product, suggestedQuantity } = alert;
  const isCritical = product.quantity === 0;

  return (
    <div className={`alert-card glass-card ${isCritical ? 'critical-alert' : 'warning-alert'}`}>
      <div className="alert-icon-container">
        <AlertOctagon size={28} />
      </div>
      
      <div className="alert-content">
        <h4 className="alert-title">
          {isCritical ? 'ESTOQUE ZERADO' : '⚠ Estoque Baixo'}
        </h4>
        <p className="alert-desc">
          <strong>{product.name}</strong> está com {product.quantity} unidades (limite: {product.reorderThreshold}).
        </p>
      </div>

      <div className="alert-action">
        <div className="suggestion">
          <span className="suggestion-label">Sugestão de Compra</span>
          <span className="suggestion-value">
            <TrendingUp size={16} /> {suggestedQuantity} un.
          </span>
        </div>
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
