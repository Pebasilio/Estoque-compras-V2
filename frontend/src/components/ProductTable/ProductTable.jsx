import React from 'react';
import { Edit2, Trash2, ArrowUpCircle, ArrowDownCircle } from 'lucide-react';
import './ProductTable.css';

const ProductTable = ({ products, onEdit, onDelete, onEntry, onExit }) => {
  if (!products || products.length === 0) {
    return (
      <div className="empty-state glass-card">
        <p>Nenhum produto encontrado.</p>
      </div>
    );
  }

  const getStatusClass = (quantity, threshold) => {
    if (quantity === 0) return 'text-critical';
    if (quantity <= threshold) return 'text-warning';
    return 'text-ok';
  };

  return (
    <div className="table-container glass-card">
      <table className="modern-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nome</th>
            <th>Categoria</th>
            <th>Preço</th>
            <th>Estoque</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <tr key={product.id}>
              <td className="text-muted">#{product.id}</td>
              <td className="fw-600">{product.name}</td>
              <td>{product.category?.name || 'N/A'}</td>
              <td>{product.price?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</td>
              <td>
                <span className={`stock-pill ${getStatusClass(product.quantity, product.reorderThreshold)}`}>
                  {product.quantity} un.
                </span>
              </td>
              <td className="actions-cell">
                <div className="table-actions">
                  {onEntry && (
                    <button className="icon-btn-small success-hover" title="Entrada" onClick={() => onEntry(product)}>
                      <ArrowUpCircle size={18} />
                    </button>
                  )}
                  {onExit && (
                    <button className="icon-btn-small warning-hover" title="Saída" onClick={() => onExit(product)}>
                      <ArrowDownCircle size={18} />
                    </button>
                  )}
                  {onEdit && (
                    <button className="icon-btn-small primary-hover" title="Editar" onClick={() => onEdit(product.id)}>
                      <Edit2 size={18} />
                    </button>
                  )}
                  {onDelete && (
                    <button className="icon-btn-small danger-hover" title="Excluir" onClick={() => onDelete(product.id)}>
                      <Trash2 size={18} />
                    </button>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ProductTable;
