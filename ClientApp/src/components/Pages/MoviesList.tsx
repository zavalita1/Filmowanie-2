import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import { ApplicationState } from '../../store';
import * as MoviesList from '../../store/MoviesList';
import List from '@mui/material/List';

import Box from '@mui/material/Box';
import MovieCard from '../MovieCard/MovieCard';
import { VotingState } from '../../store/votingState';
import SubPageWrapper from '../SubPageWrapper';
import MovieCardPlaceholder from '../MovieCard/MovieCardPlaceholder';
import Confetti from '../Confetti';
import VotingConfirmationDialog from '../Dialogs/VotingConfirmationDialog';
import { IMovie } from '../../store/MoviesList';
import { Button } from 'reactstrap';

type MoviesListProps = MoviesList.MoviesListState &
typeof MoviesList.actionCreators &
IMoviesListProps &
RouteComponentProps<{}>;

interface IMoviesListProps {
    votingState?: VotingState,
    isAdmin?: boolean 
}

const MoviesListWrapper = (props: MoviesListProps) => {
    const subWrapperProps = {
        initializeChildren: () => {
            if (props.isStale) {
                props.loadList();
            }
        },
        child: MoviesListComponent,
        childProps: props
    };

    return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
}

const MoviesListComponent = (props: MoviesListProps) => {
    const [isDialogClosed, setIsDialogClosed] = React.useState(false);
    const areAllVotesAssigned = props.movies?.length !== 0 
        ? props.movies.reduce((prev, current) => prev += Math.abs(current.userCurrentVotes), 0) === props.allVotesAbsSum 
        : false;

    if (!areAllVotesAssigned && isDialogClosed && !props.isStale) {
        setIsDialogClosed(false);
    }

    React.useEffect(() => {
        if (props.isStale) {
            props.loadList();
        }
    }, [props.isStale]);

    const openPopup = areAllVotesAssigned && !isDialogClosed;
    
    return (
        <div>
            {props.votingState === VotingState.Results ?
                <div> Głosowanie zakończone.</div> :
                <div>
                    <Box sx={{ width: '100%' }}>
                        <VotingConfirmationDialog isOpen={openPopup} onClose={() => setIsDialogClosed(true)}/>
                        <nav aria-label="secondary mailbox folders">
                            <List>
                                <Confetti isEnabled={openPopup} />
                                {renderMoviesList()}
                            </List>
                            <div className='break-big'></div>
                        </nav>
                    </Box>
                </div>
            }
        </div>
    );

function renderMoviesList() {
    return props.movies.map(movie => {
    if (movie.isPlaceholder) {
        return <MovieCardPlaceholder message={movie.title} ></MovieCardPlaceholder>
    }

    const getAdminButton = (movie: IMovie) => !props.isAdmin ? undefined : <AdminButton movie={movie}></AdminButton>;
    return <MovieCard {...{movie, key: movie.title, readOnly: false, extraBox: getAdminButton(movie)}} />
});
}
}


function AdminButton(props: {movie: IMovie}) {
    function onClick() {
        fetch(`api/nominations?movieId=${props.movie.movieId}`, { method: "DELETE"});
    }

    return <Button onClick={onClick}>Delete this movie</Button>
}

export default connect(
    (state: ApplicationState) => ({ 
        ...state.moviesList, 
        votingState: state.state?.state,
        isAdmin: state.user?.user?.isAdmin ?? false,
    }),
    MoviesList.actionCreators
)(MoviesListWrapper as any);