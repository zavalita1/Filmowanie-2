import { commonOnQueryStarted } from "../../utils/queryStoreWrapper";
import { GlobalConfigSlice, globalConfigSlice } from '../../globalConfigSlice';
import { votingApi } from "../2-Voting/votingApi";
import { UserIncomingDTO } from "./types";

export const adminApi = votingApi
.enhanceEndpoints({addTagTypes: ['UsersList']})
.injectEndpoints({
    endpoints: builder => ({
        endVoting: builder.mutation<void, void, void>({
          query: () => ({url: '/voting/admin/end', method: 'POST'}),
          async onQueryStarted(params, { dispatch, queryFulfilled }) {
            await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
          },
          invalidatesTags: ['VotingStatus']
        }),
        startVoting: builder.mutation<void, void, void>({
          query: () => ({url: '/voting/admin/start', method: 'POST'}),
          async onQueryStarted(params, { dispatch, queryFulfilled }) {
            await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
          },
          invalidatesTags: ['VotingStatus']
        }),
        getAllUsers: builder.query<UserIncomingDTO[], void>({
            query: () => ({url: '/user/all', method: 'GET'}),
            async onQueryStarted(params, { dispatch, queryFulfilled }) {
            await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
            },
            providesTags: ['UsersList']
        }),
        createUser: builder.mutation<void, string, void>({
          query: username => ({url: 'user', method: 'POST', body: { id: '', displayName: username }}),
           async onQueryStarted(params, { dispatch, queryFulfilled }) {
            await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, true, true);
          },
          invalidatesTags: ['UsersList']
        })
    })
});

export const { useStartVotingMutation, useEndVotingMutation, useGetAllUsersQuery, useCreateUserMutation } = adminApi;