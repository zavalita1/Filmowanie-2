import * as React from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import * as Nominations from '../../store/Nominations';

type NominateConfirmationDialogProps = Nominations.NominationsState & 
typeof Nominations.actionCreators &
{ isLoading: boolean}

function NominateConfirmationDialog(props: NominateConfirmationDialogProps) {
    const isOpen = props.pendingNomination !== undefined && props.waitingForPosterPick !== true && props.isLoading !== true;
  
    return (
    <>
      <Dialog
        open={isOpen}
        onClose={props.cancelMovieNomination}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">
          {"Czy aby na pewno?"}
        </DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            Czy na pewno chcesz nominować podany film? Po zaakceptowaniu nie będzie odwrotu.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={props.cancelMovieNomination}>W sumie to chciałbym zmienić zdanie.</Button>
          <Button onClick={() => props.confirmMovieNomination()} autoFocus>
              Nie pierdol, tylko nominuj.
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}

export default connect(
    (state: ApplicationState) => ({...state.nominations, isLoading: state.app?.isLoading}),
    Nominations.actionCreators)(NominateConfirmationDialog as any);