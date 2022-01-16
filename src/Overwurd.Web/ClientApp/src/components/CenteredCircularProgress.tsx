import * as React from 'react';
import { CircularProgress, CircularProgressProps, Grid } from '@mui/material';

export const CenteredCircularProgress = (props: CircularProgressProps) => (
    <Grid container direction="column" alignItems="center" justifyContent="center" style={{ minHeight: '100vh' }}>
        <CircularProgress size={70} />
    </Grid>
);