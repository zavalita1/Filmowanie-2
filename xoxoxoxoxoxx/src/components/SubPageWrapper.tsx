import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../store';
import { VotingState } from '../store/votingState';
import Loader from './Loader';
import { UserState } from '../store/User';
import VotingStateDialog from './Dialogs/VotingStateDialog';
import Error from "./NotificationPopups/Error";
import Info from "./NotificationPopups/Info";
import { actionCreators }  from '../store/App/actionCreators';
import { AppState }  from '../store/App/types';

type SupPageWrapperProps<T> = { votingState: VotingState; app: AppState, child: React.ComponentType<T>, childProps: {}, initializeChildren?: () => Promise<T>} 
& UserState
& AppState
& typeof actionCreators

function SubPageWrapper<T extends JSX.IntrinsicAttributes>(props: SupPageWrapperProps<T>) {
    const [childData, setChildData] = React.useState({} as T);
    const [isInitialLoad, setIsInitialLoad] = React.useState(props.app.isLoading !== false);
    const [isInitializing, setIsInitializing] = React.useState(props.initializeChildren !== undefined);

    React.useEffect(() => {
        if (props.initializeChildren !== undefined) {
            const initializeChildrenResult = props.initializeChildren();

            if (initializeChildrenResult !== undefined) {
                initializeChildrenResult.then(data => {
                    setChildData(data);
                    setIsInitializing(false);
                });
            } else {
                setIsInitializing(false);
            }
        }
    }, []);

    React.useEffect(() => {
        if (props.app.isLoading === false && isInitialLoad) {
            setIsInitialLoad(false);
        }
    }, [props.app.isLoading]);

    const Children = props.child;
    const childExtendedProps = {...props.childProps, ...childData};
    const shouldDisplayChild = !isInitializing && !isInitialLoad;

    return <div>
        <Loader isLoading={props.app.isLoading}></Loader>
        <VotingStateDialog></VotingStateDialog>
        {shouldDisplayChild? <Children { ...childExtendedProps} ></Children> : <></>}
        <Error showErrorToast={props.app?.isError } onAknowledgeError={props.aknowledgeError} errorMessage={props.app?.errorMessage} ></Error>
        <Info onAknowledge={props.aknowledgeInfo} message={props.app?.infoMessage} ></Info>
    </div>;
};

export default connect(
    (state: ApplicationState) => ({ ...state.user, votingState: state.state?.state, app: state.app }),
    { ...actionCreators }
)(SubPageWrapper as any);
