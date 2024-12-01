import { AnyAction, Store } from 'redux';
import { FetchWrapperBuilder } from './FetchWrapperBuilder';
import { getStore } from './initialize';
import { ApplicationState } from './store';
import * as appActionCreators from './store/App/actionCreators';
import * as userActionCreators from './store/User/actionCreators';

const successFullCodes = [200, 201, 202, 203, 204, 205, 206, 300, 301, 302, 303, 304, 307, 308];
type FetchWrapperConfiguration = RequestInit  & 
{ 
    deserializeResponse: boolean,
    codesWithHandlers?: {code: number, handler: any}[],
    minRequestTimeMs?: number,
    timeoutMs?: number;
};

export const fetchWrapper = <T>(path: string, options?: FetchWrapperConfiguration) => {
    const store = getStore();
    store?.dispatch(appActionCreators.actionCreators.setLoading(true));

    if (options?.timeoutMs !== undefined) {
        options.signal = AbortSignal.timeout(options.timeoutMs);
    }
    const delayPromise = options?.minRequestTimeMs !== undefined ? new Promise(res=>setTimeout(()=>res(1), options.minRequestTimeMs)) : Promise.resolve(1);
    
    let promise = Promise.all([fetch(path, options), delayPromise]).then(responseArray => responseArray[0]).then(response => {
        const customHandler = options?.codesWithHandlers?.find(x => x.code === response.status)
        if (customHandler !== undefined) {
            customHandler.handler(response);
            return;
        }

        if (!successFullCodes.includes(response.status)) {
            if (response.status === 401) {
                store?.dispatch(userActionCreators.actionCreators.loggedOut());
                
                response.text().then(t => {
                    if (t !== "Please log in") {
                        store?.dispatch(appActionCreators.actionCreators.setError("Czas sesji upłynął, zaloguj się ponownie."));
                    }
                });
                store?.dispatch(appActionCreators.actionCreators.setLoading(false));
                throw new Error('Unauthorized');
            }
            else if (response.status === 400) {
                return response.json().then(message => {
                    store?.dispatch(appActionCreators.actionCreators.setError(message));
                    store?.dispatch(appActionCreators.actionCreators.setLoading(false));
                }).catch(() => HandleGeneralError(store));
            }
            else {
                HandleGeneralError(store);
                throw new Error('General error');
            }
        }

        return response.json().then(json => json as T).catch(() => HandleGeneralError(store));
    }).catch((ex) => {
        if(ex.message !== 'Unauthorized') {
            HandleGeneralError(store);
        }

        throw new Error('General error');
    });

    return promise as Promise<T>;
};

function HandleGeneralError(store: Store<ApplicationState, AnyAction> | undefined) {
    store?.dispatch(appActionCreators.actionCreators.setError("coś się zerao, spróbuj ponownie."));
                    store?.dispatch(appActionCreators.actionCreators.setLoading(false));
}

export default function getFetchWrapperBuilder() { return new FetchWrapperBuilder(); };