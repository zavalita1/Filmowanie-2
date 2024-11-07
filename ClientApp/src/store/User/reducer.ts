// import { Action, Reducer } from "redux";
// import { IUser, KnownAction, UserState } from "./types";
// import { InitialState } from "../configureStore";

// export const reducer: Reducer<UserState> = (state: UserState | undefined, incomingAction: Action): UserState => {
//     if (state === undefined) {
//         state = InitialState.user;
//     }

//     const action = incomingAction as KnownAction;
//     switch (action.type) {
//         case 'LOGGING':
//             return {...state, isStale: true};
//         case 'LOGGED':
//             return {...state, user: action.payload, isStale: false };
//         case 'UNLOGGED':
//             return {...state, isStale: false, user: undefined };
//         default:
//             return state;
//     }
// };
