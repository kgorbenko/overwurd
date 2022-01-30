import React from 'react';
import { AppBar, Avatar, Box, Menu, MenuItem, Toolbar, Typography } from '@mui/material';
import { AppBarTitle } from './AppBarTitle';

import AccountCircleIcon from '@mui/icons-material/AccountCircle';

export interface ILayoutProps {
    homePath: string;
    onSignOut: () => void;
}

export const Layout: React.FunctionComponent<ILayoutProps> =
    (props: React.PropsWithChildren<ILayoutProps>) => {
        const [userMenuAnchor, setUserMenuAnchor] = React.useState<HTMLElement | undefined>(undefined);
        const userMenuOptions = [{ title: 'Sign out', onClick: props.onSignOut }];

        const handleOpenUserMenu = (event: React.SyntheticEvent<HTMLElement>) => {
            setUserMenuAnchor(event.currentTarget);
        }

        const handleCloseUserMenu = () => {
            setUserMenuAnchor(undefined);
        }

        return (
            <Box>
                <AppBar position="static" color="primary" enableColorOnDark>
                    <Toolbar>
                        <AppBarTitle enableLink homePath={props.homePath} title="Overwurd" />
                        <Avatar sx={{ bgcolor: 'secondary.main', ml: 2 }} onClick={handleOpenUserMenu}>
                            <AccountCircleIcon fontSize="large" />
                        </Avatar>
                        <Menu
                            open={userMenuAnchor !== undefined}
                            anchorEl={userMenuAnchor}
                            onClose={handleCloseUserMenu}
                        >
                            {userMenuOptions.map((x, i) => (
                                <MenuItem key={i} onClick={x.onClick}>
                                    <Typography>{x.title}</Typography>
                                </MenuItem>
                            ))}
                        </Menu>
                    </Toolbar>
                </AppBar>
                {props.children}
            </Box>
        );
    };