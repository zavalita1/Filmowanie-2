import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQuery } from '../utils/baseQuery';

export const apiSlice = createApi({
  baseQuery: baseQuery(),
  endpoints: () => ({}),
  reducerPath: 'api',
  keepUnusedDataFor: 60*5, // 5 mins
});
