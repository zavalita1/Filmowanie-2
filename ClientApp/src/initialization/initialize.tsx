import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import configureStore from '../store/configureStore';
// import { ApplicationState } from '../store';
// import { actionCreators } from "../store/User";
import { Action, Store } from 'redux';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import getRouter from '../Routes';
import Layout from '../components/Layout';

export function RenderReactDOM(store: Store) {
    const spaContainer = document.getElementById('root');
    const root = ReactDOM.createRoot(spaContainer!, { });
    const router = getRouter();

    root.render(
        // TODO <React.StrictMode>
        <Provider store={store}>
            {/* <Layout> */}
            <RouterProvider router={router}>
            </RouterProvider>
            <link rel="preconnect" href="https://fonts.googleapis.com" />
            <link rel="preconnect" href="https://fonts.gstatic.com" />
            <link
                rel="stylesheet"
                href="https://fonts.googleapis.com/css2?family=Lora:wght@300;400;500;600;700&display=swap"
            />
        {/* </Layout> */}
        </Provider>
        );
}

// let store: Store<ApplicationState, Action> | undefined = undefined;

export function InitStore() {
    const store = configureStore();
    return store;
}

export function InitActions(store: Store) {
    const promise = fetch('state').then(async response => {
    //     const data = await response.json();
    //     if (data.state === "Results") {
    //         store.dispatch({ type: 'VOTING_ENDED' });
    //     }
    //     else if (data.state === "Voting") {
    //         store.dispatch({ type: 'VOTING_STARTED' });
    //     }
    // }).catch((ex) => {
    //     debugger;
    //     // TODO
     });

    // store.dispatch(actionCreators.getUser(true) as any);
     return promise;
}

export function getStore() { 
    // return store; 
};

