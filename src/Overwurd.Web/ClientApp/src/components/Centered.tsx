import * as React from 'react';
import { Grid } from '@mui/material';

export const Centered = (props: React.PropsWithChildren<{}>) => {
    return (
        <Grid container direction="column" alignItems="center" justifyContent="center" style={{ minHeight: '100vh' }}>
            {props.children}
        </Grid>
    );
}