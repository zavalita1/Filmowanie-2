import { configureStore } from '@reduxjs/toolkit'
import { reducers } from './';

export default function initStore() {
    const store = configureStore({
        reducer: {...reducers}
    });
    return {store};
}