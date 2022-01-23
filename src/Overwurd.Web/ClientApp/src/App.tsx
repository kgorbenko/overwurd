import * as React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { AppContextProvider } from './AppContextProvider';
import { Dashboard } from './Dashboard';
import { theme } from './Theme';
import { CssBaseline, ThemeProvider } from '@mui/material';

export const App: React.FunctionComponent<{}> = (props: React.PropsWithChildren<{}>) => {
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