export type HistoryDTO = {
    entries: HistoryEntryDTO[];
}

export type HistoryEntryDTO = {
    title: string;
    originalTitle: string;
    createdYear: number;
    nominatedBy: string,
    watched: string
}
