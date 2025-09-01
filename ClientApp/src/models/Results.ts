export type ResultRow = {
    rank: number;
    movieTitle: string;
    votesCount: number;
    isDecorated: boolean;
    voters?: string[];
}

export type Results = {
    voting: ResultRow[];
    trashVoting: ResultRow[];
}

/**
 * 
 * @property concluded - date as isoString 
 */
export type ResultsMetadata = {
    id: string;
    concluded: string;
    concludedLocalized: string;
}