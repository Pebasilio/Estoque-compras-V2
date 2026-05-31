import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, LayoutGrid, List as ListIcon } from 'lucide-react';
import { produtoService } from '../../services/produtoService';
import ProductTable from '../../components/ProductTable/ProductTable';
import ProductCard from '../../components/ProductCard/ProductCard';
import './Produtos.css';

const Produtos = () => {
  const navigate = useNavigate();
  const [produtos, setProdutos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState('list'); // 'list' | 'grid'

  useEffect(() => {
    loadProdutos();
  }, []);

  const loadProdutos = async () => {
    setLoading(true);
    try {
      const data = await produtoService.getProdutos();
      setProdutos(data);
    } catch (error) {
      console.error('Erro ao carregar produtos', error);
      alert('Não foi possível carregar os produtos.');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm('Tem certeza que deseja excluir este produto?')) {
      try {
        await produtoService.deleteProduto(id);
        setProdutos(produtos.filter(p => p.id !== id));
      } catch (error) {
        alert('Erro ao excluir produto.');
      }
    }
  };

  const handleEdit = (id) => {
    navigate(`/produtos/editar/${id}`);
  };

  return (
    <div className="page-wrapper">
      <div className="page-header-actions">
        <div className="view-toggles">
          <button 
            className={`icon-btn ${viewMode === 'list' ? 'active' : ''}`} 
            onClick={() => setViewMode('list')}
            title="Ver em Lista"
          >
            <ListIcon size={20} />
          </button>
          <button 
            className={`icon-btn ${viewMode === 'grid' ? 'active' : ''}`} 
            onClick={() => setViewMode('grid')}
            title="Ver em Grade"
          >
            <LayoutGrid size={20} />
          </button>
        </div>
        
        <button className="btn-primary" onClick={() => navigate('/produtos/novo')}>
          <Plus size={20} /> Novo Produto
        </button>
      </div>

      {loading ? (
        <div className="loading-state">Carregando catálogo de produtos...</div>
      ) : (
        viewMode === 'list' ? (
          <ProductTable 
            products={produtos} 
            onEdit={handleEdit} 
            onDelete={handleDelete} 
          />
        ) : (
          <div className="products-grid">
            {produtos.map(product => (
              <ProductCard 
                key={product.id} 
                product={product} 
                onEdit={handleEdit} 
                onDelete={handleDelete} 
              />
            ))}
            {produtos.length === 0 && (
              <div className="empty-state glass-card" style={{ gridColumn: '1 / -1' }}>
                <p>Nenhum produto cadastrado.</p>
              </div>
            )}
          </div>
        )
      )}
    </div>
  );
};

export default Produtos;
