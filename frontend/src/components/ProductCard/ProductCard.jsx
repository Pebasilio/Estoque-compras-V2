import React from 'react';
import { Package, AlertTriangle, CheckCircle, Tag } from 'lucide-react';
import './ProductCard.css';

// ==========================================
// COMPONENTE PRODUCTCARD (Card de Produto)
// Versão "card" visual de um produto, usada na visualização em grade (grid).
// Mostra nome, categoria, quantidade, preço e status do estoque.
// Props: product (dados do produto), onEdit e onDelete (callbacks de ação).
// ==========================================
const ProductCard = ({ product, onEdit, onDelete }) => {
  /**
   * getStatusColor: Retorna a classe CSS correspondente ao status do estoque.
   * Usado para aplicar bordas e cores condicionais no card.
   */
  const getStatusColor = (status) => {
    switch (status) {
      case 'OK': return 'status-ok';               // Verde: estoque saudável
      case 'ESTOQUE_BAIXO': return 'status-warning'; // Amarelo: precisa de atenção
      case 'SEM_ESTOQUE': return 'status-critical';  // Vermelho: urgente!
      default: return 'status-ok';
    }
  };

  /**
   * getStatusIcon: Retorna o ícone correspondente ao status do estoque.
   * CheckCircle (✓) para OK, AlertTriangle (⚠) para problemas.
   */
  const getStatusIcon = (status) => {
    switch (status) {
      case 'OK': return <CheckCircle size={16} />;
      case 'ESTOQUE_BAIXO': return <AlertTriangle size={16} />;
      case 'SEM_ESTOQUE': return <AlertTriangle size={16} />;
      default: return <CheckCircle size={16} />;
    }
  };

  return (
    // A classe de status é aplicada dinamicamente para colorir a borda lateral do card
    <div className={`product-card glass-card ${getStatusColor(product.stockStatus)}`}>
      {/* Cabeçalho: Nome do produto + tag de categoria */}
      <div className="product-card-header">
        <h3 className="product-name">{product.name}</h3>
        <span className="product-category">
          <Tag size={12} /> {product.category?.name || 'Sem Categoria'}
        </span>
      </div>
      
      {/* Corpo: Informações numéricas (quantidade em estoque e preço unitário) */}
      <div className="product-card-body">
        <div className="stock-info">
          <span className="info-label">Quantidade</span>
          <span className="info-value">{product.quantity} un.</span>
        </div>
        <div className="stock-info">
          <span className="info-label">Preço</span>
          {/* Formata o preço no padrão brasileiro (R$ 59,90) */}
          <span className="info-value">{product.price?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</span>
        </div>
      </div>
      
      {/* Rodapé: Badge de status + botões de ação (Editar/Excluir) */}
      <div className="product-card-footer">
        <div className="status-badge">
          {getStatusIcon(product.stockStatus)}
          {/* Substitui underscores por espaços no texto do status (ex: ESTOQUE_BAIXO → ESTOQUE BAIXO) */}
          <span>{product.stockStatus?.replace('_', ' ')}</span>
        </div>
        <div className="card-actions">
          {/* Os botões só aparecem se as callbacks foram passadas como props */}
          {onEdit && <button className="btn-secondary btn-sm" onClick={() => onEdit(product.id)}>Editar</button>}
          {onDelete && <button className="btn-danger btn-sm" onClick={() => onDelete(product.id)}>Excluir</button>}
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
