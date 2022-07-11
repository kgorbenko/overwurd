import * as React from 'react';
import { CircularProgress, CircularProgressProps } from '@mui/material';
import { Centered } from './Centered';

export const CenteredCircularProgress = (props: CircularProgressProps) => (
    <Centered>
        <CircularProgress size={70} {...props} />
    </Centered>
);