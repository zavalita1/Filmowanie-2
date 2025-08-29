import React from 'react'
import { Provider as ReduxStoreProvider } from 'react-redux'
import { BrowserRouter, Routes, Route } from 'react-router'

import Home from './pages/Home/Home';
import Login from './pages/Login/Login';
import MoviesList from './pages/MoviesList/MoviesList';
import Results from './pages/Results/Results';
import About from './pages/About/About';
import Admin from './pages/Admin/Admin';
import Nomination from './pages/Nomination/Nomination';
import { store } from './store/store';

const App: React.FC = () => {
  const routeProps = {} as any;
  
  return (
    <ReduxStoreProvider store={store}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Home {...routeProps}/>} />
          <Route path="/about" element={<About {...routeProps}/>} />
          <Route path="/login" element={<Login />} />
          <Route path="/movieslist" element={<MoviesList {...routeProps}/>} />
          <Route path="/results" element={<Results {...routeProps}/>} />
          <Route path="/admin" element={<Admin {...routeProps}/>} />
          <Route path="/nominate" element={<Nomination {...routeProps}/>} />
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
