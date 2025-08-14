import React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../../store'; 
import { UserState } from '../../../store/User';
import { getPreviousVotingLists } from "../../../repositories/historyRepository";
import { HistoryList } from '../../Lists/HistoryList';
import { HistoryDTO } from '../../../DTO/Incoming/HistoryDTO';
import SubPageWrapper from '../../SubPageWrapper';
import * as App from '../../../store/App';
import { VotingSessionDTO, VotingSessionsDTO } from '../../../DTO/Incoming/VotingSessionsDTO';

import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import Results from '../Results';
import { getVotingResult } from '../../../repositories/votesRepository';
import { VotingResultDTO } from '../../../DTO/Incoming/VotingResultDTO';

const HistoryWrapper = (props: PreviousVotingSessionsProps) => {
    const subWrapperProps = {
        initializeChildren: () => getPreviousVotingLists().then(data => {
            props.setLoading(false);
            return data;
        }),
        child: PreviousVotingSessionsSubPage,
        childProps: props
    };

    return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
}

type PreviousVotingSessionsProps = UserState & {
    isLoading? :boolean,
    votingSessions: VotingSessionDTO[]
} & typeof App.actionCreators

function PreviousVotingSessionsSubPage(props: PreviousVotingSessionsProps) {
    const [endedYear, setEndedYear] = React.useState<number | undefined>();
    const [endedMonth, setEndedMonth] = React.useState<number | undefined>();
    const [endedDate, setEndedDate] = React.useState<string | undefined>();
    const [selectedVotesId, setSelectedVotesId] = React.useState<string | undefined>(undefined);
    const [votes, setVotes] = React.useState<VotingResultDTO | undefined>(undefined);

    const allDatesToChoseFrom = props.votingSessions.map(x => ({Date: new Date(x.endedUnlocalized), DTO: x}));
    const yearsToChoseFrom = Array.from(new Set(allDatesToChoseFrom.map(x => x.Date.getFullYear())));
    const monthsToChoseFrom = Array.from(new Set(allDatesToChoseFrom.filter(x => x.Date.getFullYear() === endedYear).map(x => x.Date.getMonth())));
    const daysToChoseFrom = allDatesToChoseFrom.filter(x => x.Date.getFullYear() === endedYear && x.Date.getMonth() == endedMonth);

    return (
        <>
        <FormControl className='select'>
        <InputLabel id="demo-simple-select-label-2">Głosowanie z roku</InputLabel>
        <Select
          labelId="demo-simple-select-label-2"
          id="demo-simple-select"
          value={endedYear}
          onChange={e => setEndedYear(e.target.value as number)}
          label="Głosowanie z roku"
          className='previous-voting-select'
        >
          {yearsToChoseFrom.map(x => <MenuItem value={x}>{x}</MenuItem>)}
        </Select>
        </FormControl>
        <FormControl className='select'>
        <InputLabel id="demo-simple-select-label-1">Głosowanie z miesiąca</InputLabel>
        <Select
          labelId="demo-simple-select-label-1"
          id="demo-simple-select"
          value={endedMonth}
          onChange={e => setEndedMonth(e.target.value as number)}
          label="Głosowanie z miesiąca"
          className='previous-voting-select'
          disabled={endedYear === null}
        >
          {monthsToChoseFrom.map(x => <MenuItem value={x}>{x+1}</MenuItem>)}
        </Select>
        </FormControl>
        <FormControl className='select'>
        <InputLabel id="demo-simple-select-label">Głosowanie z dnia</InputLabel>
        <Select
          labelId="demo-simple-select-label"
          id="demo-simple-select"
          value={endedDate}
          onChange={handleChange}
          label="Głosowanie z dnia"
          className='previous-voting-select'
          disabled={endedMonth === null}
        >
          {daysToChoseFrom.map(x => <MenuItem value={x.DTO.ended}>{x.DTO.ended}</MenuItem>)}
        </Select>
      </FormControl>
        {
                selectedVotesId === undefined
            ? <></> 
            : <Results votes={votes} shouldAlwaysRender={true}></Results>
        }
        </>
    );

    async function handleChange(event: SelectChangeEvent) {
        const endedDate = event.target.value;
        const votingSession = props.votingSessions.find(x => x.ended === endedDate);

        const votes = await getVotingResult(votingSession!.id);
        props.setLoading(false);
        setVotes(votes);
        setSelectedVotesId(votingSession!.id);
        setEndedDate(endedDate);
      };
}

export default connect(
    (state: ApplicationState) => ({  ...state.user, isLoading: state.app?.isLoading }),
    App.actionCreators
)(HistoryWrapper as any);
