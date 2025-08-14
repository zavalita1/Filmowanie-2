import * as MoviesList from './MoviesList';
import * as User from './User';
import * as State from './votingState';
import * as App from "./App";
import * as Nominations from "./Nominations";

export interface ApplicationState {
    moviesList: MoviesList.MoviesListState | undefined;
    user: User.UserState | undefined;
    state: State.StateState | undefined;
    app: App.AppState | undefined;
    nominations: Nominations.NominationsState | undefined;
}

export const reducers = {
    moviesList: MoviesList.reducer,
    user: User.reducer,
    state: State.reducer,
    app: App.reducer,
    nominations: Nominations.reducer,
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
    (dispatch: (action: TAction) => void, getState: () => ApplicationState): void;
}
