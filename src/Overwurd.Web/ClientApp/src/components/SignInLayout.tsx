import * as React from 'react';
import { AppBar, Box, Link, Toolbar } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { Outlet } from 'react-router-dom';

export const SignInLayout: React.FunctionComponent = () => (
    <Box>
        <AppBar position="static" color="primary" enableColorOnDark>
            <Toolbar>
                <Link component={RouterLink as any} to="/" underline="none" variant="h6" style={{ color: '#000000' }}>
                    Overwurd
                </Link>
            </Toolbar>
        </AppBar>
        <Box>
            <Outlet />
        </Box>
    </Box>
);