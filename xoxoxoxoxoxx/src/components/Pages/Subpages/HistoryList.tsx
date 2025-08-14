import React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../../store'; 
import { UserState } from '../../../store/User';
import { getHistory } from "../../../repositories/historyRepository";
import { HistoryList } from '../../Lists/HistoryList';
import { HistoryDTO } from '../../../DTO/Incoming/HistoryDTO';
import SubPageWrapper from '../../SubPageWrapper';
import * as App from '../../../store/App';

const HistoryWrapper = (props: HistoryProps) => {
    const subWrapperProps = {
        initializeChildren: () => getHistory().then(data => {
            props.setLoading(false);
            return data;
        }),
        child: HistoryListSubPage,
        childProps: props
    };

    return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
}

type HistoryProps = UserState & {
    isLoading? :boolean,
    history?: HistoryDTO
} & typeof App.actionCreators

function HistoryListSubPage(votes: HistoryDTO) {
    return (<HistoryList {...votes}></HistoryList>);
}

export default connect(
    (state: ApplicationState) => ({  ...state.user, isLoading: state.app?.isLoading }),
    App.actionCreators
)(HistoryWrapper);