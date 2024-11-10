import { Action, Reducer } from "redux";
import { KnownAction, AppState, ErrorOccurredAction } from "./types";
import { ReloadMovieListAction } from "../MoviesList";
import { NominatingWaitingForPosterAction, NominatingWaitingForPostersAction, LoadingNominationsEndedAction, LoadingNominationsStartedAction } from "../Nominations/types";

export const reducer: Reducer<AppState> = (state: AppState | undefined, incomingAction: Action): AppState => {
    if (state === undefined) {
        const theme = window.localStorage?.getItem('theme') === 'dark' ? 'dark' : 'light';
        return { isLoading: true, isError: false, theme };
    }

    const action = incomingAction as KnownAction | ReloadMovieListAction | NominatingWaitingForPostersAction | NominatingWaitingForPosterAction | LoadingNominationsStartedAction | LoadingNominationsEndedAction;
    switch (action.type) {
        case 'LOADING':
            return { ...state, isLoading: true };
        case 'LOADED':
            return { ...state, isLoading: false };
        case 'ERROR_OCCURRED':
            const errorOccurredAction = incomingAction as ErrorOccurredAction;
            return { ...state, isError: true, errorMessage: errorOccurredAction.payload, isLoading: false };
        case 'ERROR_AKNOWLEDGED':
            return { ...state, isError: false, errorMessage: undefined };
        case 'INFO_AKNOWLEDGED':
            return { ...state, isError: false, infoMessage: undefined };
        case 'SET_THEME':
            window.localStorage?.setItem("theme", action.payload);
            return { ...state, theme: action.payload };
        case 'RELOAD_MOVIE_LIST':
            return { ...state, infoMessage: action.payload?.infoMessage};
        case 'NOTIFICATION_OCCURRED':
            return { ...state, infoMessage: action.payload };
        case 'NOMINATING_MOVIE_WAITING_FOR_POSTERS':
            return { ...state, isLoading: true }
        case 'NOMINATING_MOVIE_WAITING_FOR_POSTER_PICK':
            return { ...state, isLoading: false }
        case 'LOADING_NOMINATIONS_DATA_STARTED':
            return { ...state, isLoading: true }
        case 'LOADING_NOMINATIONS_DATA_ENDED':
            return { ...state, isLoading: false }
            
        default:
            return state;
    }
};
