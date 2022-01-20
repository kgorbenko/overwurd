import * as React from 'react';
import dayjs from 'dayjs';
import { Button, TextField, Typography } from '@mui/material';
import { Link, useLocation } from 'react-router-dom';
import { ISignInResult, useAuth } from '../hooks/use-auth';
import { AuthFormProcessor } from '../components/AuthFormProcessor';

interface ISignInData {
    userName: string;
    password: string;
}

export const SignInPage = () => {
    const { signInAsync } = useAuth(dayjs.utc());
    const location = useLocation();

    const from = location.state?.from?.pathname ?? '/';

    const getDataFromForm = (formData: FormData): ISignInData => ({
        userName: formData.get('userName') as string,
        password: formData.get('password') as string
    });

    const onSubmit = async (signInData: ISignInData): Promise<ISignInResult> => {
        return await signInAsync(signInData);
    }

    return (
        <AuthFormProcessor title="Sign In" onSubmit={onSubmit} getDataFromForm={getDataFromForm} from={from}>
            <TextField
                required
                label="User name"
                id="userName"
                name="userName"
                autoComplete="username"
                margin="normal"
                size="small"
                autoFocus
                fullWidth
            />
            <TextField
                required
                type="password"
                label="Password"
                id="password"
                name="password"
                autoComplete="current-password"
                margin="normal"
                size="small"
                fullWidth
            />
            <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
                Sign In
            </Button>
            <Typography variant="body2" align="right">
                <Link to="/auth/signup" state={{ from: location.state?.from }}>
                    Don't have an account? Sign Up
                </Link>
            </Typography>
        </AuthFormProcessor>
    );
}