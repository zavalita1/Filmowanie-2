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

export async function InitActions(store: Store) {
    store.dispatch(actionCreators.getUser(true) as any);
    return;
}

export function getStore() { return store; };

export async function requestPushNotificationPermission() {
    const result = await Notification.requestPermission();
    if (result === 'denied') {
        console.error('The user explicitly denied the permission request.');
        return;
    }
    if (result === 'granted') {
        console.info('The user accepted the permission request.');
    }

    const registration = await navigator.serviceWorker.getRegistration();
    const subscribed = await registration?.pushManager.getSubscription();
    if (subscribed) {
        console.info('User is already subscribed.');
        return;
    }
    const subscription = await registration?.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlB64ToUint8Array("BGoh8C2Kn6-swZDbZC9y1fCROtvQyG3q6R4q6c1O1QMgpjjYaRQenNzFfU6uVyoz8oxi5ktPrWut0Jy8LXyMlG0")
    });
    fetch('/pushNotification/add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(subscription)
    });
}


function urlB64ToUint8Array(base64String: string) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

