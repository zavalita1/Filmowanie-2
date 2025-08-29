import { apiSlice } from '../apiSlice';
import { commonOnQueryStarted } from '../../utils/queryStoreWrapper';
import { GlobalConfigSlice, globalConfigSlice } from '../../globalConfigSlice';
import { StatusCode } from '../../../consts/httpStatusCodes';

import type { UserIncomingDTO, UserState, LoginWithCodeOutgoingDTO, LoginWithBasicAuthOutgoingDTO } from './types';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';

export const userApi = apiSlice
.enhanceEndpoints({addTagTypes: ['UserData']})
.injectEndpoints({
  endpoints: (builder) => ({
    getUser: builder.query<UserState | null, void>({
      async queryFn(arg, queryApi, extraOptions, fetchWithBQ) {
        const { getState } = queryApi;
        const state: GlobalConfigSlice = (getState() as any).global;
        const baseUrl = `${state.apiUrl}account`;
        const resultWrapped = await fetchWithBQ(baseUrl);
        let isError = !resultWrapped.data;
        let result = { data: resultWrapped.data as UserState | null };

        if (resultWrapped.error?.status === StatusCode.Unauthorized) {
          console.log('Unauthorized user');
          result = { data: null };
          isError = false;
        }

        return !isError
          ? result
          : { error: resultWrapped.error as FetchBaseQueryError }
      },
      providesTags: ['UserData']
    }),
    loginWithCode: builder.mutation<UserState, LoginWithCodeOutgoingDTO, UserState>({
      query: dto => ({ url: '/account/login/code', method: 'POST', body: dto}),
      transformResponse: (response: UserIncomingDTO, meta, arg) => response as UserState,
      transformErrorResponse: (response, meta, arg) => {
        // TODO
        debugger;
      },
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      invalidatesTags: ['UserData']
    }),
    loginWithBasicAuth: builder.mutation<UserState, LoginWithBasicAuthOutgoingDTO, UserState>({
      query: dto => ({ url: '/account/login/basic', method: 'POST', body: dto}),
      transformResponse: (response: UserIncomingDTO, meta, arg) => response as UserState,
      transformErrorResponse: (response, meta, arg) => {
        // TODO
        debugger;
      },
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      invalidatesTags: ['UserData']
    }),
    signUp: builder.mutation<UserState, LoginWithBasicAuthOutgoingDTO, UserState>({
      query: dto => ({ url: '/account/signup', method: 'POST', body: dto}),
      transformResponse: (response: UserIncomingDTO, meta, arg) => response as UserState,
      transformErrorResponse: (response, meta, arg) => {
        // TODO
        debugger;
      },
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      }
    }),
    logout: builder.mutation<any, void>({
      query: () => ({ url: '/account/logout', method: 'POST'}),
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      invalidatesTags: ['UserData']
    })
  })
});

export const { useGetUserQuery, useLoginWithCodeMutation, useLoginWithBasicAuthMutation, useLogoutMutation, useSignUpMutation } = userApi;
