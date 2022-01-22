import React from 'react';
import { AppBar, Box, Button, Toolbar, Typography } from '@mui/material';
import { isMobile } from 'react-device-detect';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import { AppBarTitle } from '../components/AppBarTitle';
import { NavigateOptions } from 'react-router-dom';

interface ILandingPageProps {
    onSignIn: (options?: NavigateOptions) => void;
}

export const LandingPage = ({
    onSignIn,
}: ILandingPageProps) => {
    const handleSignIn = () => {
        onSignIn();
    }

    return <Box>
        <AppBar position="static" color="primary" enableColorOnDark>
            <Toolbar>
                <AppBarTitle enableLink={false} title="Overwurd" />
                <Button variant="contained" color="secondary" startIcon={<AccountCircleIcon />} onClick={handleSignIn}>
                    Sign In
                </Button>
            </Toolbar>
        </AppBar>
        <Box>
            <Typography
                align="center"
                variant="h1"
                sx={{ mt: 20 }}
                style={{ fontSize: isMobile ? '4rem' : '7rem' }}>
                Overwurd
            </Typography>
        </Box>
    </Box>;
};
