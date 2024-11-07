// import { Action, Reducer } from "redux";
// import { KnownAction, NominationsState } from "./types";
// import { InitialState } from "../configureStore";

// export const reducer: Reducer<NominationsState> = (state: NominationsState | undefined, incomingAction: Action): NominationsState => {
//     if (state === undefined) {
//         state = InitialState.nominations;
//     }

//     const action = incomingAction as KnownAction;
//     switch (action.type) {
//         case 'NOMINATING_MOVIE_END': 
//             return { 
//                 ...state, 
//                 nominations: action.payload.success ? state.nominations!.filter(x => x != action.payload.decade) : state.nominations,
//                 moviesThatCanBeNominatedAgain: action.payload.success
//                     ? state.moviesThatCanBeNominatedAgain?.filter(x => (x.createdYear - x.createdYear % 10) + 's' !== action.payload.decade)
//                     : state.moviesThatCanBeNominatedAgain 
//             };
//         case 'NOMINATING_MOVIE_START': 
//             return { ...state, pendingNomination: action.payload };
//         case 'NOMINATING_MOVIE_CONFIRMED': 
//             return { ...state, pendingNomination: undefined, waitingForPosterPick: false };
//         case 'NOMINATING_MOVIE_CANCELLED': 
//             return { ...state, pendingNomination: undefined, waitingForPosterPick: false };
//         case 'NOMINATING_MOVIE_WAITING_FOR_POSTER_PICK': 
//             return { ...state, waitingForPosterPick: true, pendingNomination: { ...state.pendingNomination, possiblePosterUrls: action.payload.posterUrls} as any}; // TODO investigate why types
//         case 'NOMINATING_MOVIE_POSTER_CHOSEN': 
//             return { ...state, waitingForPosterPick: false, pendingNomination: { ...state.pendingNomination, posterUrl: action.payload.chosenPosterUrl} as any }; // TODO investigate why types
//         case 'LOADING_NOMINATIONS_DATA_ENDED': 
//             return { ...state, nominations: action.payload.nominations, moviesThatCanBeNominatedAgain: action.payload.moviesThatCanBeNominatedAgain};
//         default:
//             return state;
//     }
// };
