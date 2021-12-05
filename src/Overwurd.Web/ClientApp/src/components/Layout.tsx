import React from 'react';
import { NavMenu } from './NavMenu';

type LayoutProps = {
    children?: React.ReactNode | undefined;
}

export const Layout: React.FunctionComponent = ({
    children
}: LayoutProps) =>
    <div>
        <NavMenu />
        {children}
    </div>;