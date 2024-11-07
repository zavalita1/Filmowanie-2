export interface IMovie {
    title: string, 
    posterUrl: string, 
    userCurrentVotes: number,
    description: string,
    filmwebUrl: string,

    isPlaceholder: boolean,
    createdYear: number,
    duration: string,
    genres: string[],
    actors: string[],
    directors: string[],
    writers: string[],
    originalTitle: string
}

export interface MoviesListState {
    movies: IMovie[],
    isStale: boolean,
    allVotesAbsSum: number
}

export type VoteDTO = {
    movieTitle: string;
    votes: Number
}

