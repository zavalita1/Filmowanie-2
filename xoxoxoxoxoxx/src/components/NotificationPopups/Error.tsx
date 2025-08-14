import * as React from 'react';
import Stack from '@mui/material/Stack';
import Snackbar from '@mui/material/Snackbar';
import MuiAlert, { AlertProps } from '@mui/material/Alert';

const Alert = React.forwardRef<HTMLDivElement, AlertProps>(function Alert(
  props,
  ref,
) {
  return <MuiAlert elevation={6} ref={ref} variant="filled" {...props} />;
});

export default function CustomizedSnackbars(props: {showErrorToast: boolean, onAknowledgeError: () => void, errorMessage?: string}) {
  const handleClose = (event?: React.SyntheticEvent | Event, reason?: string) => {
    if (reason === 'clickaway') {
      return;
    }

    props.onAknowledgeError();
  };

  return (
    <Stack spacing={2} sx={{ width: '100%' }}>
      { props.errorMessage !== undefined ? 
        <Snackbar open={props.showErrorToast} autoHideDuration={6000} onClose={handleClose}>
        <Alert onClose={handleClose} severity="error" sx={{ width: '100%' }}>
          {props.errorMessage}
        </Alert>
      </Snackbar> :
      <></>
      }
    </Stack>
  );
}