import * as React from 'react';
import { connect } from 'react-redux';
import * as Nominations from '../../store/Nominations';
import { ApplicationState } from '../../store';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import SubPageWrapper from '../SubPageWrapper';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import { useHistory } from 'react-router-dom';
import NominateConfirmationDialog from '../Dialogs/NominateConfirmationDialog';
import PosterPickerDialog from '../Dialogs/PosterPickerDialog';
import Checkbox from '@mui/material/Checkbox';
import FormGroup from '@mui/material/FormGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import MovieCard from '../MovieCard/MovieCard';
import { IMovie } from '../../store/MoviesList';
import { Typography } from '@mui/material';

type NominateProps = Nominations.NominationsState &
{isMobile?: boolean, nominationsLoaded: boolean, canUserNominate: boolean } &
typeof Nominations.actionCreators;

const Nominate = (props: NominateProps) => {
  const [url, setUrl] = React.useState('');
  const [pickPoster, setPickPoster] = React.useState<boolean>(false);
  const history = useHistory();

  if (!props.canUserNominate) {
    history.replace("/");
    return <></>
  }

  if (!props.nominationsLoaded) {
    return <></>;
  }

  const containerClassName = props.isMobile ? 'nominate-container mobile' : 'nominate-container';

  return (<>
    <div className={containerClassName}>
      <div className='nominate-header'>{getText(props.nominations!)}</div>
      <TextField id="standard-basic" label="Wklej link do filmweba" variant="standard"
        value={url} onChange={e => setUrl(e.target.value)} onKeyDown={onKeyDown} autoFocus={true}
        className='nominate-link' />
      <div className='break'></div>
      <FormControl component="fieldset">
        <FormGroup aria-label="position" row>
          <FormControlLabel
            value="end"
            control={<Checkbox onChange={() => setPickPoster(val => !val)} />}
            label="Chcę samodzielnie wybrać plakat"
            labelPlacement="end"
          />
        </FormGroup>
      </FormControl>
      <div className='break'></div>
      <Button variant="contained" type='button' onClick={nominateMovie} className='nominate-button'>Ślij</Button>
      <div className='break'></div>
      {
        renderMoviesList(props.moviesThatCanBeNominatedAgain)
      }
    </div>
    <NominateConfirmationDialog></NominateConfirmationDialog>
    <PosterPickerDialog posterUrls={props.pendingNomination?.possiblePosterUrls}></PosterPickerDialog>
  </>
  )
  
  
function renderMoviesList(movies?: IMovie[]) {
  if (movies === undefined) {
    return <></>;
  }

  const onExtraBoxClick = (movie: IMovie) => () => {
    setUrl(movie.filmwebUrl);
    window.scrollTo({ top: 0});
  }

  const extraBox = (movie: IMovie) => <div>
     <Button variant="contained" type='button' onClick={onExtraBoxClick(movie)} className='nominate-button'>Chcę nominować ten, wklej link</Button>
    </div>;

  return (<><div>
    <div className='break-big'></div>
    <Typography>Filmy, które można nominować ponownie: </Typography>
    <div className='break'></div>
    {movies.map(movie => {
      return <MovieCard {...{ movie, key: movie.title, readOnly: true, extraBox: extraBox(movie) }} />
    })}
  </div>
  </>);
};

  function onKeyDown(e: React.KeyboardEvent<HTMLDivElement>) {
    if (e.key === 'Enter') {
      e.preventDefault();
      nominateMovie();
    }
  }

  function nominateMovie() {
    setUrl("");
    props.nominateMovie(url, pickPoster);
  }
};


function getText(nominations: Nominations.NominationDecade[]) {
  if (nominations.length === 1) {
    return "Możesz nominować film z lat: " + nominations[0];
  }

  return "Możesz nominować filmy z lat: " + nominations.join(", ");
}

const NominateWrapper = (props: NominateProps) => {
  const subWrapperProps = {
      child: Nominate,
      childProps: props,
      initializeChildren: () => {
        if (!props.nominationsLoaded) {
          props.loadNominationsData();
        }
    },
  };

  return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
}

function mapProps(state: ApplicationState) {
  return ({ 
    ...state.nominations, 
    isMobile: state.state?.isMobile, 
    nominationsLoaded: state.nominations?.nominations?.length ?? 0 > 0, 
    canUserNominate: state.user?.user?.hasNominations ?? false 
  });
}

export default connect(
  mapProps,
  Nominations.actionCreators)(NominateWrapper as any);
