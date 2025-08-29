import { userApi } from '../1-User/userApi';
import { commonOnQueryStarted } from '../../utils/queryStoreWrapper';
import { GlobalConfigSlice, globalConfigSlice } from '../../globalConfigSlice';

import { CurrentVotingIncomingDTO, MovieDTO, VoteOutgoingDTO, VotingResultIncomingDTO, VotingSessionStatusIncomingDTO } from './types';
import { VotingStatus } from '../../../consts/votingStatus';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';
import { ConcreteMovie, Movie, PlaceholderMovie } from '../../../models/Movie';
import { ResultRow, Results } from '../../../models/Results';

export const votingApi = userApi
.enhanceEndpoints({ addTagTypes: ['MoviesList', 'VotingStatus']})
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
      transformResponse: mapVotingStatus,
      providesTags: ['VotingStatus']
    }),
    vote: builder.mutation<void, VoteOutgoingDTO, void>({
      query: dto => ({ url: '/voting/vote', method: 'POST', body: dto}),
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
         const patchResult = dispatch(
          votingApi.util.updateQueryData('getCurrentVoting', undefined, r => {
            const movieToPatchIndex = r.findIndex(m => (m as ConcreteMovie)?.movieId === params.movieId);
            const movieToPatch = {...r[movieToPatchIndex], votes: params.votes};
            return [...r.slice(0, movieToPatchIndex), movieToPatch,...r.slice(movieToPatchIndex + 1)];
          }));
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, false, false, async () => patchResult.undo());
      },
    }),
    getResults: builder.query<Results, string>({
      query: votingSessionId => ({ url: `voting/results?votingSessionId=${votingSessionId}`, method: 'GET'}),
      async onQueryStarted(params, { dispatch, queryFulfilled }) {
        await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true);
      },
      transformResponse: mapVotingResults
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

function mapVotingResults(dto: VotingResultIncomingDTO): Results {
  let rank = 0;
  const votingResult = dto.votingResults.map(x => ({ rank: ++rank, movieTitle: x.movieName, isDecorated: x.isWinner ?? false, votesCount: x.votersCount } satisfies ResultRow));
  rank = 0;
  const trashVotingResult = dto.trashVotingResults.filter(x => x.voters.length !== 0).map(x => ({ rank: ++rank, movieTitle: x.movieName, isDecorated: x.isAwarded ?? false, votesCount: x.voters.length, voters: x.voters} satisfies ResultRow));
  return { voting: votingResult, trashVoting: trashVotingResult }; 
}

export const { useGetCurrentVotingQuery, useGetStateQuery, useVoteMutation, useGetResultsQuery } = votingApi;
