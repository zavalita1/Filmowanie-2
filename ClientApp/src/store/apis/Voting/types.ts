export type MovieDTO = {
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
    isPlaceholder: boolean;
};

export type VotingSessionStatusIncomingDTO = {
    status: string;
}

export type CurrentVotingIncomingDTO = Array<MovieDTO>;

export type VoteOutgoingDTO = { 
    movieTitle: string;
    votes: number;
}