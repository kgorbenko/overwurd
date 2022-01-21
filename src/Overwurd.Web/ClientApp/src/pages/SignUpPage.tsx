import * as React from 'react';
import { Button, Grid, TextField, Link } from '@mui/material';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import { ISignInResult, useAuth } from '../hooks/use-auth';
import dayjs from 'dayjs';
import { AuthFormProcessor, IValidationResult } from '../components/AuthFormProcessor';
import { ISignUpParameters } from '../services/auth-service';

interface ISignUpData {
    userName: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
}

export const SignUpPage = () => {
    const now = dayjs.utc();
    const { signUpAsync } = useAuth(now);
    const location = useLocation();

    const from = location.state?.from?.pathname ?? '/';

    const getDataFromForm = (formData: FormData): ISignUpData => ({
        userName: formData.get('userName') as string,
        password: formData.get('password') as string,
        confirmPassword: formData.get('confirmPassword') as string,
        firstName: formData.get('firstName') as string,
        lastName: formData.get('lastName') as string
    });

    const validate = (signUpData: ISignUpData): IValidationResult => {
        return signUpData.password === signUpData.confirmPassword
            ? { isValid: true, errors: [] }
            : { isValid: false, errors: ['Entered passwords do not match'] }
    }

    const onSubmit = async (signUpData: ISignUpData): Promise<ISignInResult> => {
        const signUpParameters: ISignUpParameters = {
            userName: signUpData.userName,
            password: signUpData.password,
            firstName: signUpData.firstName,
            lastName: signUpData.lastName
        };
        return await signUpAsync(signUpParameters);
    }

    return (
        <AuthFormProcessor title="Sign Up" onSubmit={onSubmit} getDataFromForm={getDataFromForm} validate={validate} from={from}>
            <Grid container spacing={1.5}>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="firstName"
                        name="firstName"
                        label="First Name"
                        required
                        variant="filled"
                        autoComplete="given-name"
                        autoFocus
                        size="small"
                        fullWidth
                    />
                </Grid>
                <Grid item xs={12} sm={6}>
                    <TextField
                        id="lastName"
                        name="lastName"
                        label="Last Name"
                        required
                        variant="filled"
                        autoComplete="family-name"
                        size="small"
                        fullWidth
                    />
                </Grid>
                <Grid item xs={12} style={{ paddingTop: '6px' }}>
                    <TextField
                        id="userName"
                        name="userName"
                        label="User name"
                        required
                        autoComplete="username"
                        variant="filled"
                        helperText="Use only letters, digits, -, ., _, @ and +"
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
                        // helperText="Minimum number of characters is 6. Password should contain lowercase and uppercase characters as well as numbers and special characters"
                        helperText="Use at least 6 characters. Uppercase, lowercase, digits and special characters are required"
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
                </Grid>
            </Grid>
            <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
                Sign Up
            </Button>
            <Link component={RouterLink as any} to="/auth/signin" state={{ from: location.state?.from }}>
                Already have an account? Sign in
            </Link>
        </AuthFormProcessor>
    );
}