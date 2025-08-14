import * as React from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import * as Nominations from '../../store/Nominations';

import ImageList from '@mui/material/ImageList';
import ImageListItem from '@mui/material/ImageListItem';

type PosterPickerDialogOwnProps = {
  posterUrls?: string[] 
}

type PosterPickerDialogProps = Nominations.NominationsState &
 typeof Nominations.actionCreators & 
 { isLoading?: boolean, isMobile?: boolean } &
 PosterPickerDialogOwnProps

function PosterPickerDialog(props: PosterPickerDialogProps) {
    const isOpen = props.waitingForPosterPick === true && props.isLoading !== true;
    const [selectedPosterUrl, setSelectedPosterUrl] = React.useState<string | undefined>(undefined);
  
    const columns = props.isMobile === true ? 1 : 3;
    const itemListClassName = props.isMobile === true ? 'poster-picker-dialog mobile' : 'poster-picker-dialog';
    return (
    <>
      <Dialog
        open={isOpen}
        onClose={props.cancelMovieNomination}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">
          {"Wybierz plakat"}
        </DialogTitle>
        <DialogContent >
        <ImageList cols={columns} rowHeight={285}  className={itemListClassName}>
      {props.posterUrls?.map(getPosterTile) ?? <></>}
    </ImageList>
        </DialogContent>
        <DialogActions>
          <Button onClick={props.cancelMovieNomination}>Jednak chcę nominować inny film</Button>
          <Button onClick={() => props.chosePoster(selectedPosterUrl!)} autoFocus disabled={!selectedPosterUrl}>
              Okej, dawaj ten plakat
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );

  function getPosterTile(item: string) {
      const className = selectedPosterUrl === item ? 'poster-tile selected' : 'poster-tile';

      return (<ImageListItem key={item}>
        <img
          className={className}
          srcSet={`${item}?w=164&h=164&fit=crop&auto=format&dpr=2 2x`}
          src={`${item}?w=164&h=164&fit=crop&auto=format`}
          alt={item}
          loading="lazy"
          onClick={() => setSelectedPosterUrl(previousItem => previousItem === item ? undefined : item)}
        />
      </ImageListItem>
  );
}
}

export default connect(
    (state: ApplicationState, ownProps: PosterPickerDialogOwnProps) => ({...state.nominations, isLoading: state.app?.isLoading, posterUrls: ownProps.posterUrls, isMobile: state.state?.isMobile}),
    Nominations.actionCreators)(PosterPickerDialog as any);