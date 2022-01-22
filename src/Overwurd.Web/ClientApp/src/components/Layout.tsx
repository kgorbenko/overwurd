import React from 'react';
import { NavMenu } from './NavMenu';
import { Outlet } from 'react-router-dom';
import { Box } from '@mui/material';
import { Dayjs } from 'dayjs';

interface ILayoutProps {
    now: Dayjs;
    hideControls?: boolean;
}

export const Layout = ({ hideControls = false, now }: ILayoutProps) =>
    <Box sx={{ bgcolor: 'background.default' }}>
        {!hideControls && <NavMenu now={now} />}
        <Outlet />
    </Box>;