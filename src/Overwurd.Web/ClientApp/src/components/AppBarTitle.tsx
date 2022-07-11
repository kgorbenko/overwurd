import { Box, Link, Typography } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import React from 'react';

interface IAppBarTitleProps {
    enableLink?: boolean;
    homePath?: string;
    title?: string;
}

export const AppBarTitle: React.FunctionComponent<IAppBarTitleProps> = ({
    enableLink = true,
    homePath,
    title = 'Overwurd'
}) => (
    <Box sx={{ flexGrow: 1 }}>
        {enableLink
            ? <Link
                component={RouterLink as any}
                to={homePath}
                underline="none"
                variant="h6"
                style={{ color: '#000000' }}
            >
                {title}
            </Link>

            : <Typography variant="h6" style={{ color: '#000000' }}>
                {title}
            </Typography>}
    </Box>)
