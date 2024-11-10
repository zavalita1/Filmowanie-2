import { KnownAction as UserAction, NominatedAction, NominatingAction, NominationDecade, NominatingWaitingForPosterAction, NominatingWaitingForPostersAction, LoadingNominationsStartedAction, LoadingNominationsEndedAction } from "./types";
import { KnownAction as AppAction } from '../App/types';
import * as appActionCreators from '../App/actionCreators';
import { AppThunkAction } from '../';
import fetchWrapperBuilder from '../../fetchWrapper';
import { IMovie, ReloadMovieListAction } from "../MoviesList";
import { NominationAknowledgedDTO, NominationsDataDTO, PostersListDTO, VoteAknowledgedDTO } from "../../DTO/Incoming/VotingResultDTO";
import * as userActions from "../User/actionCreators";

const nominateMovie = (movieFilmwebUrl: string, pickPosterManually: boolean): AppThunkAction<NominatingAction | NominatingWaitingForPosterAction | NominatingWaitingForPostersAction | AppAction> => (dispatch, getState) => {
   if (!movieFilmwebUrl.startsWith("https://filmweb.pl") && !movieFilmwebUrl.startsWith("https://www.filmweb.pl")) {
    const errorAction = appActionCreators.actionCreators.setError('to nie jest dobry link do filmweba. Co ty próbujesz osiągnąć? Kim ty w ogóle jesteś?');
    dispatch(errorAction);
    return;
   }

   dispatch({ type: 'NOMINATING_MOVIE_START', payload: {movieFilmwebUrl}});

   if (pickPosterManually) {
    dispatch({type: 'NOMINATING_MOVIE_WAITING_FOR_POSTERS'});
    const fetchWrapper = fetchWrapperBuilder().build();
    fetchWrapper<PostersListDTO>(`movies/posters?movieUrl=${movieFilmwebUrl}`).then(response => {
        dispatch({type: 'NOMINATING_MOVIE_WAITING_FOR_POSTER_PICK', payload: { posterUrls: response.posterUrls }});
    }).catch(error => {
    console.log('error during getting posters', error);
    });
    
   }
}

const loadNominationsData = (): AppThunkAction<LoadingNominationsStartedAction | LoadingNominationsEndedAction> => (dispatch, getState) => {
    dispatch({type: 'LOADING_NOMINATIONS_DATA_STARTED'});

    const fetchWrapper = fetchWrapperBuilder().build();
    fetchWrapper<NominationsDataDTO>('nominateMovie/getData').then(response => {

        const nominations = response.nominations.map(x => x as NominationDecade)
        const moviesThatCanBeNominatedAgain = response.moviesThatCanBeNominatedAgain.map((x: any) => ({
            description: x.description,
            posterUrl: x.posterUrl,
            title: x.movieName,
            filmwebUrl: x.filmwebUrl,

            isPlaceholder: x.isPlaceholder,
            createdYear: x.createdYear,
            duration: x.duration,
            genres: x.genres,
            actors: x.actors.slice(0, 3),
            directors: x.directors,
            writers: x.writers,
            originalTitle: x.originalTitle
        } as IMovie))

        dispatch({type: 'LOADING_NOMINATIONS_DATA_ENDED', payload: { nominations, moviesThatCanBeNominatedAgain}});
    }).catch(error => {
        console.log('error during nomination data fetching', error);
     });
}

const confirmMovieNomination = (): AppThunkAction<NominatedAction | AppAction | ReloadMovieListAction | UserAction> => (dispatch, getState) => {
    const state = getState();
    const pendingNomination = state.nominations?.pendingNomination;
    const posterUrl = state.nominations?.pendingNomination?.posterUrl;
    const body = JSON.stringify({ ...pendingNomination, posterUrl });
    dispatch({type: 'NOMINATING_MOVIE_CONFIRMED'});
   const fetchOptions = { 
       method: "POST", 
       body, 
       headers: { 'content-type': 'application/json;charset=UTF-8', } 
   };
   const fetchWrapper = fetchWrapperBuilder().build();
   fetchWrapper<NominationAknowledgedDTO>('nominateMovie', fetchOptions).then(response => {
    dispatch({ type: 'NOMINATING_MOVIE_END', payload: { decade: response.decade, success: response !== undefined} });
    userActions.actionCreators.getUser(false)(dispatch as any, getState);
    dispatch({ type: 'RELOAD_MOVIE_LIST'});
   }).catch(error => {
    console.log('error during nominating', error);
 });
}

const cancelMovieNomination = () => ({type: 'NOMINATING_MOVIE_CANCELLED'});

const chosePoster = (chosenPosterUrl: string) => ({type: 'NOMINATING_MOVIE_POSTER_CHOSEN', payload: { chosenPosterUrl }})

export const actionCreators = {
    nominateMovie,
    confirmMovieNomination,
    cancelMovieNomination,
    chosePoster,
    loadNominationsData
};
