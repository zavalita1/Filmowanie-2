import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import { VotingState } from '../../store/votingState';
import { UserState, actionCreators } from '../../store/User';
import { getVotingResult, getPreviousVotingResult } from "../../repositories/votesRepository";
import { VotingResultDTO } from '../../DTO/Incoming/VotingResultDTO';
import { VotingList } from '../Lists/VotingList';
import TrashVotingList from '../Lists/TrashVotingList';
import { Typography } from '@mui/material';
import SubPageWrapper from '../SubPageWrapper';
import * as App from '../../store/App';

type ResultProps = UserState & {
    state?: VotingState
    isLoading? :boolean,
    votes?: VotingResultDTO,
    shouldAlwaysRender?: boolean
} & typeof App.actionCreators

const ResultsWrapper = (props: ResultProps) => {
    const subWrapperProps = {
        initializeChildren: props.votes !== undefined ? undefined : () => 
            getVotingResult().then(data => {
                props.setLoading(false);
                return { votes: data };
        }),
        child: Results,
        childProps: props
    };

    return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
}

function Results(props: ResultProps): any {
        if(!props.shouldAlwaysRender && props.user?.isAdmin !== true && props.state !== VotingState.Results) {
            return <div>Głosowanie trwa...</div>
        }

        return (
            <React.Fragment>
                <div>
                {
                    renderResults(props.votes!)
                }
                </div>
            </React.Fragment>
        );
};

function renderResults(votes: VotingResultDTO) {

    return (
        <>
        <div>
            <VotingList {...votes}></VotingList>
        </div>
        <br/>
        <Typography variant='h5' sx={{"margin-top": "1em"}}> Głosowanie na śmiecie: </Typography>
        <div>
            <TrashVotingList {...votes}></TrashVotingList>
        </div>
        </>
    )
}

export default connect(
    (state: ApplicationState, ownProps: { votes?: VotingResultDTO, shouldAlwaysRender?: boolean }) => ({  
            ...state.user, 
            state: state.state?.state, 
            isLoading: state.app?.isLoading,
            votes: ownProps.votes,
            shouldAlwaysRender: ownProps.shouldAlwaysRender
        }),
    App.actionCreators
)(ResultsWrapper as any);
