import { KnownAction as UserAction, IUser, UnloggedAction } from "./types";
import { KnownAction as AppAction } from '../App/types';
import * as appActionCreators from '../App/actionCreators';
import { AppThunkAction } from '../';
import fetchWrapperBuilder from '../../fetchWrapper';
import { VotingConcludedAction, VotingStartedAction } from "../votingState";

type ActionTypes = UserAction | AppAction | VotingConcludedAction | VotingStartedAction;

const login = (code: string): AppThunkAction<ActionTypes> => (dispatch, getState) => {
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

    fetchWrapper<any>('api/account/login/code', fetchOptions).then(response => {
        if (response === undefined){
            return;
        }

        // TODO mapping
        const userClaims : IUser = {...response};
        return getNominationsAndStateData(userClaims, fetchWrapper, dispatch);
    });
}

const basicLogin = (mail: string, password: string): AppThunkAction<ActionTypes> => (dispatch, getState) => {
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

    fetchWrapper<any>('api/account/login/basic', fetchOptions).then(response => {
        if (response === undefined){
            return;
        }
        
        // TODO mapping
        const userClaims : IUser = {...response};
        return getNominationsAndStateData(userClaims, fetchWrapper, dispatch);
    });
}

const signUp = (mail:string, password: string) : AppThunkAction<ActionTypes> => (dispatch, getState) => {
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

    fetchWrapper<any>('api/account/signup', fetchOptions).then(response => {
        // TODO mapping
        const userClaims : IUser = {...response};
        setUser(userClaims, dispatch);
    });
}

const
    getUser = (init?: boolean): AppThunkAction<ActionTypes> => (dispatch, getState) => {
    dispatch({ type: 'LOGGING' });
    const fetchWrapper = fetchWrapperBuilder().build();

    fetchWrapper<any>('api/account').then(response => {
        // TODO mapping
        const userClaims : IUser = {...response};
        return getNominationsAndStateData(userClaims, fetchWrapper, dispatch);
    }).catch(error => {
       console.log('error during getting user', error);
    });
}

const getNominationsAndStateData = (userClaims: IUser, fetchWrapper: <T>(path: string, options?: RequestInit) => Promise<T>, dispatch: (arg: ActionTypes) => void) => {
    const nominationsPromise = fetchWrapper<any>('api/nominations');
        const promise = fetch('api/voting/state').then(async response => {
            const data = await response.json();
            if (data.status === "Results") {
                dispatch({ type: 'VOTING_ENDED' });
            }
            else if (data.status === "Voting") {
                dispatch({ type: 'VOTING_STARTED' });
            }
        }).catch((ex) => {
            debugger;
            // TODO
        });

        return Promise.all([promise, nominationsPromise]).then(dtos => {
            userClaims.hasNominations = dtos[1].nominations?.length > 0;
            setUser(userClaims, dispatch);
        });
}

const setUser = (user: IUser, dispatch: (action: UserAction | AppAction) => void) => {
    dispatch({ type: 'LOGGED', payload:  user });
    dispatch(appActionCreators.actionCreators.setLoading(false));
}

const loggedOut = () => ({type: 'UNLOGGED'} as UnloggedAction);

const logOut = () : AppThunkAction<UnloggedAction> => async (dispatch, getState) => {
    await fetch('api/account/logout', { method: 'POST' });
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
