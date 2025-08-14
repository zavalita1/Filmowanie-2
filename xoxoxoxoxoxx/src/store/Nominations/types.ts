import { IMovie } from "../MoviesList/types";

export interface NominationsState {
    nominations?: NominationDecade[];
    pendingNomination?: { movieFilmwebUrl: string, posterUrl?: string, possiblePosterUrls?: string[] }
    waitingForPosterPick?: boolean;
    moviesThatCanBeNominatedAgain?: IMovie[]
}

interface BaseAction { payload?: any }
export interface NominatingAction extends BaseAction { type: 'NOMINATING_MOVIE_START', payload: { movieFilmwebUrl: string }}
export interface NominatingWaitingForPosterAction extends BaseAction { type: 'NOMINATING_MOVIE_WAITING_FOR_POSTER_PICK', payload: { posterUrls: string[]} }
export interface NominatingPosterChosenAction extends BaseAction { type: 'NOMINATING_MOVIE_POSTER_CHOSEN', payload: { chosenPosterUrl: string} }
export interface NominatingWaitingForPostersAction extends BaseAction { type: 'NOMINATING_MOVIE_WAITING_FOR_POSTERS' }
export interface NominatingPosterPickedAction extends BaseAction { type: 'NOMINATING_MOVIE_POSTER_PICKED', payload: {posterUrl: string} }
export interface NominatedAction extends BaseAction { type: 'NOMINATING_MOVIE_END', payload: { success: boolean, decade?: NominationDecade}  }
export interface NominatingCancelledAction extends BaseAction { type: 'NOMINATING_MOVIE_CANCELLED' }
export interface NominatingConfirmedAction extends BaseAction { type: 'NOMINATING_MOVIE_CONFIRMED' }
export interface LoadingNominationsStartedAction extends BaseAction { type: 'LOADING_NOMINATIONS_DATA_STARTED' }
export interface LoadingNominationsEndedAction extends BaseAction { type: 'LOADING_NOMINATIONS_DATA_ENDED', payload: { moviesThatCanBeNominatedAgain: IMovie[], nominations: NominationDecade[] } }

export type KnownAction = LoadingNominationsStartedAction | LoadingNominationsEndedAction | NominatedAction | NominatingAction | NominatingCancelledAction | NominatingConfirmedAction | NominatingWaitingForPosterAction | NominatingPosterPickedAction | NominatingWaitingForPostersAction | NominatingPosterChosenAction;

export type NominationDecade = '2020s' | '2010s' | '2000s' | '1990s' | '1980s' | '1970s' | '1960s' | '1950s' | '1940s';
