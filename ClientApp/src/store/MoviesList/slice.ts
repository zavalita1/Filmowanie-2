import { createSlice, createAction } from '@reduxjs/toolkit';
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { MoviesListState, VoteDTO } from './types';
import { actions as appActions } from '../App/slice'

const initialState: MoviesListState = { movies: [], isStale: true, allVotesAbsSum: 7 };

const api = createApi({
    baseQuery: fetchBaseQuery({
        baseUrl: ''
    }),
    tagTypes: ['Votes'],
    endpoints: build => ({
        vote: build.mutation<{}, VoteDTO>({
            query: ((dto: VoteDTO) => ({
               url: 'votes/vote',
               method: 'POST',
               body: dto    
            })),
            invalidatesTags: ['Votes'],
            async onQueryStarted(arg, { dispatch, getState, queryFulfilled, requestId, extra, getCacheEntry }) {
                dispatch(appActions.setLoading({ isLoading: true}));

                queryFulfilled.catch(() => dispatch(appActions.setError({ errorMessage: 'coś się zerao, spróbuj ponownie.'})))
            },
            async onCacheEntryAdded(arg, { dispatch }) {
                dispatch(appActions.setLoading({ isLoading: false }));
            },
        }),
        getList: build.query({
            query: () => 'movies/list',
            transformResponse: (baseQueryReturnValue) => {
               debugger;
               return 0;
            }
        })
    })
});

const slice = createSlice({
    name: 'MoviesList',
    initialState,
    reducers: { },
    extraReducers: builder => {
        builder.addMatcher(api.endpoints.vote.matchFulfilled, (state, action) => {

        })
    }
});

const { } = slice;
const { useVoteMutation, useGetListQuery } = api;

export const actions = { useVoteMutation, useGetListQuery };
export const reducer = api.reducer;
