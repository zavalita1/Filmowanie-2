import { Action, Reducer } from "redux";
import { IUser, KnownAction, UserState } from "./types";

export const reducer: Reducer<UserState> = (state: UserState | undefined, incomingAction: Action): UserState => {
    if (state === undefined) {
        return { isStale: true };
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'LOGGING':
            return {...state, isStale: true};
        case 'LOGGED':
            return {...state, user: action.payload, isStale: false };
        case 'UNLOGGED':
            return {...state, isStale: false, user: undefined };
        default:
            return state;
    }
};
