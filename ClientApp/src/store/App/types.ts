export interface AppState {
    isLoading: boolean;
    isError: boolean;
    errorMessage?: string;
    infoMessage?: string;
    theme: SupportedTheme;
}

export type SupportedTheme = 'light' | 'dark';

export interface LoadingAction { type: 'LOADING' };
export interface LoadedAction { type: 'LOADED' };
export interface ErrorOccurredAction { type: 'ERROR_OCCURRED', payload: string };
export interface NotificationOccurredAction { type: 'NOTIFICATION_OCCURRED', payload: string };
export interface ErrorAwknowledged { type: 'ERROR_AKNOWLEDGED' };
export interface InfoAwknowledged { type: 'INFO_AKNOWLEDGED' };
export interface SetThemeAction {type: 'SET_THEME', payload: SupportedTheme};

export type KnownAction = LoadingAction | LoadedAction | ErrorOccurredAction | NotificationOccurredAction | ErrorAwknowledged | InfoAwknowledged | SetThemeAction;
