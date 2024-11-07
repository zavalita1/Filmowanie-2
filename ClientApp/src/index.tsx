import 'bootstrap/dist/css/bootstrap.css';
import './components/css/MovieCard.css';
import './components/css/Nominate.css';
import './components/css/General.css';

// import registerServiceWorker from './initialization/registerServiceWorker';
import * as Init from "./initialization/initialize";
// import * as SetupSignalRConnection from './initialization/SetupSignalRConnection';

const { store } = Init.InitStore();
Init.RenderReactDOM(store);

Init.InitActions(store).then(() => {
    // SetupSignalRConnection.SetupSignalRConnection(store);
});


//registerServiceWorker();
