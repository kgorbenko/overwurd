import * as React from 'react';
import { IRequireAuthenticatedProps, RequireAuthenticated } from './components/RequireAuthenticated';
import { Route, Routes } from 'react-router-dom';
import { ISignOutPageProps, SignOutPage } from './pages/SignOutPage';
import { NotFound } from './pages/NotFound';

export interface ISignOutRegionProps {
    requireAuthenticatedProps: IRequireAuthenticatedProps;
    signOutPageProps: ISignOutPageProps;
}

export class SignOutRegionRoutes {
    public static readonly baseRoute = 'auth/signOut';
    public static readonly absoluteRoute = '/auth/signOut';
}

export const SignOutRegion: React.FunctionComponent<ISignOutRegionProps> =
    (props: ISignOutRegionProps) => {
        return (
            <RequireAuthenticated { ...props.requireAuthenticatedProps } >
                <Routes>
                    <Route index element={<SignOutPage { ...props.signOutPageProps } />} />
                    <Route path="*" element={<NotFound />} />
                </Routes>
            </RequireAuthenticated>
        );
    };