import { Decade } from "../../../consts/Decade";

export type UserIncomingDTO = {
    username: string;
    isAdmin: boolean;
    hasRegisteredBasicAuth: boolean;
};

export type UserState = {
    username: string;
    isAdmin: boolean;
    hasRegisteredBasicAuth: boolean;
    useFemaleSuffixes: boolean;
};

export type UserStateWithNominations = UserState & {
    nominations: Decade[];
}

export type LoginWithCodeOutgoingDTO = {
    code: string;
}

export type LoginWithBasicAuthOutgoingDTO = {
    email: string;
    password: string;
}

export type LoginWithGoogleOutgoingDTO = {
    code: string;
    scope: string;
}