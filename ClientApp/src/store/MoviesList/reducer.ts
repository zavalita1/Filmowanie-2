// import { Action, Reducer } from "redux";
// import { IMovie, KnownAction, MoviesListState } from "./types";
// import { InitialState } from "../configureStore";

// export const reducer: Reducer<MoviesListState> = (state: MoviesListState | undefined, incomingAction: Action): MoviesListState => {
//     if (state === undefined) {
//         state = InitialState.moviesList;
//     }

//     const action = incomingAction as KnownAction;
//     switch (action.type) {
//         case 'INCREMENT_VOTES':
//             const getIncrementedMovie = (movie: IMovie): IMovie => ({ ...movie, userCurrentVotes: movie.userCurrentVotes + action.payload.value });
//             const newMovies = findMovieAndReplace(state.movies, action.payload.title, getIncrementedMovie)    
//             return {...state, movies: newMovies };
//         case 'RESET_VOTES':
//             const getResetedMovie = (movie: IMovie): IMovie => ({ ...movie, userCurrentVotes: 0 });
//             const newMoviesAfterReset = findMovieAndReplace(state.movies, action.payload.title, getResetedMovie)    
//             return {...state, movies: newMoviesAfterReset };
//         case 'LOADING_VOTES':
//             return {...state, isStale: true};
//         case 'LOADED_VOTES':
//             return {...state, isStale: false, movies: action.payload }
//         case 'RELOAD_MOVIE_LIST': 
//             return { ...state, isStale: true };
//         default:
//             return state;
//     }
// };

// function findMovieAndReplace(movies: IMovie[], title: string, getNewMovie: (movie: IMovie) => IMovie) {
//     const movieIndex = movies.findIndex(x => x.title === title);

//     if (movieIndex === -1) {
//         // TODO
//         throw Error('loo');
//     }

//     const newMovies = [...movies];
//     newMovies[movieIndex] = getNewMovie(movies[movieIndex]);

//     return newMovies;
// }
