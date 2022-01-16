import React from 'react';
import { NavMenu } from './NavMenu';
import { Outlet } from 'react-router-dom';

export const Layout: React.FunctionComponent = () =>
    <div>
        <NavMenu />
        <Outlet />
    </div>;