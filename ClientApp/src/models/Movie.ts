export type WatchedMovie = {
    title: string;
    originalTitle: string;
    watched: string;
    nominatedBy: string;
    filmwebUrl: string;
}

export type Movie = {
    movieId: string;
    movieName: string;
    posterUrl: string;
    bigPosterUrl: string;
    description: string;
    altDescription?: string;
    filmwebUrl: string;
    createdYear: number;
    duration: string;
    genres: string[];
    actors: string[];
    directors: string[];
    writers: string[];
    originalTitle: string;
};

export type VoteableMovie = Movie & {
    votes: number;
}

export type PlaceholderMovie = {
    title: string;
    decade: number;
}

export type VotableOrPlaceholderMovie = VoteableMovie | PlaceholderMovie;