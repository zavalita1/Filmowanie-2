import React from 'react'
import { Provider as ReduxStoreProvider } from 'react-redux'
import { BrowserRouter, Routes, Route } from 'react-router'

import './features/Counter/index.module.css'
import Counter from './features/Counter/index'
import Home from './pages/Home/Home'
import { store } from './store/store'

const App: React.FC = () => {
  return (
    <ReduxStoreProvider store={store}>
      <BrowserRouter>
        <Routes>
          <Route path="/test" element={<Counter />} />
          <Route path="/" element={<Home />} />
        </Routes>
      </BrowserRouter>
      <link rel="preconnect" href="https://fonts.googleapis.com" />
      <link rel="preconnect" href="https://fonts.gstatic.com" />
      <link
        rel="stylesheet"
        href="https://fonts.googleapis.com/css2?family=Lora:wght@300;400;500;600;700&display=swap"
      />
    </ReduxStoreProvider>
  )
}

export default App
