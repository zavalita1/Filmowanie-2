import { KnownAction as UserAction, IUser, UnloggedAction } from "./types";
import { KnownAction as AppAction } from '../App/types';
import * as appActionCreators from '../App/actionCreators';
import { AppThunkAction } from '../';
import fetchWrapperBuilder from '../../fetchWrapper';

const login = (code: string): AppThunkAction<UserAction | AppAction> => (dispatch, getState) => {
    dispatch({ type: 'LOGGING' });
    
    const body = JSON.stringify({code});
    const fetchOptions = { 
        method: "POST", 
        body, 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    const fetchWrapper = fetchWrapperBuilder().customErrorHandling([400], response => {
        const errorAction = appActionCreators.actionCreators.setError('dawaj dobry kod chujku');
        dispatch(errorAction);
        dispatch({ type: 'UNLOGGED'});
        return;
    }).build();

    fetchWrapper<any>('account/login/code', fetchOptions).then(response => {
        if (response === undefined){
            return;
        }

        // TODO mapping
        const userClaims : IUser = {...response};

        dispatch({ type: 'LOGGED', payload: userClaims });
        dispatch(appActionCreators.actionCreators.setLoading(false));
    });
}

const basicLogin = (mail: string, password: string): AppThunkAction<UserAction | AppAction> => (dispatch, getState) => {
    dispatch({ type: 'LOGGING' });
    
    const body = JSON.stringify({email: mail, password});
    const fetchOptions = { 
        method: "POST", 
        body, 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    const fetchWrapper = fetchWrapperBuilder().customErrorHandling([400], response => {
        const errorAction = appActionCreators.actionCreators.setError('dawaj dobre hasło chujku');
        dispatch(errorAction);
        dispatch({ type: 'UNLOGGED'});
        return;
    }).build();

    fetchWrapper<any>('account/login/basic', fetchOptions).then(response => {
        if (response === undefined){
            return;
        }
        
        // TODO mapping
        const userClaims : IUser = {...response};

        dispatch({ type: 'LOGGED', payload: userClaims });
        dispatch(appActionCreators.actionCreators.setLoading(false));
    });
}

const signUp = (mail:string, password: string) : AppThunkAction<UserAction | AppAction> => (dispatch, getState) => {
    const body = JSON.stringify({email: mail, password});
    const fetchOptions = { 
        method: "POST", 
        body, 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };

    const fetchWrapper = fetchWrapperBuilder().customErrorHandling([400], async response => {
        const responseMsg = await response.text();

        let errorText = '';
        if (responseMsg === 'Not a valid email!') {
            errorText = 'To nie jest poprawny adres mail!';
        }

        const errorAction = appActionCreators.actionCreators.setError(errorText);
        dispatch(errorAction);
        return;
    }).build();

    fetchWrapper<any>('account/signup', fetchOptions).then(response => {
        // TODO mapping
        const userClaims : IUser = {...response};
        setUser(userClaims, dispatch);
    });
}

const getUser = (init?: boolean): AppThunkAction<UserAction | AppAction> => (dispatch, getState) => {
    dispatch({ type: 'LOGGING' });
    const fetchWrapper = fetchWrapperBuilder().build();
    fetchWrapper<any>('account').then(response => {
        // TODO mapping
        const userClaims : IUser = {...response};
        setUser(userClaims, dispatch);
    }).catch(error => {
       console.log('error during getting user', error);
    });
}

const setUser = (user: IUser, dispatch: (action: UserAction | AppAction) => void) => {
    dispatch({ type: 'LOGGED', payload:  user });
    dispatch(appActionCreators.actionCreators.setLoading(false));
}

const loggedOut = () => ({type: 'UNLOGGED'} as UnloggedAction);

const logOut = () : AppThunkAction<UnloggedAction> => async (dispatch, getState) => {
    await fetch('account/logout', { method: 'POST' });
    dispatch({ type: 'UNLOGGED'});
}

export const actionCreators = {
    login: (code: string) => login(code),
    basicLogin: (mail:string, password: string) => basicLogin(mail, password),
    getUser,
    loggedOut,
    logOut,
    signUp,
};
