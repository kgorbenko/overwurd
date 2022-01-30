import React from 'react';
import { NavigateOptions, Route, Routes, useNavigate } from 'react-router-dom';
import { useCurrentDateTime } from './hooks/UseCurrentDateTime';
import { NotFound } from './pages/NotFound';
import { useAuth } from './hooks/UseAuth';
import { withDisableAsync, withLoadingAsync } from './utils/Misc';
import { CenteredCircularProgress } from './components/CenteredCircularProgress';
import { AuthRegion, AuthRegionRoutes } from './AuthRegion';
import { LandingRegion, LandingRegionRoutes } from './LandingRegion';
import { DashboardRegion, DashboardRegionRoutes } from './DashboardRegion';
import { SignOutRegion, SignOutRegionRoutes } from './SignOutRegion';

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
            navigate(AuthRegionRoutes.signInAbsoluteRoute, options);
        }

        const navigateToSignOut = (options?: NavigateOptions) => {
            navigate(SignOutRegionRoutes.absoluteRoute, options);
        }

        const navigateHome = (options?: NavigateOptions) => {
            navigate(LandingRegionRoutes.homeRoute, options)
        }

        if (isLoading) {
            return <CenteredCircularProgress />;
        }

        return (
            <Routes>
                <Route path={LandingRegionRoutes.homeRoute} element={<LandingRegion requireUnauthenticatedProps={{ navigateTo: DashboardRegionRoutes.absoluteBaseRoute }} landingPageProps={{ onSignIn: navigateToSignIn }} />} />
                <Route path={`${DashboardRegionRoutes.baseRoute}/*`} element={<DashboardRegion layoutProps={{ onSignOut: navigateToSignOut, homePath: DashboardRegionRoutes.absoluteBaseRoute }} requireAuthenticatedProps={{ navigateTo: AuthRegionRoutes.signInAbsoluteRoute }} />} />
                <Route path={`${AuthRegionRoutes.baseRoute}/*`} element={<AuthRegion requireUnauthenticatedProps={{ navigateTo: DashboardRegionRoutes.absoluteBaseRoute }} signInProps={{ defaultRedirectPath: DashboardRegionRoutes.absoluteBaseRoute }} signInLayoutProps={{ homePath: LandingRegionRoutes.homeRoute }} />} />
                <Route path={SignOutRegionRoutes.baseRoute} element={<SignOutRegion requireAuthenticatedProps={{ navigateTo: LandingRegionRoutes.homeRoute }} signOutPageProps={{ onPostSignOut: navigateHome }} />} />
                <Route path="*" element={<NotFound />} />
            </Routes>
        );
    };
