import * as React from 'react';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';

export default function MovieCard(props: {message: string }) {
    return <Paper elevation={3} key={props.message}>
        <div className='movie-card'>
            <div className='movie-card-placeholder'>
                <Typography variant='h6' >{props.message}</Typography>
            </div>
        </div>
    </Paper>
}

