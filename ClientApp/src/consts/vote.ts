export enum Vote {
    ThreePoints = 3,
    TwoPoints = 2,
    OnePoint = 1,
    Trash = -1
}

export const allVoteTypes = [
    Vote.ThreePoints,
    Vote.TwoPoints,
    Vote.OnePoint,
    Vote.Trash
]

export function fromNumber(vote: number) {
    switch (vote) {
        case -1:
            return Vote.Trash;
        case 1:
            return Vote.OnePoint;
        case 2:
            return Vote.TwoPoints;
        case 3:
            return Vote.ThreePoints;
        default:
            throw new Error("Unknown value: "+ vote);
    }
}

export function toNumber(vote: Vote) {
    switch (vote) {
        case Vote.ThreePoints:
            return 3;
        case Vote.TwoPoints:
            return 2;
        case Vote.OnePoint:
            return 1;
        case Vote.Trash:
            return -1;
    }
}