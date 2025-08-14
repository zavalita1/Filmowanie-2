import ReactDOM from 'react-dom/client'

import './index.css'
import App from './App'

const root = ReactDOM.createRoot(document.getElementById('root')!)

mockBackendIfNeeded().then(() => root.render(<App />));

async function mockBackendIfNeeded() {
  if (import.meta.env.MODE !== 'mockedBackend')
    return;
  
  const workerEnv = await import('../mocks/browser');
  await workerEnv.worker.start();
}
