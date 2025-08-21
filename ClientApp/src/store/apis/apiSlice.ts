import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQuery } from '../utils/baseQuery';

export const apiSlice = createApi({
  baseQuery: baseQuery(),
  tagTypes: ['UserData'],
  endpoints: () => ({}),
  reducerPath: 'api'});
