import { IMovie, KnownAction as VoteAction } from './types';
import { KnownAction as AppAction } from '../App/types';
import * as appActions from '../App/actionCreators';
import { AppThunkAction } from '../';
import * as repository from '../../repositories/votesRepository';
import getFetchWrapperBuilder from '../../fetchWrapper';

const loadList = (): AppThunkAction<VoteAction | AppAction> => (dispatch, getState) => {
    const fetchWrapper = getFetchWrapperBuilder().useMinRequestTime(500).build();
    
    fetchWrapper<any>(`api/voting/current`)
        .then(response => {
            const mappedData = response.map((x: any) => ({
                movieId: x.movieId,
                userCurrentVotes: x.votes,
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
            } as IMovie)).sort((x: IMovie, y: IMovie) => y.createdYear - x.createdYear);

            dispatch({ type: 'LOADED_VOTES', payload: mappedData });
            dispatch(appActions.actionCreators.setLoading(false));
        })
        .catch(() => {console.log('error in movies list')});

    dispatch({ type: 'LOADING_VOTES', });
}

const vote = (movieId: string, movieTitle: string, votes: number): AppThunkAction<VoteAction | AppAction> => (dispatch, getState) => {
    repository.placeVote(movieId, movieTitle, votes).then(() => {
        dispatch({ type: 'INCREMENT_VOTES', payload: { title : movieTitle, value: votes } });
        dispatch(appActions.actionCreators.setLoading(false));
    });
}

const resetVote = (movieId: string, movieTitle: string): AppThunkAction<VoteAction | AppAction> => (dispatch, getState) => {
    repository.resetVote(movieId, movieTitle).then(() => {
        dispatch({ type: 'RESET_VOTES', payload: { title : movieTitle } });
        dispatch(appActions.actionCreators.setLoading(false));
    });
}

export const actionCreators = {
    increment: (movieId: string, title: string, value: number) => vote(movieId, title, value),
    reset: (movieId: string, title: string) => resetVote(movieId, title),
    loadList: loadList
};
