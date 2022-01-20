import React from 'react';
import { NavMenu } from './NavMenu';
import { Outlet } from 'react-router-dom';
import { Box } from '@mui/material';

interface ILayoutProps {
    hideControls?: boolean;
}

export const Layout = ({ hideControls = false }: ILayoutProps) =>
    <Box sx={{ bgcolor: 'background.default' }}>
        {!hideControls && <NavMenu />}
        <Outlet />
    </Box>;