export type HistoryStandingsDTO = {
    [key: number]: HistoryStandingsRowDTO
}

export type HistoryStandingsRowDTO = {
    movieTitle: string,
    votingPlaces: number[]
}
