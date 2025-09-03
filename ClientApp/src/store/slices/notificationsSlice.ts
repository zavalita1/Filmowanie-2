import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface NotificationsState {
    notificationsToDisplay: string[]
}

const initialState: NotificationsState = {
    notificationsToDisplay: []
};

export const notificationsSlice = createSlice({
    name: 'notifications',
    initialState,
    reducers: {
        addNotification: (state, action: PayloadAction<string>) => {
            return {...state, notificationsToDisplay: [...state.notificationsToDisplay, action.payload]};
        },
        displayNotification: (state, action:PayloadAction<string>) => {
            const foundNotificationToRemove = state.notificationsToDisplay.findIndex(x => x === action.payload);
            return {...state, notificationsToDisplay: [...state.notificationsToDisplay.splice(foundNotificationToRemove, 1)]}
        }
    }
});