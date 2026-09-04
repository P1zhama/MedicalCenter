import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider, DictionariesProvider, ToastProvider } from '@mmc/shared';
import '@mmc/shared/styles/tokens.css';
import '@mmc/shared/styles/base.css';
import App from './App';

const container = document.getElementById('root');

if (container === null) {
  throw new Error('Root container is missing in index.html');
}

createRoot(container).render(
  <StrictMode>
    <BrowserRouter>
      <ToastProvider>
        <AuthProvider>
          <DictionariesProvider>
            <App />
          </DictionariesProvider>
        </AuthProvider>
      </ToastProvider>
    </BrowserRouter>
  </StrictMode>,
);
