import React from 'react';
import { NavigateOptions, Route, Routes, useNavigate } from 'react-router-dom';
import { useCurrentDateTime } from './hooks/UseCurrentDateTime';
import { NotFound } from './pages/NotFound';
import { useAuth } from './hooks/UseAuth';
import { withDisableAsync, withLoadingAsync } from './utils/Misc';
import { CenteredCircularProgress } from './components/CenteredCircularProgress';
import { AuthRegion } from './AuthRegion';
import { LandingRegion } from './LandingRegion';
import { DashboardRegion } from './DashboardRegion';
import { SignOutRegion } from './SignOutRegion';

export const Overwurd: React.FunctionComponent =
    () => {
        const now = useCurrentDateTime(30000);
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
                <Route path="/" element={<LandingRegion requireUnauthenticatedProps={{ navigateTo: '/dashboard' }} landingPageProps={{ onSignIn: navigateToSignIn }} />} />
                <Route path="dashboard/*" element={<DashboardRegion layoutProps={{ onSignOut: navigateToSignOut, homePath: '/dashboard' }} />} />
                <Route path="auth/*" element={<AuthRegion requireUnauthenticatedProps={{ navigateTo: '/dashboard' }} signInProps={{ defaultRedirectPath: '/dashboard' }} signInLayoutProps={{ homePath: '/' }} />} />
                <Route path="auth/signOut" element={<SignOutRegion requireAuthenticatedProps={{ navigateTo: '/' }} signOutPageProps={{ onPostSignOut: navigateHome }} />} />
                <Route path="*" element={<NotFound />} />
            </Routes>
        );
    };
