import * as React from 'react';
import { Button, Grid, TextField, Typography } from '@mui/material';
import { Link, useLocation } from 'react-router-dom';
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
                <Grid item xs={12}>
                    <TextField
                        type="password"
                        id="confirmPassword"
                        name="confirmPassword"
                        label="Confirm password"
                        required
                        autoComplete="new-password"
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
        </AuthFormProcessor>
    );
}