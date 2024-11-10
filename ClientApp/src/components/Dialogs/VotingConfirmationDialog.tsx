import * as React from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';

type VotingConfirmationDialogProps = {
  onClose: () => void;
  isOpen: boolean;
}

export default function NominateConfirmationDialog(props: VotingConfirmationDialogProps) {
    return (
    <>
      <Dialog
        open={props.isOpen}
        onClose={props.onClose}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">
          {"You're simply the best, better than all the rest."}
        </DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            Winszuję, wszystkie głosy zostały przydzielone. Możesz je jeszcze zmienić, dopóki admin nie zakończy głosowania podczas następnego filmowania.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={props.onClose}>Dobra, przestań strzelać.</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}