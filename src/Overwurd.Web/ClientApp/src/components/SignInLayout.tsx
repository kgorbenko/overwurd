import * as React from 'react';
import { AppBar, Box, Toolbar } from '@mui/material';
import { Outlet } from 'react-router-dom';
import { AppBarTitle } from './AppBarTitle';

interface ISignInLayoutProps {
    homePath: string;
}

export const SignInLayout: React.FunctionComponent<ISignInLayoutProps> = ({
    homePath
}: ISignInLayoutProps) => (
    <Box>
        <AppBar position="static" color="primary" enableColorOnDark>
            <Toolbar>
                <AppBarTitle enableLink homePath={homePath} title="Overwurd" />
            </Toolbar>
        </AppBar>
        <Box>
            <Outlet />
        </Box>
    </Box>
);