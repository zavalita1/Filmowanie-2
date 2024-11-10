import * as React from 'react';
import Typography from '@mui/material/Typography';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { IMovie } from '../../store/MoviesList';
import RatingContainer, { IIconsAvailability } from './RatingContainer';
import { MoviesListProps } from './MovieCard';

export function MovieCardExpandedPart(props: MoviesListProps) {
    const [isHidden, setIsHidden] = React.useState(props.movie.userCurrentVotes === 0);

    if (props.movie.isPlaceholder) {
        return <></>;
    }

    const filmwebLinkClassName = !props.isMobile ? 'movie-card-link' : 'movie-card-link movie-card mobile';
    const actorsClassName = !props.isMobile ? 'movie-card-actors movie-card-people' : 'movie-card-actors mobile movie-card-people';

    return (<><div className='movie-card-expand movie-card-align-right' onClick={() => setIsHidden(!isHidden)}>
        <ExpandMoreIcon />
    </div>
        {hideableContainer(isHidden, <div className='movie-card-expanded'>
            <>
                <div className='movie-card-people'><b>Gatunek</b>: {props.movie.genres.join(', ')}</div> <br/>
                <div className='movie-card-people'><b>Reżyseria</b>: {props.movie.directors.join(', ')}</div> <br/>
                <div className='movie-card-people'><b>Scenariusz</b>: {props.movie.writers.join(', ')}</div> <br/>
                <div className={actorsClassName}><b>Występują</b>: {props.movie.actors.join(', ')}</div>
                {!props.isMobile ? <></> : <div className='movie-card-votes-description movie-card mobile'><i><Typography>{props.movie.description}</Typography></i></div>}
                <div className={filmwebLinkClassName}><a href={props.movie.filmwebUrl} target="_blank">Link do filmweba.</a></div>
               
                {renderRatingContainer()}
            </>
        </div>)}
    </>);

    function renderRatingContainer() {
        if (props.isReadOnly) {
            return <></>;
        }

        return (<div className='movie-card-votes-count movie-card-align-right'>
            <RatingContainer iconsAvailability={getChosenIconIndex(props.movie, props.movies)} onChange={(value: number) => onVotesChange(value, props.movie, props)}></RatingContainer>
        </div>);
    }
}

function getChosenIconIndex(currentMovie: IMovie, allMovies: IMovie[]): IIconsAvailability {
    if (currentMovie.userCurrentVotes == -1) {
        return { chosenIndex: 0, availableIconsIndices: [1] };
    }
    if (currentMovie.userCurrentVotes > 0) {
        return { chosenIndex: currentMovie.userCurrentVotes, availableIconsIndices: [currentMovie.userCurrentVotes + 1] };
    }

    const availableIconsIndices = [];
    if (allMovies.findIndex(x => x.userCurrentVotes === -1) === -1) {
        availableIconsIndices.push(1);
    }
    if (allMovies.findIndex(x => x.userCurrentVotes === 1) === -1) {
        availableIconsIndices.push(2);
    }
    if (allMovies.findIndex(x => x.userCurrentVotes === 2) === -1) {
        availableIconsIndices.push(3);
    }
    if (allMovies.findIndex(x => x.userCurrentVotes === 3) === -1) {
        availableIconsIndices.push(4);
    }

    return {
        availableIconsIndices
    };
}
function onVotesChange(value: number, currentMovie: IMovie, props: MoviesListProps) {
    if (currentMovie.userCurrentVotes !== 0) {
        props.reset(currentMovie.title);
        return;
    }

    const valueToIncrement = value === 1 ? -1 : value - 1;
    props.increment(currentMovie.title, valueToIncrement);
}
function hideableContainer(hidden: boolean, element: any) {
    if (hidden) {
        return <div className='hidden'>
            {element}
        </div>;
    }

    return element;
}
