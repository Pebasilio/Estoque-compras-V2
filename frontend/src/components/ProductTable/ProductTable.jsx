import React from 'react';
import { Edit2, Trash2, ArrowUpCircle, ArrowDownCircle } from 'lucide-react';
import './ProductTable.css';

// ==========================================
// COMPONENTE PRODUCTTABLE (Tabela de Produtos)
// Renderiza a lista de produtos em formato de tabela HTML.
// Aceita callbacks opcionais para ações: editar, excluir, entrada e saída de estoque.
// Se nenhuma callback for passada, o respectivo botão não aparece (renderização condicional).
// ==========================================
const ProductTable = ({ products, onEdit, onDelete, onEntry, onExit }) => {
  // Se não houver produtos, exibe uma mensagem amigável ao invés da tabela vazia
  if (!products || products.length === 0) {
    return (
      <div className="empty-state glass-card">
        <p>Nenhum produto encontrado.</p>
      </div>
    );
  }

  /**
   * getStatusClass: Retorna a classe CSS baseada na relação entre quantidade e limite.
   * - Vermelho (text-critical): estoque zerado
   * - Amarelo (text-warning): estoque abaixo do limite de reposição
   * - Verde (text-ok): estoque saudável
   */
  const getStatusClass = (quantity, threshold) => {
    if (quantity === 0) return 'text-critical';
    if (quantity <= threshold) return 'text-warning';
    return 'text-ok';
  };

  return (
    <div className="table-container glass-card">
      <table className="modern-table">
        {/* Cabeçalho fixo da tabela com os nomes das colunas */}
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
          {/* Itera sobre a lista de produtos e cria uma linha (<tr>) para cada um */}
          {products.map((product) => (
            <tr key={product.id}>
              <td className="text-muted">#{product.id}</td>
              <td className="fw-600">{product.name}</td>
              {/* Operador ?. (optional chaining) evita erro se category for null */}
              <td>{product.category?.name || 'N/A'}</td>
              {/* Formata o preço no padrão brasileiro: R$ 129,90 */}
              <td>{product.price?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</td>
              <td>
                {/* Pill colorida que mostra a quantidade com cor baseada no status */}
                <span className={`stock-pill ${getStatusClass(product.quantity, product.reorderThreshold)}`}>
                  {product.quantity} un.
                </span>
              </td>
              <td className="actions-cell">
                {/* Botões de ação: cada um só aparece se sua callback correspondente foi passada */}
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
