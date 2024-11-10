import 'bootstrap/dist/css/bootstrap.css';
import './components/css/MovieCard.css';
import './components/css/Nominate.css';
import './components/css/General.css';

import registerServiceWorker from './registerServiceWorker';
import * as Init from "./initialize";
import * as SetupSignalRConnection from './SetupSignalRConnection';

const {history, store} = Init.InitStore();
Init.RenderReactDOM(store, history);

Init.InitActions(store).then(() => {
    SetupSignalRConnection.SetupSignalRConnection(store);
});


//registerServiceWorker();
