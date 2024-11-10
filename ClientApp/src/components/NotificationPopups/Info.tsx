import * as React from 'react';
import Stack from '@mui/material/Stack';
import Snackbar from '@mui/material/Snackbar';
import MuiAlert, { AlertProps } from '@mui/material/Alert';
import Slide  from '@mui/material/Slide';

const Alert = React.forwardRef<HTMLDivElement, AlertProps>(function Alert(
  props,
  ref,
) {
  return <MuiAlert elevation={6} ref={ref} variant="filled" {...props} />;
});

export default function CustomizedSnackbars(props: {onAknowledge: () => void, message?: string}) {
  const handleClose = (event?: React.SyntheticEvent | Event, reason?: string) => {
    
    if (reason === 'clickaway') {
      return;
    }

    props.onAknowledge();
  };
  const show = props.message !== undefined;

  return (
    <Stack spacing={2} sx={{ width: '100%' }}>
      { show ? 
        <Snackbar open={show} autoHideDuration={3000} onClose={handleClose} TransitionComponent={Slide}>
        <Alert onClose={handleClose} severity="info" sx={{ width: '100%' }}>
          {props.message}
        </Alert>
      </Snackbar> :
      <></>
      }
    </Stack>
  );
}