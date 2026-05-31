import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Package, LogIn, AlertCircle } from 'lucide-react';
import { authService } from '../../services/authService';
import './Login.css';

const Login = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await authService.login(email, password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.message || 'Falha na autenticação. Verifique suas credenciais.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-glass-panel">
        <div className="login-header">
          <div className="logo-box">
            <Package size={40} color="#0EA5E9" />
          </div>
          <h2>Bem-vindo ao StockPro</h2>
          <p>Faça login para gerenciar seu inventário</p>
        </div>

        {error && (
          <div className="login-error">
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        <form className="login-form" onSubmit={handleLogin}>
          <div className="form-group">
            <label className="input-label">E-mail</label>
            <input 
              type="email" 
              className="input-field" 
              placeholder="admin@stockpro.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="input-label">Senha</label>
            <input 
              type="password" 
              className="input-field" 
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="btn-primary login-btn" disabled={loading}>
            {loading ? 'Autenticando...' : (
              <>
                <LogIn size={20} />
                Entrar
              </>
            )}
          </button>
        </form>
      </div>
    </div>
  );
};

export default Login;
