import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Save, ArrowLeft } from 'lucide-react';
import { produtoService } from '../../services/produtoService';
import './CadastroProduto.css';

const CadastroProduto = () => {
  const navigate = useNavigate();
  const [categorias, setCategorias] = useState([]);
  const [loading, setLoading] = useState(false);
  
  const [formData, setFormData] = useState({
    name: '',
    categoryId: '',
    quantity: 0,
    reorderThreshold: 5,
    price: 0
  });

  useEffect(() => {
    // Carrega categorias para o select
    produtoService.getCategorias().then(data => setCategorias(data)).catch(console.error);
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'quantity' || name === 'reorderThreshold' || name === 'categoryId' 
                ? parseInt(value) || 0 
                : name === 'price' ? parseFloat(value) || 0 : value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await produtoService.createProduto(formData);
      navigate('/produtos');
    } catch (error) {
      alert('Erro ao salvar produto.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="form-page-wrapper">
      <div className="form-header-actions">
        <button className="btn-secondary" onClick={() => navigate('/produtos')}>
          <ArrowLeft size={20} /> Voltar
        </button>
      </div>

      <div className="glass-card form-card">
        <h2 className="form-title">Informações do Produto</h2>
        <p className="form-subtitle">Preencha os dados da nova roupa para o estoque.</p>

        <form onSubmit={handleSubmit} className="product-form">
          <div className="form-row">
            <div className="form-group span-2">
              <label className="input-label">Nome da Roupa</label>
              <input 
                type="text" 
                name="name" 
                className="input-field" 
                placeholder="Ex: Camiseta Básica Algodão"
                value={formData.name}
                onChange={handleChange}
                required 
              />
            </div>
            <div className="form-group">
              <label className="input-label">Categoria</label>
              <select 
                name="categoryId" 
                className="input-field" 
                value={formData.categoryId}
                onChange={handleChange}
                required
              >
                <option value="">Selecione...</option>
                {categorias.map(cat => (
                  <option key={cat.id} value={cat.id}>{cat.name}</option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-row triple">
            <div className="form-group">
              <label className="input-label">Preço (R$)</label>
              <input 
                type="number" 
                name="price" 
                className="input-field" 
                step="0.01"
                min="0"
                value={formData.price}
                onChange={handleChange}
                required 
              />
            </div>
            <div className="form-group">
              <label className="input-label">Estoque Inicial</label>
              <input 
                type="number" 
                name="quantity" 
                className="input-field" 
                min="0"
                value={formData.quantity}
                onChange={handleChange}
                required 
              />
            </div>
            <div className="form-group">
              <label className="input-label">Limite de Alerta (Mínimo)</label>
              <input 
                type="number" 
                name="reorderThreshold" 
                className="input-field" 
                min="1"
                value={formData.reorderThreshold}
                onChange={handleChange}
                required 
              />
            </div>
          </div>

          <div className="form-actions">
            <button type="button" className="btn-secondary" onClick={() => navigate('/produtos')}>Cancelar</button>
            <button type="submit" className="btn-primary" disabled={loading}>
              <Save size={20} /> {loading ? 'Salvando...' : 'Salvar Produto'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CadastroProduto;
