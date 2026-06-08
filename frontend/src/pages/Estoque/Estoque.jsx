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
  const [produtos, setProdutos] = useState([]); // Lista de todos os produtos vindos da API
  const [movements, setMovements] = useState([]); // Histórico de movimentações (entradas e saídas) para alimentar a tabela inferior
  const [loading, setLoading] = useState(true); // Flag de carregamento para exibir spinner enquanto busca dados
  
  // Modal State
  const [modalOpen, setModalOpen] = useState(false);
  const [modalType, setModalType] = useState('entry'); // 'entry' ou 'exit'
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [form, setForm] = useState({ quantity: 1, reason: '', user: '' });

  // Ao montar o componente pela primeira vez, carrega os produtos E o histórico de movimentações em paralelo
  useEffect(() => {
    loadProdutos(); // Busca a tabela principal de produtos
    loadMovements(); // Busca o histórico de entradas/saídas para a tabela inferior
  }, []); // Array vazio [] = executa apenas 1 vez, quando a tela abre

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

  // Busca o histórico completo de movimentações na rota GET /api/stock/movements
  const loadMovements = async () => {
    try {
      const data = await produtoService.getMovements(); // Chama a API C# que retorna todas as movimentações
      // Ordena decrescente pela data para que as mais recentes apareçam primeiro na tabela
      const sorted = data.sort((a, b) => new Date(b.date) - new Date(a.date));
      setMovements(sorted); // Salva no state para o React renderizar a tabela
    } catch (error) {
      console.error(error); // Loga o erro silenciosamente (não trava a tela principal)
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
      loadMovements(); // Atualiza o histórico
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

      {/* ==========================================
          TABELA DE HISTÓRICO DE MOVIMENTAÇÕES
          Exibe as últimas 15 movimentações (entradas e saídas) registradas no sistema.
          Os dados vêm da rota GET /api/stock/movements do backend C#.
          ========================================== */}
      <div className="stock-header" style={{ marginTop: '3rem' }}>
        <h2>Últimas Movimentações</h2>
        <p className="text-muted">Histórico recente de entradas e saídas do estoque.</p>
      </div>
      
      <div className="glass-card mb-4">
        <table className="custom-table">
          <thead>
            <tr>
              <th>Data/Hora</th>
              <th>Tipo</th>        {/* ENTRADA (verde) ou SAIDA (vermelho) */}
              <th>Produto</th>     {/* Nome do produto no momento da movimentação */}
              <th>Qtd</th>         {/* Quantidade movimentada nesta ação */}
              <th>Motivo</th>      {/* Ex: "Venda", "Reposição", "Avaria" */}
              <th>Usuário</th>     {/* Quem realizou a operação */}
            </tr>
          </thead>
          <tbody>
            {/* slice(0, 15) limita a exibição às 15 movimentações mais recentes para não poluir a tela */}
            {movements.slice(0, 15).map((m) => (
              <tr key={m.id}>
                {/* Converte a data ISO do C# para o formato brasileiro (dd/mm/aaaa hh:mm:ss) */}
                <td>{new Date(m.date).toLocaleString('pt-BR')}</td>
                <td>
                  {/* Aplica cor condicional: verde (text-ok) para ENTRADA, vermelho (text-danger) para SAÍDA */}
                  <span className={m.type === 'ENTRADA' ? 'text-ok fw-bold' : 'text-danger fw-bold'}>
                    {m.type}
                  </span>
                </td>
                <td>{m.productName}</td>
                <td>{m.quantity}</td>
                <td>{m.reason}</td>
                <td>{m.user}</td>
              </tr>
            ))}
            {/* Mensagem amigável caso ainda não exista nenhuma movimentação no banco */}
            {movements.length === 0 && (
              <tr>
                <td colSpan="6" className="text-center text-muted py-4">Nenhuma movimentação registrada.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Estoque;
