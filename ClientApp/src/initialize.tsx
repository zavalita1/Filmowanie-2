import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { ConnectedRouter } from 'connected-react-router';
import { createBrowserHistory } from 'history';
import configureStore from './store/configureStore';
import { ApplicationState } from './store';
import App from './App';
import { actionCreators } from "./store/User";
import { AnyAction, Store } from 'redux';

export function RenderReactDOM(store: Store, history: any) {
    ReactDOM.render(
        <Provider store={store}>
            <ConnectedRouter history={history}>
                <App />
            </ConnectedRouter>
            <link rel="preconnect" href="https://fonts.googleapis.com" />
            <link rel="preconnect" href="https://fonts.gstatic.com" />
            <link
                rel="stylesheet"
                href="https://fonts.googleapis.com/css2?family=Lora:wght@300;400;500;600;700&display=swap"
            />
        </Provider>,
        document.getElementById('root'));
}

let store: Store<ApplicationState, AnyAction> | undefined = undefined;

export function InitStore() {
    const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href') as string;
    const history = createBrowserHistory({ basename: baseUrl });

    store = configureStore(history);

    return { history, store };
}

export function InitActions(store: Store) {
    const promise = fetch('api/voting/state').then(async response => {
        const data = await response.json();
        if (data.status === "Results") {
            store.dispatch({ type: 'VOTING_ENDED' });
        }
        else if (data.status === "Voting") {
            store.dispatch({ type: 'VOTING_STARTED' });
        }
    }).catch((ex) => {
        debugger;
        // TODO
    });

    store.dispatch(actionCreators.getUser(true) as any);
    return promise;
}

export function getStore() { return store; };

