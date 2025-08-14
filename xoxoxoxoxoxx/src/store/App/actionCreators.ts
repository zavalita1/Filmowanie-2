import { LoadedAction, LoadingAction, ErrorAwknowledged, ErrorOccurredAction, SetThemeAction, SupportedTheme, InfoAwknowledged } from './types';

function setLoading(value: boolean) {
    if (value) {
        return ({type: 'LOADING'} as LoadingAction);
    }

    return ({type: 'LOADED'} as LoadedAction);
}

function aknowledgeError() {
    return ({type: 'ERROR_AKNOWLEDGED'} as ErrorAwknowledged);
}

function aknowledgeInfo() {
    return ({type: 'INFO_AKNOWLEDGED'} as InfoAwknowledged);
}

function setError(errorMessage: string) {
    return ({type: 'ERROR_OCCURRED', payload: errorMessage} as ErrorOccurredAction)
}

function setTheme(theme: SupportedTheme) {
    return ({type: 'SET_THEME', payload: theme} as SetThemeAction)
}

export const actionCreators = {
    setLoading,
    aknowledgeError,
    setError,
    setTheme,
    aknowledgeInfo
};
