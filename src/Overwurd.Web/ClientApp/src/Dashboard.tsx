import React from 'react';
import { Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './pages/Home';
import { useCurrentDateTime } from './hooks/use-current-date-time';
import { RequireAuthenticated } from './components/RequireAuthenticated';
import { SignInPage } from './pages/SignInPage';
import { NotFound } from './pages/NotFound';
import { SignUpPage } from './pages/SignUpPage';
import { useAuth } from './hooks/use-auth';
import { withDisableAsync, withLoadingAsync } from './utils/misc';
import { SignOutPage } from './pages/SignOutPage';
import { CenteredCircularProgress } from './components/CenteredCircularProgress';
import { SignInLayout } from './components/SignInLayout';
import { RequireUnauthenticated } from './components/RequireUnauthenticated';

export const Protected = () => <h1>Protected</h1>;

export const Dashboard = () => {
    const now = useCurrentDateTime(1000);
    const auth = useAuth(now);

    const [isLoading, setLoading] = React.useState<boolean>(false);
    const [shouldAutoRefreshToken, setShouldAutoRefreshToken] = React.useState<boolean>(false);

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
        <Routes>
            <Route path="/" element={<Layout now={now} />}>
                <Route index element={<Home />} />
                <Route path="protected" element={<RequireAuthenticated navigateTo="/auth/signin"><Protected /></RequireAuthenticated>} />
            </Route>
            <Route path="auth" element={<RequireUnauthenticated navigateTo="/"><SignInLayout /></RequireUnauthenticated>}>
                <Route index element={<NotFound />} />
                <Route path="signin" element={<SignInPage />} />
                <Route path="signup" element={<SignUpPage />} />
            </Route>
            <Route path="/auth/signout" element={<RequireAuthenticated navigateTo="/"><SignOutPage /></RequireAuthenticated>} />
            <Route path="*" element={<NotFound />} />
        </Routes>
    );
}
