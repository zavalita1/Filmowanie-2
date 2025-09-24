import { userApi } from '../1-User/userApi';
import { commonOnQueryStarted } from '../../utils/queryStoreWrapper';
import { GlobalConfigSlice, globalConfigSlice } from '../../slices/globalConfigSlice';

import { CurrentVotingIncomingDTO, MovieDTO, VoteOutgoingDTO, VotingResultIncomingDTO, VotingSessionStatusIncomingDTO } from './types';
import { VotingStatus } from '../../../consts/votingStatus';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';
import { VoteableMovie, VotableOrPlaceholderMovie, PlaceholderMovie } from '../../../models/Movie';
import { ResultRow, Results } from '../../../models/Results';
import { mapVotingResults } from '../../../mappers/mapVotingResults';

export const votingApi = userApi
.enhanceEndpoints({ addTagTypes: ['MoviesList', 'VotingStatus', 'Results'], endpoints: { logout: { invalidatesTags: ['MoviesList', 'UserData']}}})
.injectEndpoints({
  endpoints: (builder) => ({
    getCurrentVoting: builder.query<VotableOrPlaceholderMovie[], void>({
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
      transformResponse: mapVotingStatus,
      providesTags: ['VotingStatus'],
       async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
    }),
    vote: builder.mutation<void, VoteOutgoingDTO, void>({
      query: dto => ({ url: '/voting/vote', method: 'POST', body: dto}),
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
         const patchResult = dispatch(
          votingApi.util.updateQueryData('getCurrentVoting', undefined, r => {
            const movieToPatchIndex = r.findIndex(m => (m as VoteableMovie)?.movieId === params.movieId);
            const movieToPatch = {...r[movieToPatchIndex], votes: params.votes};
            return [...r.slice(0, movieToPatchIndex), movieToPatch,...r.slice(movieToPatchIndex + 1)];
          }));
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, params?.votes === 0 ? "Głos usunięty" : "Zagłosowane!", true, async () => patchResult.undo());
      },
      invalidatesTags: ['Results'],
    }),
    getResults: builder.query<Results, string>({
      query: votingSessionId => ({ url: `voting/results?votingSessionId=${votingSessionId}`, method: 'GET'}),
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      transformResponse: mapVotingResults,
      providesTags: ['Results']
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

function mapMovie(dto: MovieDTO): VotableOrPlaceholderMovie {
  if (dto.isPlaceholder)
    return ({ title: dto.movieName, decade: dto.createdYear, bigPosterUrl: dto.bigPosterUrl ?? "https://fwcdn.pl/fpo/00/33/120033/7606010_1.8.webp" });

  return dto;
}



export const { useGetCurrentVotingQuery, useLazyGetStateQuery, useVoteMutation, useGetResultsQuery } = votingApi;
