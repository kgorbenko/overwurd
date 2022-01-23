import React from 'react';
import { NavigateOptions, Route, Routes, useNavigate } from 'react-router-dom';
import { LandingPage } from './pages/LandingPage';
import { useCurrentDateTime } from './hooks/UseCurrentDateTime';
import { RequireAuthenticated } from './components/RequireAuthenticated';
import { SignInPage } from './pages/SignInPage';
import { NotFound } from './pages/NotFound';
import { SignUpPage } from './pages/SignUpPage';
import { useAuth } from './hooks/UseAuth';
import { withDisableAsync, withLoadingAsync } from './utils/Misc';
import { SignOutPage } from './pages/SignOutPage';
import { CenteredCircularProgress } from './components/CenteredCircularProgress';
import { SignInLayout } from './components/SignInLayout';
import { RequireUnauthenticated } from './components/RequireUnauthenticated';
import { Layout } from './components/Layout';

export const Protected = () => <h1>Protected</h1>;

export const Dashboard = () => {
    const now = useCurrentDateTime(1000);
    const auth = useAuth(now);
    const navigate = useNavigate();

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

    const navigateToSignIn = (options?: NavigateOptions) => {
        navigate('/auth/signin', options);
    }

    const navigateToSignOut = (options?: NavigateOptions) => {
        navigate('/auth/signout', options);
    }

    const navigateHome = (options?: NavigateOptions) => {
        navigate('/', options)
    }

    if (isLoading) {
        return <CenteredCircularProgress />;
    }

    return (
        <Routes>
            <Route path="/" element={
                <RequireUnauthenticated navigateTo="/dashboard">
                    <LandingPage onSignIn={navigateToSignIn} />
                </RequireUnauthenticated>} />
            <Route path="dashboard" element={
                <RequireAuthenticated navigateTo="/auth/signin">
                    <Layout homePath="/dashboard" onSignOut={navigateToSignOut} />
                </RequireAuthenticated>}>
                <Route index element={<Protected />} />
            </Route>
            <Route path="auth" element={
                <RequireUnauthenticated navigateTo="/dashboard">
                    <SignInLayout homePath="/" />
                </RequireUnauthenticated>}>
                <Route index element={<NotFound />} />
                <Route path="signin" element={<SignInPage defaultRedirectPath="/dashboard" />} />
                <Route path="signup" element={<SignUpPage defaultRedirectPath="/dashboard" />} />
            </Route>
            <Route path="auth/signout" element={
                <RequireAuthenticated navigateTo="/">
                    <SignOutPage onPostSignOut={navigateHome} />
                </RequireAuthenticated>} />
            <Route path="*" element={<NotFound />} />
        </Routes>
    );
}
