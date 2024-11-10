import * as signalR from "@microsoft/signalr";
import { Store } from 'redux';
import { ApplicationState } from "./store";


export function SetupSignalRConnection(store: Store<ApplicationState>) {
    const connection = new signalR.HubConnectionBuilder().withUrl("/votesHub").withAutomaticReconnect().build();
    connection.on("ReceiveMessage", function (user, message) {
        alert(`${user} + ${message}`);
    });

    connection.on("voting started", () => {
        store.dispatch({ type: 'VOTING_STARTING' });
        store.dispatch({ type: 'RELOAD_MOVIE_LIST' });
    });

    connection.on("voting ended", () => {
        store.dispatch({ type: 'VOTING_ENDING' });
    });

    connection.on("you can nominate", () => {
        debugger;
    });

    connection.on("movie nominated", (user: string) => {
        const gender = user.endsWith('a');
        const nominated = gender ? 'nominowała' : 'nominował'
        store.dispatch({type: 'RELOAD_MOVIE_LIST', payload: { infoMessage: `${user} właśnie ${nominated} film.`}});
    });

    connection.on("voted", (user: string) => {
        const state  = store.getState();
        if (state.user?.user?.username !== user) {
            const gender = user.endsWith('a');
            const nominated = gender ? 'zagłosowała' : 'zagłosował'
            store.dispatch({type: 'NOTIFICATION_OCCURRED', payload: `${user} właśnie ${nominated} na jeden z filmów.`});
        }
    });

    connection.start();

    // .then(() => { // TODO
    //     connection.invoke("SendMessage", "useeeeee", "loo").catch(function (err) {
    //         return console.error(err.toString());
    //     });
    // }).catch(() => { // TODO
    // });
}
