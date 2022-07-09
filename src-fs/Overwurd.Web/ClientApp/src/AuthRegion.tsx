import * as React from 'react';
import { Route, Routes } from 'react-router-dom';
import { IRequireUnauthenticatedProps, RequireUnauthenticated } from './components/RequireUnauthenticated';
import { ISignInLayoutProps, SignInLayout } from './components/SignInLayout';
import { ISignInProps, SignInPage } from './pages/SignInPage';
import { SignUpPage } from './pages/SignUpPage';
import { NotFound } from './pages/NotFound';

interface AuthRegionProps {
    requireUnauthenticatedProps: IRequireUnauthenticatedProps;
    signInLayoutProps: ISignInLayoutProps;
    signInProps: ISignInProps;
}

export class AuthRegionRoutes {
    public static readonly baseRoute = 'auth';

    public static readonly signInRoute = 'signIn';
    public static readonly signInAbsoluteRoute = `/${AuthRegionRoutes.baseRoute}/${AuthRegionRoutes.signInRoute}`;

    public static readonly signUpRoute = 'signUp';
    public static readonly signUpAbsoluteRoute = `/${AuthRegionRoutes.baseRoute}/${AuthRegionRoutes.signUpRoute}`;
}

export const AuthRegion: React.FunctionComponent<AuthRegionProps> =
    (props: React.PropsWithChildren<AuthRegionProps>) => {
        return (
            <RequireUnauthenticated { ...props.requireUnauthenticatedProps }>
                <SignInLayout { ...props.signInLayoutProps }>
                    <Routes>
                        <Route path={AuthRegionRoutes.signInRoute} element={<SignInPage { ...props.signInProps } />} />
                        <Route path={AuthRegionRoutes.signUpRoute} element={<SignUpPage { ...props.signInProps } />} />
                        <Route path="*" element={<NotFound />} />
                    </Routes>
                </SignInLayout>
            </RequireUnauthenticated>
        );
    };
