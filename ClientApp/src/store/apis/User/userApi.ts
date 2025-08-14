import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQuery } from '../../utils/baseQuery';
import { StatusCode } from '../../../consts/httpStatusCodes';
import ky from 'ky';

import type { UserIncomingDTO, UserState } from './types';

export const userApi = createApi({
  baseQuery: baseQuery({
    fetchFn: async (...args) => {
        try {
        const result = await ky(...args);
        return result;
        }
        catch (err) {
            if (err.response.status === StatusCode.Unauthorized) {
                console.log('Unauthorized user');
                return new Response();
            }
            throw err;
        }


    },
  }, 'api/account'),
  tagTypes: ['User'],
  endpoints: (builder) => ({
    getUser: builder.query<UserState | null, void>({
      query: () => '',
      transformResponse: (response: { data: UserIncomingDTO }, meta, arg) => {
        if (response === null)
            return null as (UserState | null);

        return response.data as UserState;
     }
    }),
  }),
  reducerPath: 'userApi',
});

export const { useGetUserQuery } = userApi;
