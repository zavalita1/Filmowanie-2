import { BaseQueryApi, FetchArgs, fetchBaseQuery, FetchBaseQueryArgs } from '@reduxjs/toolkit/query/react';
import { GlobalConfigSlice } from '../globalConfigSlice';

export const baseQuery = (baseQueryArgs: FetchBaseQueryArgs = {}, baseUrlSuffix = '') => async (
  fetchArgs : string | FetchArgs = '',
  api: BaseQueryApi,
  extraOptions = {},
) => {
  const { getState } = api;
  const state : GlobalConfigSlice = (getState() as any).global; 
  const baseUrl = `${state.apiUrl}${baseUrlSuffix}`;

  return fetchBaseQuery({
    ...baseQueryArgs,
    baseUrl,
  })(fetchArgs, api, extraOptions);
};

export const query = (baseQueryArgs: FetchBaseQueryArgs = {}, baseUrlSuffix = '') => async (
  fetchArgs : string | FetchArgs = '',
  api: BaseQueryApi,
  extraOptions = {},
) => {
  const { getState } = api;
  const state : GlobalConfigSlice = (getState() as any).global; 
  const baseUrl = `${state.apiUrl}${baseUrlSuffix}`;

};

