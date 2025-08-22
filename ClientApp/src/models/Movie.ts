export type ConcreteMovie = {
    movieId: string;
    movieName: string;
    votes: number;
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

export type PlaceholderMovie = {
    title: string;
    decade: number;
}

export type Movie = ConcreteMovie | PlaceholderMovie;