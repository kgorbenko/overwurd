import React from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './pages/Home';
import { useCurrentDateTime } from './hooks/use-current-date-time';
import { RequireAuth } from './components/RequireAuth';
import { SignInPage } from './pages/SignInPage';
import { NotFound } from './pages/NotFound';
import { SignUpPage } from './pages/SignUpPage';
import { useAuth } from './hooks/use-auth';
import { withDisableAsync, withLoadingAsync } from './utils/misc';
import { SignOutPage } from './pages/SignOutPage';
import { CenteredCircularProgress } from './components/CenteredCircularProgress';
import { AppContextProvider } from './AppContextProvider';
import { CssBaseline, ThemeProvider } from '@mui/material';
import { theme } from './theme';
import { SignInLayout } from './components/SignInLayout';

export const Protected = () => <h1>Protected</h1>;

export const App = () => {
    const now = useCurrentDateTime(1000);
    const auth = useAuth(now);

    const [isLoading, setLoading] = React.useState<boolean>(false);
    const [shouldAutoRefreshToken, setShouldAutoRefreshToken] = React.useState<boolean>(false);

    const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href') ?? undefined;

    React.useEffect(() => {
        (async () => {
            await withLoadingAsync(setLoading, async () => {
                if (auth.canRefreshAccessToken) {
                    await auth.refreshAccessTokenAsync();
                }
                setShouldAutoRefreshToken(true);
            });
        })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    React.useEffect(() => {
        (async () => {
            if (shouldAutoRefreshToken && auth.canRefreshAccessToken) {
                await withDisableAsync(setShouldAutoRefreshToken, async () => {
                    await auth.refreshAccessTokenAsync();
                });
            }
        })();
    }, [shouldAutoRefreshToken, auth, auth.canRefreshAccessToken]);

    if (isLoading) {
        return <CenteredCircularProgress />;
    }

    return (
        <BrowserRouter basename={baseUrl}>
            <AppContextProvider>
                <ThemeProvider theme={theme}>
                    <CssBaseline />
                    <Routes>
                        <Route path="/" element={<Layout />}>
                            <Route index element={<Home />} />
                            <Route path="protected" element={<RequireAuth now={now}><Protected /></RequireAuth>} />
                        </Route>
                        <Route path="auth" element={<SignInLayout />}>
                            <Route index element={<NotFound />} />
                            <Route path="signin" element={<SignInPage />}/>
                            <Route path="signup" element={<SignUpPage />}/>
                            <Route path="signout" element={<SignOutPage />}/>
                        </Route>
                        <Route path="*" element={<NotFound />} />
                    </Routes>
                </ThemeProvider>
            </AppContextProvider>
        </BrowserRouter>
    );
}
