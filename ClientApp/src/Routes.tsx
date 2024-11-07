import * as React from 'react';

import { RouterProvider, createBrowserRouter } from 'react-router-dom';

import Layout from './components/Layout';
import Home from './components/Pages/Home';
// import MoviesList from './components/Pages/MoviesList';
// import Results from './components/Pages/Results';
// import Admin from './components/Pages/Admin';
// import Nominate from './components/Pages/Nominate';
// import History from './components/Pages/History';
import NotFound from  './components/Pages/NotFound';

// import './components/css/custom.css'

export default function getRouter() {
    const router = createBrowserRouter([{
        path: "/",
        element: <Layout />,
        errorElement: <NotFound />,
        children: [
          {
            path: "/",
            element: <Home />
          }
        ]
      },
      // {
      //   path: '/movies-list',
      //   element: <MoviesList />
      // },
      // {
      //   path: '/results',
      //   element: <Results />
      // },
      // {
      //   path: '/admin',
      //   element: <Admin />
      // },
      // {
      //   path: '/nominate',
      //   element: <Nominate />
      // },
      // {
      //   path: '/history',
      //   element: <History />
      // },
    ]);
    return router;
};
