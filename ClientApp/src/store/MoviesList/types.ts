export interface IMovie {
    title: string, 
    posterUrl: string, 
    userCurrentVotes: number,
    description: string,
    filmwebUrl: string,

    isPlaceholder: boolean,
    createdYear: number,
    duration: string,
    genres: string[],
    actors: string[],
    directors: string[],
    writers: string[],
    originalTitle: string
}

export interface MoviesListState {
    movies: IMovie[],
    isStale: boolean,
    allVotesAbsSum: number
}

interface BaseAction { payload?: any }
export interface IncrementVotesAction extends BaseAction { type: 'INCREMENT_VOTES' }
export interface ResetVotesAction extends BaseAction { type: 'RESET_VOTES' }
export interface LoadingVotesAction extends BaseAction { type: 'LOADING_VOTES' }
export interface LoadedVotesAction extends BaseAction { type: 'LOADED_VOTES' }
export interface ReloadMovieListAction extends BaseAction { type: 'RELOAD_MOVIE_LIST', payload?: { infoMessage: string} }

export type KnownAction = IncrementVotesAction | ResetVotesAction | LoadingVotesAction | LoadedVotesAction | ReloadMovieListAction;
