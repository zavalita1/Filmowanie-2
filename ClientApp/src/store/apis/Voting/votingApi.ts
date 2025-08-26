import { userApi } from '../User/userApi';
import { commonOnQueryStarted } from '../../utils/queryStoreWrapper';
import { GlobalConfigSlice, globalConfigSlice } from '../../globalConfigSlice';

import { CurrentVotingIncomingDTO, MovieDTO, VoteOutgoingDTO, VotingSessionStatusIncomingDTO } from './types';
import { VotingStatus } from '../../../consts/votingStatus';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';
import { ConcreteMovie, Movie, PlaceholderMovie } from '../../../models/Movie';

export const votingApi = userApi
.enhanceEndpoints({ addTagTypes: ['MoviesList']})
.injectEndpoints({
  endpoints: (builder) => ({
    getCurrentVoting: builder.query<Movie[], void>({
      async queryFn(arg, queryApi, extraOptions, fetchWithBQ) {
        const { getState } = queryApi;
        const state: GlobalConfigSlice = (getState() as any).global;
        const moviesListUrl = `${state.apiUrl}voting/current`;
        const resultWrapped = await fetchWithBQ(moviesListUrl);
        const isError = !resultWrapped.data;
        const result = { data: resultWrapped.data as CurrentVotingIncomingDTO };

        return !isError
          ? { data: result.data.sort((x,y) => y.createdYear - x.createdYear).map(dto => mapMovie(dto)) }
          : { error: resultWrapped.error as FetchBaseQueryError }
      },
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      providesTags: ['MoviesList'],

    }),
    getState: builder.query<VotingStatus, void>({
      query: () => 'voting/state',
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      transformResponse: mapVotingStatus
    }),
    vote: builder.mutation<void, VoteOutgoingDTO, void>({
      query: dto => ({ url: '/voting/vote', method: 'POST', body: dto}),
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
    })
  })
});

function mapVotingStatus(dto: VotingSessionStatusIncomingDTO) {
  switch (dto.status) {
    case "Loading": return VotingStatus.Loading;
    case "Results": return VotingStatus.Results;
    case "Voting": return VotingStatus.Voting;
    case "VotingEnding": return VotingStatus.VotingEnding;
    case "VotingStarting": return VotingStatus.VotingStarting;
    default: throw new Error("Unknown status!")
  }
}

function mapMovie(dto: MovieDTO): Movie {
  if (dto.isPlaceholder)
    return ({ title: dto.movieName, decade: dto.createdYear });

  return dto;
}

export const { useGetCurrentVotingQuery, useGetStateQuery, useVoteMutation } = votingApi;
