import { BaseQueryApi, FetchArgs, fetchBaseQuery, FetchBaseQueryArgs } from '@reduxjs/toolkit/query/react';
import { GlobalConfigSlice } from '../globalConfigSlice';
import ky from 'ky';

export const baseQuery = (baseQueryArgs: FetchBaseQueryArgs = {}) => async (
  fetchArgs : string | FetchArgs = '',
  api: BaseQueryApi,
  extraOptions = {},
) => {
  const { getState } = api;
  const state : GlobalConfigSlice = (getState() as any).global; 
  const baseUrl = `${state.apiUrl}`;
  return fetchBaseQuery({
    ...baseQueryArgs,
    baseUrl,
  })(fetchArgs, api, extraOptions);
};