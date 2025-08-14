import * as React from 'react';
import * as MoviesList from '../../store/MoviesList';
import Typography from '@mui/material/Typography';
import { IMovie } from '../../store/MoviesList';
import Paper from '@mui/material/Paper';
import { ApplicationState } from '../../store';
import { connect } from 'react-redux';
import { MovieCardExpandedPart } from './MovieCardExpandedPart';

interface IMovieCardProps {
    movie: IMovie;
    key: string;
    isReadOnly: string;
    extraBox?: JSX.Element;
}

export type MoviesListProps = MoviesList.MoviesListState &
    typeof MoviesList.actionCreators &
    IMovieCardProps &
{ isMobile: boolean };

function MovieCard(props: MoviesListProps) {
    const movie = props.movie;

    const movieCardClassName = !props.isMobile ? 'movie-card' : 'movie-card mobile';
    const movieCardTitleClassName = !props.isMobile ? 'movie-card-title' : 'movie-card-title mobile';
    const movieCardImageClassName = !props.isMobile ? 'movie-card-image' : 'movie-card-image mobile';

    return <Paper elevation={3} key={props.key}>
        <div className={movieCardClassName}>
            <div className={movieCardImageClassName}>
                <img src={movie.posterUrl}></img>
            </div>
            <div className={movieCardTitleClassName}>
                <Typography variant='h4' >{movie.title} ({movie.createdYear})</Typography>
                <Typography variant='h6' >{movie.duration}</Typography>
                { props.extraBox !== undefined ? props.extraBox : <></>}
                {props.isMobile ? <></> : <i><Typography paragraph={true} gutterBottom={true}>{movie.description}</Typography></i>}
            </div>
        <MovieCardExpandedPart {...props}></MovieCardExpandedPart>
        </div>
    </Paper>
}


function mapProps(state: ApplicationState, ownProps: { readOnly: boolean, extraBox?: JSX.Element}) {
    return ({
        ...state.moviesList, 
        isMobile: state.state?.isMobile, 
        isReadOnly: ownProps.readOnly,
        extraBox: ownProps.extraBox
    });
}

export default connect(
    mapProps,
    MoviesList.actionCreators
)(MovieCard as any);