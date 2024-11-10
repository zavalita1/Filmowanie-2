import * as React from 'react';
import HistoryList from './Subpages/HistoryList';
import LastStandings from './Subpages/LastStandings';
import PreviousVotingSessions from './Subpages/PreviousVotingSessions';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';

type PossibleTab = 'List' | 'LastStandings' | 'LastVote';

export default function History() {

    const [value, setValue] = React.useState<PossibleTab>('List');

    const handleChange = (event: React.SyntheticEvent, newValue: PossibleTab) => {
        setValue(newValue);
    };

    return (
        <>
            <div className='tabs'>
                <Box sx={{ width: '100%' }}>
                    <Tabs
                        value={value}
                        onChange={handleChange}
                        textColor="secondary"
                        indicatorColor="secondary"
                        aria-label="secondary tabs example"
                    >
                        <Tab value="LastVote" label="Wyniki ostatnich głosowań" />
                        <Tab value="List" label="Lista Obejrzanych Filmów" />
                        <Tab value="LastStandings" label="Wykresy" />
                    </Tabs>
                </Box>
            </div>
            <>
                {
                    getTab(value)
                }
            </>
        </>
    );

};

function getTab(tabName: PossibleTab) {
    if (tabName === "List") {
        return (
            <HistoryList></HistoryList>
        );
    }

    if (tabName === 'LastStandings') {
        return (
            <LastStandings></LastStandings>
        )
    }

    if (tabName === 'LastVote') {
        return (
            <PreviousVotingSessions></PreviousVotingSessions>
        )       
    }

    return <></>
}