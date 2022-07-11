import * as React from 'react';
import { AppBar, Box, Toolbar } from '@mui/material';
import { AppBarTitle } from './AppBarTitle';

export interface ISignInLayoutProps {
    homePath: string;
}

export const SignInLayout: React.FunctionComponent<ISignInLayoutProps> =
    (props: React.PropsWithChildren<ISignInLayoutProps>) => (
        <Box>
            <AppBar position="static" color="primary" enableColorOnDark>
                <Toolbar>
                    <AppBarTitle enableLink homePath={props.homePath} title="Overwurd" />
                </Toolbar>
            </AppBar>
            <Box>
                {props.children}
            </Box>
        </Box>
    );