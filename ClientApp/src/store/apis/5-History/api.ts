import { commonOnQueryStarted } from "../../utils/queryStoreWrapper";
import {  globalConfigSlice } from '../../globalConfigSlice';
import { nominationApi } from "../4-Nomination/api";
import { ConcludedVotingsIncomingDTO, WatchedMoviesListIncomingDTO } from "./types";
import { WatchedMovie } from "../../../models/Movie";
import { Results, ResultsMetadata } from "../../../models/Results";
import { mapVotingResults } from "../../../mappers/mapVotingResults";

export const historyApi = nominationApi
.enhanceEndpoints({addTagTypes: ['WatchedMoviesList', 'VotingsList', 'Voting']})
.injectEndpoints({
    endpoints: builder => ({
        getWatchedMoviesList: builder.query<WatchedMovie[], void>({
            query: () => ({url: 'voting/results/winners', method: 'GET'}),
            async onQueryStarted(params, { dispatch, queryFulfilled }) {
                await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, false, true);
            },
            transformResponse: mapWinnersList,
            providesTags: ['WatchedMoviesList']
        }),
        getVotingsList: builder.query<ResultsMetadata[], void>({
            query: () => ({url: 'voting/results/list', method: 'GET'}),
            async onQueryStarted(params, { dispatch, queryFulfilled }) {
                await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, false, true);
            },
            transformResponse: mapVotingsList,
            providesTags: ['VotingsList']
        }),
        getVoting: builder.query<Results, string>({
            query: id => ({url: 'voting/results/', method: 'GET', params: { votingSessionId: id }}),
            async onQueryStarted(params, { dispatch, queryFulfilled }) {
                await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, false, true);
            },
            transformResponse: mapVotingResults,
            providesTags: ['Voting']
        })
    })
});

function mapWinnersList(dto: WatchedMoviesListIncomingDTO) {
    return dto.entries.map(x => ({
        nominatedBy: x.nominatedBy,
        watched: x.watched,
        title: x.title,
        originalTitle: x.originalTitle
    } satisfies WatchedMovie));
}

function mapVotingsList(dto: ConcludedVotingsIncomingDTO) {
    return dto.votingSessions.map(x => ({
        id: x.id,
        concluded: new Date(x.endedUnlocalized).toISOString(),
        concludedLocalized: x.ended
    } satisfies ResultsMetadata))
}

export const { useGetWatchedMoviesListQuery, useGetVotingsListQuery, useLazyGetVotingQuery } = historyApi;