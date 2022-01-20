import * as React from 'react';
import { Alert, Avatar, Box, Container, Typography } from '@mui/material';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import { Centered } from './Centered';

interface ICenteredAuthFormProps {
    title: string;
    onSubmit: (event: React.FormEvent<HTMLFormElement>) => void;
    apiError: boolean;
    alerts?: string[];
}

export const CenteredAuthForm = (props: React.PropsWithChildren<ICenteredAuthFormProps>) => (
    <Centered>
        <Container component="main" maxWidth="xs">
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 2 }}>
                <Avatar sx={{ m: 1, bgcolor: 'secondary.main' }}>
                    <AccountCircleIcon />
                </Avatar>
                <Typography component="h1" variant="h5" sx={{ mb: 2 }}>
                    {props.title}
                </Typography>
                <Box sx={{ mb: 2 }} style={{ width: '100%' }}>
                    {props.apiError &&
                     <Alert severity="error" variant="filled" sx={{ mb: 1 }}>
                         We couldn't connect to our server. If the reason for this is some issue on our server
                         we will try to fix it as soon as possible. Please try again later.
                     </Alert>}
                    {props.alerts !== undefined && props.alerts.length > 0 && props.alerts.map((x, i) => (
                        <Alert severity="error" key={i} sx={{ mb: 1 }}>{x}</Alert>)
                    )}
                </Box>
                <Box component="form" onSubmit={props.onSubmit}>
                    {props.children}
                </Box>
            </Box>
        </Container>
    </Centered>
);
