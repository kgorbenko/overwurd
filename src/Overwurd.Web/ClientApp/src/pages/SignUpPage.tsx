import * as React from 'react';
import dayjs from 'dayjs';
import { Button, TextField, Link } from '@mui/material';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import { ISignInResult, useAuth } from '../hooks/UseAuth';
import { AuthFormProcessor, IValidationResult } from '../components/AuthFormProcessor';
import { ISignUpParameters } from '../services/AuthService';
import { AuthRegionRoutes } from '../AuthRegion';

interface ISignUpData {
    login: string;
    password: string;
    confirmPassword: string;
}

interface ISignInProps {
    defaultRedirectPath?: string;
}

export const SignUpPage: React.FunctionComponent<ISignInProps> =
    (props: ISignInProps) => {
        const now = dayjs.utc();
        const { signUpAsync } = useAuth(now);
        const location = useLocation();

        const from = location.state?.from?.pathname ?? props.defaultRedirectPath;

        const getDataFromForm = (formData: FormData): ISignUpData => ({
            login: formData.get('login') as string,
            password: formData.get('password') as string,
            confirmPassword: formData.get('confirmPassword') as string
        });

        const validate = (signUpData: ISignUpData): IValidationResult => {
            return signUpData.password === signUpData.confirmPassword
                ? { isValid: true, errors: [] }
                : { isValid: false, errors: ['Entered passwords do not match'] }
        }

        const onSubmit = async (signUpData: ISignUpData): Promise<ISignInResult> => {
            const signUpParameters: ISignUpParameters = {
                login: signUpData.login,
                password: signUpData.password
            };
            return await signUpAsync(signUpParameters);
        }

        return (
            <AuthFormProcessor title="Sign Up" onSubmit={onSubmit} getDataFromForm={getDataFromForm} validate={validate} from={from}>
                <TextField
                    id="login"
                    name="login"
                    label="Login"
                    required
                    autoComplete="username"
                    variant="filled"
                    helperText="Login should be between 8 and 30 characters. Use only letters, digits, -, ., _"
                    size="small"
                    margin="dense"
                    fullWidth
                />
                <TextField
                    type="password"
                    id="password"
                    name="password"
                    label="Password"
                    required
                    autoComplete="new-password"
                    variant="filled"
                    helperText="Use at least 8 characters. Both uppercase and lowercase characters are required"
                    size="small"
                    margin="dense"
                    fullWidth
                />
                <TextField
                    type="password"
                    id="confirmPassword"
                    name="confirmPassword"
                    label="Confirm password"
                    required
                    variant="filled"
                    autoComplete="new-password"
                    size="small"
                    margin="dense"
                    fullWidth
                />
                <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
                    Sign Up
                </Button>
                <Link component={RouterLink as any} to={AuthRegionRoutes.signInAbsoluteRoute} state={{ from: location.state?.from }}>
                    Already have an account? Sign in
                </Link>
            </AuthFormProcessor>
        );
    };