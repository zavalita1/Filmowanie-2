export type ReadonlyMovie = {
    movieId: string;
    movieName: string;
    posterUrl: string;
    description: string;
    filmwebUrl: string;
    createdYear: number;
    duration: string;
    genres: string[];
    actors: string[];
    directors: string[];
    writers: string[];
    originalTitle: string;
}

export type VoteableMovie = ReadonlyMovie & {
    votes: number;
}

export type PlaceholderMovie = {
    title: string;
    decade: number;
}

export type Movie = VoteableMovie | PlaceholderMovie;