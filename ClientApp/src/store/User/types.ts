export interface IUser {
    username: string;
    isAdmin: boolean;
    hasNominations: boolean;
    hasBasicAuthSetup: boolean;
}

export interface UserState {
    user?: IUser
    isStale?: boolean;
}

interface BaseAction { payload?: any }
export interface LoggingAction extends BaseAction { type: 'LOGGING' }
export interface LoggedAction extends BaseAction { type: 'LOGGED' }
export interface UnloggedAction extends BaseAction { type: 'UNLOGGED' }
export interface UnloggingAction extends BaseAction { type: 'UNLOGGING' }

export type KnownAction = LoggingAction | LoggedAction | UnloggedAction | UnloggingAction;
