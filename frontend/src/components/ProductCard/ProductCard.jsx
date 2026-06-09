import React from 'react';
import { Package, AlertTriangle, CheckCircle, Tag } from 'lucide-react';
import './ProductCard.css';

const ProductCard = ({ product, onEdit, onDelete }) => {
  const getStatusColor = (status) => {
    switch (status) {
      case 'OK': return 'status-ok';
      case 'ESTOQUE_BAIXO': return 'status-warning';
      case 'SEM_ESTOQUE': return 'status-critical';
      default: return 'status-ok';
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case 'OK': return <CheckCircle size={16} />;
      case 'ESTOQUE_BAIXO': return <AlertTriangle size={16} />;
      case 'SEM_ESTOQUE': return <AlertTriangle size={16} />;
      default: return <CheckCircle size={16} />;
    }
  };

  return (
    <div className={`product-card glass-card ${getStatusColor(product.stockStatus)}`}>
      <div className="product-card-header">
        <h3 className="product-name">{product.name}</h3>
        <span className="product-category">
          <Tag size={12} /> {product.category?.name || 'Sem Categoria'}
        </span>
      </div>
      
      <div className="product-card-body">
        <div className="stock-info">
          <span className="info-label">Quantidade</span>
          <span className="info-value">{product.quantity} un.</span>
        </div>
        <div className="stock-info">
          <span className="info-label">Preço</span>
          <span className="info-value">{product.price?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</span>
        </div>
      </div>
      
      <div className="product-card-footer">
        <div className="status-badge">
          {getStatusIcon(product.stockStatus)}
          <span>{product.stockStatus?.replace('_', ' ')}</span>
        </div>
        <div className="card-actions">
          {onEdit && <button className="btn-secondary btn-sm" onClick={() => onEdit(product.id)}>Editar</button>}
          {onDelete && <button className="btn-danger btn-sm" onClick={() => onDelete(product.id)}>Excluir</button>}
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
