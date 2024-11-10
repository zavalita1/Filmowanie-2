import React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../../store'; 
import { UserState } from '../../../store/User';
import { getHistoryStandings } from "../../../repositories/historyRepository";
import { HistoryList } from '../../Lists/HistoryList';
import { HistoryStandingsDTO, HistoryStandingsRowDTO } from '../../../DTO/Incoming/HistoryStandingsDTO';
import SubPageWrapper from '../../SubPageWrapper';
import * as App from '../../../store/App';
import { Chart } from 'react-chartjs-2';
import { ChartDataset, ChartTypeRegistry, Legend, LineController, LineElement, Point, PointElement } from 'chart.js';
import {CategoryScale, LinearScale, Chart as ChartJS} from 'chart.js'; 

ChartJS.register(CategoryScale);
ChartJS.register(LinearScale);
ChartJS.register(PointElement);
ChartJS.register(LineElement);
ChartJS.register(LineController);
ChartJS.register(Legend);

const HistoryWrapper = (props: HistoryProps) => {
    const subWrapperProps = {
        initializeChildren: () => getHistoryStandings().then(data => {
            props.setLoading(false);
            return {data};
        }),
        child: HistoryListSubPage,
        childProps: props
    };

    return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
}

type HistoryProps = UserState & {
    isLoading? :boolean,
    history?: HistoryStandingsDTO
} & typeof App.actionCreators

function HistoryListSubPage(props: { data: HistoryStandingsDTO}) {
    const seriesData: ChartDataset<any, any>[] = [];
    Object.entries(props.data).forEach(x => populateSeriesData(x[1], seriesData));

    const seriesLenght = seriesData[0].data.length;
    function getXAxisLabel(x: number): any {
        if (x === 1) {
            return `1 głosowanie temu`;
        }
        else if (x < 5) {
            return `${x} głosowania temu`;
        }
        else {
            return `${x} głosowań temu`;
        }
    }

    return ( 
    <Chart
        type='line'
        data={{ 
              labels: [...toTen.slice(-seriesLenght).map(x => getXAxisLabel(x))], 
              datasets: seriesData,
              }}
        options={{
            scales: {
                y: {
                    ticks: {
                        callback: (tickValue, index, ticks) => index == 9 ? "" : `${9-index}. miejsce`
                    },
                    min: 0,
                    max: 9
                }
            },
            plugins: {
                legend: {
                    display: true
                }
            }
        }}
      />);
}

export default connect(
    (state: ApplicationState) => ({  ...state.user, isLoading: state.app?.isLoading }),
    App.actionCreators
)(HistoryWrapper);

function populateSeriesData(historyStandingsRow: HistoryStandingsRowDTO, seriesData: ChartDataset<any, any>[]) {
    seriesData.push({
        data: historyStandingsRow.votingPlaces.map(x => x === null ? null : 9-x).reverse(), 
        label: historyStandingsRow.movieTitle,
        fill: true,
        borderColor: colors[seriesData.length % (colors.length - 1)]
    });
}

const toTen = [1,2,3,4,5,6,7,8,9].reverse();

const colors = [
 '#003f5c',
 '#2f4b7c',
 '#665191',
 '#a05195',
 '#d45087',
 '#f95d6a',
 '#ff7c43',
 '#ffa600',
'#00876c',
'#439981',
'#6aaa96',
'#8cbcac',
'#aecdc2',
'#cfdfd9',
'#f1f1f1',
'#f1d4d4',
'#f0b8b8',
'#ec9c9d',
'#e67f83',
'#de6069',
'#d43d51'
];