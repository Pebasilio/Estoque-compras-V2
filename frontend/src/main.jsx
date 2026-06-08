import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.jsx'

// ==========================================
// PONTO DE ENTRADA DO REACT (O INÍCIO DE TUDO)
// Aqui o React "gruda" toda a nossa aplicação na <div> com id="root" que existe lá no index.html
// ==========================================
createRoot(document.getElementById('root')).render(
  // O StrictMode ajuda a encontrar bugs rodando o código 2 vezes em modo de desenvolvimento
  <StrictMode>
    <App />
  </StrictMode>,
)
