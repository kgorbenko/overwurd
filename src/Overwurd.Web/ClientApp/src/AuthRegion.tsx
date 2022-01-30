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

export const AuthRegion: React.FunctionComponent<AuthRegionProps> =
    (props: React.PropsWithChildren<AuthRegionProps>) => {
        return (
            <RequireUnauthenticated { ...props.requireUnauthenticatedProps }>
                <SignInLayout { ...props.signInLayoutProps }>
                    <Routes>
                        <Route path="signin" element={<SignInPage { ...props.signInProps } />} />
                        <Route path="signup" element={<SignUpPage { ...props.signInProps } />} />
                        <Route path="*" element={<NotFound />} />
                    </Routes>
                </SignInLayout>
            </RequireUnauthenticated>
        );
    };
