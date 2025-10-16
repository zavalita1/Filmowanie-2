import { commonOnQueryStarted } from "../../utils/queryStoreWrapper";
import { GlobalConfigSlice, globalConfigSlice } from '../../slices/globalConfigSlice';
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
        resumeVoting: builder.mutation<void, void, void>({
          query: () => ({url: '/voting/admin/resume', method: 'POST'}),
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
        createUser: builder.mutation<void, {username: string, gender: string, displayName: string }, void>({
          query: ({ username, gender, displayName }) => ({url: 'user', method: 'POST', body: { id: '', displayName, gender, username }}),
           async onQueryStarted(params, { dispatch, queryFulfilled }) {
            await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, true, true);
          },
          invalidatesTags: ['UsersList']
        })
    })
});

export const { useStartVotingMutation, useEndVotingMutation, useGetAllUsersQuery, useCreateUserMutation, useResumeVotingMutation } = adminApi;