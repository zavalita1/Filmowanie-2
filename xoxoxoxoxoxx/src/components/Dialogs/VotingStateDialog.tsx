import * as React from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import * as State from '../../store/votingState';

type AlertDialogProps = State.StateState &
{
    endVoting: () => State.VotingConcludedAction
    startVoting: () => State.VotingConcludedAction
};

function AlertDialog(props: AlertDialogProps) {
    const isStartingOpen = props.state === State.VotingState.VotingStarting;
    const isEndingOpen = props.state === State.VotingState.VotingEnding;

  
    return (
    <div>
      <Dialog
        open={isStartingOpen}
        onClose={() => props.startVoting()}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">
          {"ZMIANY ZMIANY ZMIANY"}
        </DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            Głosowanie się zaczyna...
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => props.startVoting()}>O boże o kurwa</Button>
          <Button onClick={() => props.startVoting()} autoFocus>
            Osom
          </Button>
        </DialogActions>
      </Dialog>
      <Dialog
        open={isEndingOpen}
        onClose={() => props.endVoting()}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">
          {"ZMIANY ZMIANY ZMIANY"}
        </DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            Głosowanie uległo zakończeniu.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => props.endVoting()}>O boże o kurwa</Button>
          <Button onClick={() => props.endVoting()} autoFocus>
            Osom
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}

export default connect(
    (state: ApplicationState) => state.state,
    {
        endVoting: () => ({type:'VOTING_ENDED'}),
        startVoting: () => ({type:'VOTING_STARTED'}),
    }
    )(AlertDialog as any);