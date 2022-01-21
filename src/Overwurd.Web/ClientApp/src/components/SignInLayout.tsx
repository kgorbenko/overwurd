import * as React from 'react';
import { AppBar, Box, Link, Toolbar, Typography } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { Outlet } from 'react-router-dom';

export const SignInLayout: React.FunctionComponent = () => (
    <Box sx={{ flexGrow: 1 }}>
        <AppBar position="static" color="default">
            <Toolbar>
                <Typography>
                    <Link component={RouterLink as any} to="/" underline="none" variant="h6">
                        Overwurd
                    </Link>
                </Typography>
            </Toolbar>
        </AppBar>
        <Box>
            <Outlet />
        </Box>
    </Box>
);