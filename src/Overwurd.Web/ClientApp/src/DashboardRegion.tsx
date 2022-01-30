import * as React from 'react';
import { Route, Routes } from 'react-router-dom';
import { RequireAuthenticated } from './components/RequireAuthenticated';
import { ILayoutProps, Layout } from './components/Layout';
import { NotFound } from './pages/NotFound';

export interface DashboardRegionProps {
    layoutProps: ILayoutProps;
}

export const Protected = () => <h1>Protected</h1>;

export const DashboardRegion: React.FunctionComponent<DashboardRegionProps> =
    (props: DashboardRegionProps) => {
        return (
            <RequireAuthenticated navigateTo="/auth/signin">
                <Layout { ...props.layoutProps }>
                    <Routes>
                        <Route index element={<Protected />} />
                        <Route path="*" element={<NotFound />} />
                    </Routes>
                </Layout>
            </RequireAuthenticated>
        );
    };