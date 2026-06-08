import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Save, ArrowLeft } from 'lucide-react';
import { produtoService } from '../../services/produtoService';
import '../CadastroProduto/CadastroProduto.css'; // Reutilizando os estilos do formulário

const EditarProduto = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [categorias, setCategorias] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  
  const [formData, setFormData] = useState({
    name: '',
    categoryId: '',
    quantity: 0,
    reorderThreshold: 5,
    price: 0
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        const cats = await produtoService.getCategorias();
        setCategorias(cats);
        
        const produto = await produtoService.getProdutoById(id);
        if (produto) {
          setFormData({
            name: produto.name,
            categoryId: produto.categoryId,
            quantity: produto.quantity,
            reorderThreshold: produto.reorderThreshold,
            price: produto.price
          });
        }
      } catch (error) {
        alert('Erro ao carregar os dados do produto.');
        navigate('/produtos');
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, [id, navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'quantity' || name === 'reorderThreshold' || name === 'categoryId' 
                ? parseInt(value) || 0 
                : name === 'price' ? parseFloat(value) || 0 : value
    }));
  };

  // Handler do formulário: Dispara quando o usuário clica em "Atualizar Produto"
  const handleSubmit = async (e) => {
    e.preventDefault(); // Evita o comportamento padrão do HTML de recarregar a página
    setSaving(true); // Ativa o spinner/loading no botão
    try {
      // CORREÇÃO IMPORTANTE: O backend C# exige que o ID do produto esteja dentro do corpo da requisição (product.Id)
      // além de estar na URL. Sem isso, a API retorna 400 Bad Request por incompatibilidade de IDs.
      // O spread (...formData) copia todos os campos e adicionamos o 'id' convertido para número inteiro.
      await produtoService.updateProduto(id, { ...formData, id: parseInt(id) });
      navigate('/produtos'); // Redireciona de volta para a listagem após salvar com sucesso
    } catch (error) {
      alert('Erro ao atualizar produto.');
    } finally {
      setSaving(false); // Desativa o loading independente de sucesso ou erro
    }
  };

  if (loading) return <div className="loading-state">Carregando dados...</div>;

  return (
    <div className="form-page-wrapper">
      <div className="form-header-actions">
        <button className="btn-secondary" onClick={() => navigate('/produtos')}>
          <ArrowLeft size={20} /> Voltar
        </button>
      </div>

      <div className="glass-card form-card">
        <h2 className="form-title">Editar Produto #{id}</h2>
        <p className="form-subtitle">Atualize as informações do cadastro desta peça.</p>

        <form onSubmit={handleSubmit} className="product-form">
          <div className="form-row">
            <div className="form-group span-2">
              <label className="input-label">Nome da Roupa</label>
              <input 
                type="text" 
                name="name" 
                className="input-field" 
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
              <label className="input-label">Quantidade Atual</label>
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
              <label className="input-label">Limite de Alerta</label>
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
            <button type="submit" className="btn-primary" disabled={saving}>
              <Save size={20} /> {saving ? 'Atualizando...' : 'Atualizar Produto'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditarProduto;
