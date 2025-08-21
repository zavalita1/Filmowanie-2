import { apiSlice } from '../apiSlice';
import { commonOnQueryStarted } from '../../utils/base';
import { GlobalConfigSlice, globalConfigSlice } from '../../globalConfigSlice';
import { StatusCode } from '../../../consts/httpStatusCodes';
import ky from 'ky';


import type { UserIncomingDTO, UserState, LoginWithCodeOutgoingDTO } from './types';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';

export const userApi = apiSlice.injectEndpoints({
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
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      providesTags: ['UserData']
    }),
    loginWithCode: builder.mutation<UserState, LoginWithCodeOutgoingDTO, UserState>({
      query: dto => ({ url: '/account/login/code', method: 'POST', body: dto}),
      transformResponse: (response: UserIncomingDTO, meta, arg) => {
        return response as UserState;
      },
      transformErrorResponse: (response, meta, arg) => {
        debugger;
      },
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      invalidatesTags: ['UserData']
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

export const { useGetUserQuery, useLoginWithCodeMutation, useLogoutMutation } = userApi;
