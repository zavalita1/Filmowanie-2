export type UserIncomingDTO = {
    username: string;
    isAdmin: boolean;
    hasBasicAuthSetup: boolean;
};

export type UserState = {
    username: string;
    isAdmin: boolean;
    hasBasicAuthSetup: boolean;
};

export type LoginWithCodeOutgoingDTO = {
    code: string;
}

export type LoginWithBasicAuthOutgoingDTO = {
    email: string;
    password: string;
}