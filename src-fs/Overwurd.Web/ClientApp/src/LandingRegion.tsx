import * as React from 'react';
import { Route, Routes } from 'react-router-dom';
import { IRequireUnauthenticatedProps, RequireUnauthenticated } from './components/RequireUnauthenticated';
import { ILandingPageProps, LandingPage } from './pages/LandingPage';
import { NotFound } from './pages/NotFound';

interface ILandingRegionProps {
    requireUnauthenticatedProps: IRequireUnauthenticatedProps;
    landingPageProps: ILandingPageProps;
}

export class LandingRegionRoutes {
    public static readonly homeRoute = '/';
}

export const LandingRegion: React.FunctionComponent<ILandingRegionProps> =
    (props: React.PropsWithChildren<ILandingRegionProps>) => {
        return (
            <RequireUnauthenticated { ...props.requireUnauthenticatedProps }>
                <Routes>
                    <Route path={LandingRegionRoutes.homeRoute} element={<LandingPage { ...props.landingPageProps } />} />
                    <Route path="*" element={<NotFound />} />
                </Routes>
            </RequireUnauthenticated>
        );
    };