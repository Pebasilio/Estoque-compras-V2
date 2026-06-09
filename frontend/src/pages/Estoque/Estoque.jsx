import React, { useState, useEffect } from 'react';
import { produtoService } from '../../services/produtoService';
import ProductTable from '../../components/ProductTable/ProductTable';
import { X } from 'lucide-react';
import './Estoque.css';

// ==========================================
// TELA DE ESTOQUE (Movimentações de Entrada e Saída)
// Tela essencial que controla o Modal (A janelinha que pula na tela)
// ==========================================
const Estoque = () => {
  const [produtos, setProdutos] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Modal State
  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState('entry'); // 'entry' ou 'exit'
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [form, setForm] = useState({ quantity: 1, reason: '', user: '' });

  useEffect(() => {
    loadProdutos();
  }, []);

  const loadProdutos = async () => {
    setLoading(true);
    try {
      const data = await produtoService.getProdutos();
      setProdutos(data);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // Função que ACORDA a janelinha Modal e configura ela pra "Entrada" ou "Saída"
  const openModal = (product, type) => {
    setSelectedProduct(product); // Guarda qual produto clicamos
    setModalType(type); // 'entry' | 'exit'
    setForm({ quantity: 1, reason: '', user: '' }); // Reseta o form para não vir lixo
    setModalOpen(true); // O estado muda, e o React renderiza a caixa preta na tela!
  };

  // Quando o cara clica em "Confirmar" dentro da Modal
  const handleStockAction = async (e) => {
    e.preventDefault();
    if (!selectedProduct) return;

    try {
      if (modalType === 'entry') {
        // Envia os dados para a API (Post /api/stock/entry)
        await produtoService.registraEntrada(selectedProduct.id, form.quantity, form.reason, form.user);
      } else {
        // Envia os dados para a API (Post /api/stock/exit)
        await produtoService.registraSaida(selectedProduct.id, form.quantity, form.reason, form.user);
      }
      setModalOpen(false); // Fecha a janelinha
      loadProdutos(); // Recarrega a tabela de fundo pra mostrar o novo número
    } catch (error) {
      alert(`Erro ao registrar ${modalType === 'entry' ? 'entrada' : 'saída'}. Verifique o estoque atual.`);
    }
  };

  return (
    <div className="page-wrapper">
      <div className="stock-header">
        <h2>Movimentação de Estoque</h2>
        <p className="text-muted">Registre entradas e saídas de peças rapidamente.</p>
      </div>

      {loading ? (
        <div className="loading-state">Carregando inventário...</div>
      ) : (
        <ProductTable 
          products={produtos} 
          onEntry={(p) => openModal(p, 'entry')}
          onExit={(p) => openModal(p, 'exit')}
        />
      )}

      {/* Modal */}
      {modalOpen && (
        <div className="modal-overlay">
          <div className="modal-content glass-card">
            <div className="modal-header">
              <h3 className={modalType === 'entry' ? 'text-ok' : 'text-warning'}>
                {modalType === 'entry' ? 'Registrar Entrada' : 'Registrar Saída'}
              </h3>
              <button className="icon-btn-small" onClick={() => setModalOpen(false)}><X size={20} /></button>
            </div>
            
            <p className="modal-subtitle">Produto: <strong>{selectedProduct?.name}</strong></p>
            <p className="text-muted text-sm mb-4">Estoque Atual: {selectedProduct?.quantity} un.</p>
            
            <form onSubmit={handleStockAction} className="modal-form">
              <div className="form-group">
                <label className="input-label">Quantidade</label>
                <input 
                  type="number" 
                  className="input-field" 
                  min="1"
                  value={form.quantity}
                  onChange={(e) => setForm({...form, quantity: parseInt(e.target.value) || 1})}
                  required 
                />
              </div>
              <div className="form-group">
                <label className="input-label">Motivo</label>
                <input 
                  type="text" 
                  className="input-field" 
                  placeholder={modalType === 'entry' ? "Ex: Reposição fornecedor" : "Ex: Venda balcão"}
                  value={form.reason}
                  onChange={(e) => setForm({...form, reason: e.target.value})}
                  required 
                />
              </div>
              <div className="form-group">
                <label className="input-label">Responsável</label>
                <input 
                  type="text" 
                  className="input-field" 
                  placeholder="Nome do usuário"
                  value={form.user}
                  onChange={(e) => setForm({...form, user: e.target.value})}
                  required 
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setModalOpen(false)}>Cancelar</button>
                <button type="submit" className="btn-primary">
                  Confirmar {modalType === 'entry' ? 'Entrada' : 'Saída'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Estoque;
