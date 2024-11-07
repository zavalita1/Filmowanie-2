import { createSlice, createAction } from '@reduxjs/toolkit';
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { actions as appActions } from '../App/slice'
import { VotingSessionState, VotingState } from './types';

const initialState: VotingSessionState = { state: VotingState.Loading };

const api = createApi({
    baseQuery: fetchBaseQuery({
        baseUrl: ''
    }),
    tagTypes: ['Votes'],
    endpoints: build => ({
        endVotingSession: build.mutation<{}, void>({
            query: ((dto) => ({
               url: 'votes/vote', // TODO
               method: 'POST',
               body: dto    
            })),
            invalidatesTags: ['Votes'],
            async onQueryStarted(arg, { dispatch, getState, queryFulfilled, requestId, extra, getCacheEntry }) {
                dispatch(appActions.setLoading({ isLoading: true}));
                // TODO
                queryFulfilled.catch(() => dispatch(appActions.setError({ errorMessage: 'coś się zerao, spróbuj ponownie.'})))
            },
            async onCacheEntryAdded(arg, { dispatch }) {
                dispatch(appActions.setLoading({ isLoading: false }));
            },
        }),
        startVotingSession: build.mutation<{}, void>({
            query: ((dto) => ({
               url: 'votes/vote', // TODO
               method: 'POST',
               body: dto    
            })),
            invalidatesTags: ['Votes'],
            async onQueryStarted(arg, { dispatch, getState, queryFulfilled, requestId, extra, getCacheEntry }) {
                dispatch(appActions.setLoading({ isLoading: true}));
                // TODO
                queryFulfilled.catch(() => dispatch(appActions.setError({ errorMessage: 'coś się zerao, spróbuj ponownie.'})))
            },
            async onCacheEntryAdded(arg, { dispatch }) {
                dispatch(appActions.setLoading({ isLoading: false }));
            },
        })
    })
});

const slice = createSlice({
    name: 'VotingSession',
    initialState,
    reducers: { },
    extraReducers: builder => {
        builder.addMatcher(api.endpoints.startVotingSession.matchFulfilled, (state, action) => {
            // TODO
        })
    }
});

const { } = slice;
const { useStartVotingSessionMutation, useEndVotingSessionMutation } = api;

export const actions = { useStartVotingSessionMutation, useEndVotingSessionMutation };
export const reducer = api.reducer;
