import * as React from 'react';
import { Avatar, Button, Box, TextField, Typography, Container, Alert } from '@mui/material';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import { Link, Navigate, useLocation } from 'react-router-dom';
import { withLoadingAsync } from '../utils/misc';
import { TopPageLinearProgress } from '../components/TopPageLinearProgress';
import { useAuth } from '../hooks/use-auth';
import dayjs from 'dayjs';

export const SignInPage = () => {
    const now = dayjs.utc();
    const { isAuthenticated, signInAsync } = useAuth(now);
    const location = useLocation();

    const [isLoading, setIsLoading] = React.useState<boolean>(false);
    const [isApiError, setIsApiError] = React.useState<boolean>(false);
    const [validationErrors, setValidationErrors] = React.useState<string[]>([]);
    const [canRedirectToHome, setCanRedirectToHome] = React.useState<boolean>(true);
    const [shouldRedirectToPreviousPage, setShouldRedirectToPreviousPage] = React.useState<boolean>(isAuthenticated);

    const from = location.state?.from?.pathname ?? '/';

    if (shouldRedirectToPreviousPage) {
        return <Navigate to={from} replace />;
    }

    if (isAuthenticated && canRedirectToHome) {
        return <Navigate to="/" replace />;
    }

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setCanRedirectToHome(false);

        const data = new FormData(event.currentTarget);

        await withLoadingAsync(setIsLoading, async () => {
            const { isSuccess, isApiError, validationErrors } = await signInAsync({
                userName: data.get('userName') as string,
                password: data.get('password') as string
            });

            if (isSuccess) {

                setShouldRedirectToPreviousPage(true);
            } else {
                setIsApiError(isApiError);
                setValidationErrors(validationErrors);
            }
        });
    }

    return (
        <Container component="main" maxWidth="xs" sx={{ mt: 8 }}>
            {isLoading && <TopPageLinearProgress />}
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Avatar sx={{ m: 1, bgcolor: 'secondary.main' }}>
                    <AccountCircleIcon />
                </Avatar>
                <Typography component="h1" variant="h5" sx={{ mb: 2 }}>
                    Sign in
                </Typography>
                <Box sx={{ mb: 2 }}>
                    {isApiError &&
                     <Alert severity="error" variant="filled" sx={{ mb: 1 }}>
                         We couldn't connect to our server. If the reason for this is some issue on our server
                         we will try to fix it as soon as possible. Please try again later.
                     </Alert>}
                    {validationErrors.length > 0 &&
                     <>
                         {validationErrors.map((x, i) =>
                             <Alert key={i} severity="error" sx={{ mb: 1 }}>{x}</Alert>)}
                     </>}
                </Box>
                <Box component="form" onSubmit={handleSubmit}>
                    <TextField
                        margin="dense"
                        required
                        fullWidth
                        id="userName"
                        label="User name"
                        name="userName"
                        autoComplete="username"
                        autoFocus
                        disabled={isLoading}
                    />
                    <TextField
                        margin="dense"
                        required
                        fullWidth
                        name="password"
                        label="Password"
                        type="password"
                        id="password"
                        autoComplete="current-password"
                        disabled={isLoading}
                    />
                    <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }} disabled={isLoading}>
                        Sign In
                    </Button>
                    <Typography variant="body2" align="right">
                        <Link to="/auth/signup" state={{ from: location.state?.from }}>
                            Don't have an account? Sign Up
                        </Link>
                    </Typography>
                </Box>
            </Box>
        </Container>
    );
}