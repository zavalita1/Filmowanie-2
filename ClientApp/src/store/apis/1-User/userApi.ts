import { apiSlice } from '../apiSlice';
import { commonOnQueryStarted } from '../../utils/queryStoreWrapper';
import { GlobalConfigSlice, globalConfigSlice } from '../../slices/globalConfigSlice';
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
        queryApi.dispatch(globalConfigSlice.actions.setLoading(true));
        const resultWrapped = await fetchWithBQ({url: baseUrl, timeout: 1000 * 60 * 1});
        queryApi.dispatch(globalConfigSlice.actions.setLoading(false));
        const resultData = resultWrapped.data as any;
        let isError = !resultData;
        const useFemaleSuffixes = resultData?.gender === "Female";
        let result = { data: { ...resultData, useFemaleSuffixes } as UserState | null };

        if (resultWrapped.error?.status === StatusCode.Unauthorized) {
          console.log('Unauthorized user');
          result = { data: null };
          isError = false;
        }

        if (isError) {
          return ({ error: resultWrapped.error as FetchBaseQueryError });
        }

        window.localStorage.setItem('isLogged', 'True');
        window.dispatchEvent(new Event('userLogsIn'));
        return result;
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
        window.localStorage.setItem('isLogged', 'False');
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      invalidatesTags: ['UserData']
    })
  })
});

export const { useGetUserQuery, useLoginWithCodeMutation, useLoginWithBasicAuthMutation, useLogoutMutation, useSignUpMutation } = userApi;
