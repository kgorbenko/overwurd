import * as React from 'react';
import dayjs from 'dayjs';
import { Button, TextField, Link } from '@mui/material';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import { ISignInResult, useAuth } from '../hooks/UseAuth';
import { AuthFormProcessor } from '../components/AuthFormProcessor';
import { AuthRegionRoutes } from '../AuthRegion';

interface ISignInData {
    userName: string;
    password: string;
}

export interface ISignInProps {
    defaultRedirectPath: string;
}

export const SignInPage: React.FunctionComponent<ISignInProps> =
    (props: ISignInProps) => {
        const { signInAsync } = useAuth(dayjs.utc());
        const location = useLocation();

        const from = location.state?.from?.pathname ?? props.defaultRedirectPath;

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
                    variant="filled"
                    margin="dense"
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
                    variant="filled"
                    autoComplete="current-password"
                    margin="dense"
                    size="small"
                    fullWidth
                />
                <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
                    Sign In
                </Button>
                <Link component={RouterLink as any} to={AuthRegionRoutes.signUpRoute} state={{ from: location.state?.from }}>
                    Don't have an account? Sign Up
                </Link>
            </AuthFormProcessor>
        );
    };