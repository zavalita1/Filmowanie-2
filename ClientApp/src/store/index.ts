import * as MoviesList from './MoviesList';
import * as User from './User';
import * as VotingSession from './VotingSession';
import * as App from "./App";
// import * as Nominations from "./Nominations";

 export interface ApplicationState {
       moviesList: MoviesList.MoviesListState;
       user: User.UserState;
        votingSession: VotingSession.VotingSessionState;
        app: App.AppState;
//     nominations: Nominations.NominationsState;
 }

export const reducers = {
    moviesList: MoviesList.reducer,
    user: User.reducer,
    votingSession: VotingSession.reducer,
    app: App.reducer,
    // nominations: Nominations.reducer,
};

// // This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// // correctly typed to match your store.
// export interface AppThunkAction<TAction> {
//     (dispatch: (action: TAction) => void, getState: () => ApplicationState): void;
// }
