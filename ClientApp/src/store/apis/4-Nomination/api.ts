import { commonOnQueryStarted } from "../../utils/queryStoreWrapper";
import { GlobalConfigSlice, globalConfigSlice } from '../../globalConfigSlice';
import { adminApi } from "../3-Admin/api";
import { MoviesThatCanBeNominatedAgainIncomingDTO, NominationOutgoingDTO, NominationsDataIncomingDTO, PostersIncomingDTO } from "./types";
import { Decade } from "../../../consts/Decade";
import { Movie } from "../../../models/Movie";

export const nominationApi = adminApi
.enhanceEndpoints({addTagTypes: ['Nominations', 'MoviesToBeNominatedAgain']})
.injectEndpoints({
    endpoints: builder => ({
        getNominations: builder.query<Decade[], void>({
            query: () => ({ url: 'nominations', method: 'GET'}),
            providesTags: ['Nominations'],
            transformResponse: (dto: NominationsDataIncomingDTO) => dto.nominations.map(mapDecade)
        }),
        getMoviesThatCanBeNominatedAgain: builder.query<Movie[], void>({
            query: () => ({ url: 'nominations/fullData', method: 'GET' }),
            providesTags: ['MoviesToBeNominatedAgain'],
            async onQueryStarted(params, { dispatch, queryFulfilled }) {
                await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, false, true);
            },
            transformResponse: (dto: MoviesThatCanBeNominatedAgainIncomingDTO) => dto.moviesThatCanBeNominatedAgain.map(mapMovieThatCanBeNominatedAgain)
        }),
        nominate: builder.mutation<void, NominationOutgoingDTO, void>({
            query: dto => ({url: 'nominations', method: 'POST', body: dto}),
            async onQueryStarted(params, { dispatch, queryFulfilled }) {
                await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, false, true);
            },
        }),
        getPosters: builder.query<string[], string>({
            query: movieUrl => ({url: 'nominations/posters', method: 'GET', params: { movieUrl } }),
            async onQueryStarted(params, { dispatch, queryFulfilled }) {
                await commonOnQueryStarted(isLoading => dispatch(globalConfigSlice.actions.setLoading(isLoading)), queryFulfilled, true, false, true);
            },
            transformResponse: (dto: PostersIncomingDTO) => dto.posterUrls
        })
    })
});

function mapMovieThatCanBeNominatedAgain(x: any): Movie {
    return ({
            movieId: x.movieId,
            description: x.description,
            posterUrl: x.posterUrl,
            movieName: x.movieName,
            filmwebUrl: x.filmwebUrl,
            createdYear: x.createdYear,
            duration: x.duration,
            genres: x.genres,
            actors: x.actors.slice(0, 3),
            directors: x.directors,
            writers: x.writers,
            originalTitle: x.originalTitle
    });
}

function mapDecade(decadeStr: string) {
    switch(decadeStr) {
        case "2020s":
            return Decade["2020s"];
        case "2010s":
            return Decade["2010s"];
        case "2000s":
            return Decade["2000s"];
        case "1990s":
            return Decade["1990s"];
        case "1980s":
            return Decade["1980s"];
        case "1970s":
            return Decade["1970s"];
        case "1960s":
            return Decade["1960s"];
        case "1950s":
            return Decade["1950s"];
        case "1940s":
            return Decade["1940s"];
    }

    throw new Error("Cannot parse provided argument: " + decadeStr);
}

export const { useGetNominationsQuery, useGetMoviesThatCanBeNominatedAgainQuery, useNominateMutation, useGetPostersQuery } = nominationApi;