import * as React from 'react';
import Backdrop from '@mui/material/Backdrop';
import CircularProgress from '@mui/material/CircularProgress';

export type LoaderProps = {
    isLoading: boolean;
}

export default function Loader(props: LoaderProps) {
  return (
    <div>
      <Backdrop
        sx={{ color: '#fff', zIndex: (theme) => theme.zIndex.drawer + 1 }}
        open={props.isLoading}
      >
        <CircularProgress color="inherit" />
      </Backdrop>
    </div>
  );
}