import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { combineReducers } from 'redux';

import { globalConfigSlice } from './slices/globalConfigSlice';
import { notificationsSlice } from './slices/notificationsSlice';
import { votingSlice } from './slices/votingSlice';
import { historyApi as api } from './apis/5-History/api';

export const store = configureStore({
  devTools: process.env.NODE_ENV === 'development',
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat([api.middleware]),
  reducer: combineReducers({
    global: globalConfigSlice.reducer,
    notification: notificationsSlice.reducer,
    voting: votingSlice.reducer,
    [api.reducerPath]: api.reducer,
  }),
})

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
// Inferred type: {posts: PostsState, comments: CommentsState, users: UsersState}
export type AppDispatch = typeof store.dispatch

setupListeners(store.dispatch)
