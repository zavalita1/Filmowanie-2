import { BaseQueryApi, FetchArgs, fetchBaseQuery, FetchBaseQueryArgs } from '@reduxjs/toolkit/query/react';
import { GlobalConfigSlice } from '../slices/globalConfigSlice';

export const baseQuery = (baseQueryArgs: FetchBaseQueryArgs = {}) => async (
  fetchArgs : string | FetchArgs = '',
  api: BaseQueryApi,
  extraOptions = {},
) => {
  const { getState } = api;
  const state : GlobalConfigSlice = (getState() as any).global; 

  const fetchUrl = typeof (fetchArgs) === "string" ? fetchArgs : fetchArgs.url;
  const baseUrl = fetchUrl.startsWith(state.apiUrl) ? "" : state.apiUrl;

  return fetchBaseQuery({
    ...baseQueryArgs,
    baseUrl,
    timeout: state.defaultApiTimeout,
  })(fetchArgs, api, extraOptions);
};