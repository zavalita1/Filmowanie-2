import * as React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Pages/Home';
import MoviesList from './components/Pages/MoviesList';
import Results from './components/Pages/Results';
import Admin from './components/Pages/Admin';
import Nominate from './components/Pages/Nominate';
import History from './components/Pages/History';

import './components/css/custom.css'

export default () => {
    return (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/movies-list' component={MoviesList} />
        <Route path='/results' component={Results} />
        <Route path='/admin' component={Admin} />
        <Route path='/nominate' component={Nominate} />
        <Route path='/history' component={History} />
    </Layout>
    )
};
