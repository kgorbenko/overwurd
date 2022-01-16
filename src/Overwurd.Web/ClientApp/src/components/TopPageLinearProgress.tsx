import * as React from 'react';
import { LinearProgress } from '@mui/material';
import { LinearProgressProps } from '@mui/material/LinearProgress/LinearProgress';

export const TopPageLinearProgress = (props: LinearProgressProps) => {
    return (
        <LinearProgress {...props} style={{
            position: 'fixed',
            top: 0,
            left: 0,
            width: '100vw'
        }} />
    );
}