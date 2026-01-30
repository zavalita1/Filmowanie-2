import React from 'react';
import { Provider as ReduxStoreProvider } from 'react-redux';
import { BrowserRouter, Routes, Route } from 'react-router';
import { GoogleOAuthProvider } from '@react-oauth/google';

import Home from './pages/Home';
import {Login, Preferences, Admin, Results, MoviesList, Nomination, History, Rules } from './pages';
import { store } from './store/store';

const App: React.FC = () => {
  const routeProps = {} as any;
  const googleClientId = store.getState().global.googleOAuthClientId;
  
  return (
    <GoogleOAuthProvider clientId={googleClientId}>
    <ReduxStoreProvider store={store}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Home {...routeProps}/>} />
          <Route path="/preferences" element={<Preferences {...routeProps}/>} />
          <Route path="/login" element={<Login />} />
          <Route path="/movieslist" element={<MoviesList {...routeProps}/>} />
          <Route path="/results" element={<Results {...routeProps}/>} />
          <Route path="/admin" element={<Admin {...routeProps}/>} />
          <Route path="/nominate" element={<Nomination {...routeProps}/>} />
          <Route path="/history" element={<History {...routeProps}/>} />
          <Route path="/rules" element={<Rules {...routeProps}/>} />
        </Routes>
      </BrowserRouter>
      <link rel="preconnect" href="https://fonts.googleapis.com" />
      <link rel="preconnect" href="https://fonts.gstatic.com" />
      <link
        rel="stylesheet"
        href="https://fonts.googleapis.com/css2?family=Lora:wght@300;400;500;600;700&display=swap"
      />
    </ReduxStoreProvider>
    </GoogleOAuthProvider>
  )
}

export default App
