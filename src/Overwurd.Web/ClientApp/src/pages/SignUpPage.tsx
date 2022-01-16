import * as React from 'react';
import { Alert, Avatar, Box, Button, Container, Grid, TextField, Typography } from '@mui/material';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import { Link, useLocation, Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/use-auth';
import { withLoadingAsync } from '../utils/misc';
import { TopPageLinearProgress } from '../components/TopPageLinearProgress';
import dayjs from 'dayjs';

export const SignUpPage = () => {
    const now = dayjs.utc();
    const { isAuthenticated, signUpAsync } = useAuth(now);
    const location = useLocation();

    const [isLoading, setIsLoading] = React.useState<boolean>(false);
    const [isApiError, setIsApiError] = React.useState<boolean>(false);
    const [validationErrors, setValidationErrors] = React.useState<string[]>([]);
    const [canRedirectToHome, setCanRedirectToHome] = React.useState<boolean>(true);
    const [shouldRedirectToPreviousPage, setShouldRedirectToPreviousPage] = React.useState<boolean>(isAuthenticated);

    const from = location.state?.from?.pathname ?? '/';

    if (shouldRedirectToPreviousPage) {
        return <Navigate to={from} replace />
    }

    if (isAuthenticated && canRedirectToHome) {
        return <Navigate to="/" replace />
    }

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setCanRedirectToHome(false);

        const data = new FormData(event.currentTarget);

        await withLoadingAsync(setIsLoading, async () => {
            const { isSuccess, isApiError, validationErrors } = await signUpAsync({
                userName: data.get('userName') as string,
                password: data.get('password') as string,
                firstName: data.get('firstName') as string,
                lastName: data.get('lastName') as string
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
        <Container component="main" maxWidth="xs" sx={{ marginTop: 8 }}>
            {isLoading && <TopPageLinearProgress />}
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Avatar sx={{ m: 1, bgcolor: 'secondary.main' }}>
                    <AccountCircleIcon />
                </Avatar>
                <Typography component="h1" variant="h5" sx={{ mb: 2 }}>
                    Sign up
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
                    <Grid container spacing={1.5}>
                        <Grid item xs={12} sm={6}>
                            <TextField
                                id="firstName"
                                name="firstName"
                                label="First Name"
                                required
                                autoComplete="given-name"
                                autoFocus
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12} sm={6}>
                            <TextField
                                id="lastName"
                                name="lastName"
                                label="Last Name"
                                required
                                autoComplete="family-name"
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                id="userName"
                                name="userName"
                                label="User name"
                                required
                                autoComplete="username"
                                helperText="Valid characters are letters, numbers and '-', '.', '_', '@' and '+'"
                                fullWidth
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <TextField
                                type="password"
                                id="password"
                                name="password"
                                label="Password"
                                required
                                autoComplete="new-password"
                                helperText="Minimum number of characters is 6. Password should contain lowercase and uppercase characters as well as numbers and special characters"
                                fullWidth
                            />
                        </Grid>
                    </Grid>
                    <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
                        Sign Up
                    </Button>
                    <Typography variant="body2" align="right">
                        <Link to="/auth/signin" state={{ from: location.state?.from }}>
                            Already have an account? Sign in
                        </Link>
                    </Typography>
                </Box>
            </Box>
        </Container>
    );
}