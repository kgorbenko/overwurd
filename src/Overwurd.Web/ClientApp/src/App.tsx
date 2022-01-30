import * as React from 'react';
import * as dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { BrowserRouter } from 'react-router-dom';
import { AppContextProvider } from './AppContextProvider';
import { Dashboard } from './Dashboard';
import { theme } from './Theme';
import { CssBaseline, ThemeProvider } from '@mui/material';

import '@fontsource/roboto/300.css';
import '@fontsource/roboto/400.css';
import '@fontsource/roboto/500.css';
import '@fontsource/roboto/700.css';

import './App.css';

dayjs.extend(utc);

export const App: React.FunctionComponent = () => {
    const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href') ?? undefined;
    return (
        <BrowserRouter basename={baseUrl}>
            <AppContextProvider>
                <ThemeProvider theme={theme}>
                    <CssBaseline />
                    <Dashboard />
                </ThemeProvider>
            </AppContextProvider>
        </BrowserRouter>
    );
}