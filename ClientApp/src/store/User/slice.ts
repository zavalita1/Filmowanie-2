import { createSlice, createAction } from '@reduxjs/toolkit';
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { UserState, LoginDTO, LoginBasicAuthDTO as BasicAuthDTO } from './types';

const initialState: UserState =  { isStale: true };

const api = createApi({
    baseQuery: fetchBaseQuery({
        baseUrl: ''
    }),
    tagTypes: ['User'],
    endpoints: build => ({
        loginWithCode: build.mutation<{}, LoginDTO>({
            query: (dto => ({
               url: 'account/login/code',
               method: 'POST',
               body: dto    
            })),
            invalidatesTags: ['User'],
            async onQueryStarted(arg, { dispatch, getState, queryFulfilled, requestId, extra, getCacheEntry }) {
                // TODO
            },
            async onCacheEntryAdded(arg, { dispatch }) {
                // TODO
            },
        }),
        loginWithBasicAuth: build.mutation<{}, BasicAuthDTO>({
            query: (dto => ({
               url: 'account/login/basic',
               method: 'POST',
               body: dto    
            })),
            invalidatesTags: ['User'],
            async onQueryStarted(arg, { dispatch, getState, queryFulfilled, requestId, extra, getCacheEntry }) {
                // TODO
            },
            async onCacheEntryAdded(arg, { dispatch }) {
                // TODO
            },
        }),
        signUp: build.mutation<{}, BasicAuthDTO>({
            query: (dto => ({
               url: 'account/signup',
               method: 'POST',
               body: dto    
            })),
            invalidatesTags: ['User'],
            async onQueryStarted(arg, { dispatch, getState, queryFulfilled, requestId, extra, getCacheEntry }) {
                // TODO
            },
            async onCacheEntryAdded(arg, { dispatch }) {
                // TODO
            },
        }),
        logOut: build.mutation<{}, void>({
            query: (() => ({
               url: 'account/logout',
               method: 'POST',
            })),
            invalidatesTags: ['User'],
            async onQueryStarted(arg, { dispatch, getState, queryFulfilled, requestId, extra, getCacheEntry }) {
                // TODO
            },
            async onCacheEntryAdded(arg, { dispatch }) {
                // TODO
            },
        }),
        getUser: build.query({
            query: () => 'account',
            transformResponse: (baseQueryReturnValue) => {
                // TODO
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
        builder.addMatcher(api.endpoints.getUser.matchFulfilled, (state, action) => {
            // TODO
        })
    }
});

const {  } = slice;
const { useGetUserQuery, useLoginWithBasicAuthMutation, useLoginWithCodeMutation, useSignUpMutation, useLogOutMutation } = api;

export const actions = { useGetUserQuery, useLoginWithBasicAuthMutation, useLoginWithCodeMutation, useSignUpMutation, useLogOutMutation };
export const reducer = api.reducer;
