export type VotingSessionsDTO = {
    votingSessions: VotingSessionDTO[];
};

export type VotingSessionDTO = {
    id: string,
    ended: string
    endedUnlocalized: string
}
