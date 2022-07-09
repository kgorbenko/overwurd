import * as React from 'react';
import { Context } from '../AppContextProvider';
import {
    signInAsync as signInInternalAsync,
    signUpAsync as signUpInternalAsync,
    refreshAccessTokenAsync as refreshAccessTokenInternalAsync,
    ISignInParameters,
    ISignUpParameters,
    IRefreshAccessTokenParameters,
    ISignInResult as SignInServiceResult,
} from '../services/AuthService';
import { Dayjs } from 'dayjs';

export interface ISignInResult {
    isSuccess: boolean;
    isApiError: boolean;
    validationErrors: string[];
}

export function useAuth(now: Dayjs) {
    const context = React.useContext(Context);

    const expirationDate = React.useMemo(() => {
        return context.userData?.accessTokenExpiresAt;
    }, [context.userData?.accessTokenExpiresAt]);

    const hasTokenExpirationDate = React.useMemo(() => {
        return expirationDate !== undefined;
    }, [expirationDate]);

    const isAuthenticated = React.useMemo(() => {
        return hasTokenExpirationDate && expirationDate!.isAfter(now);
    }, [hasTokenExpirationDate, expirationDate, now]);

    const canRefreshAccessToken = React.useMemo(() => {
        const isTokenExpired = hasTokenExpirationDate && now.isAfter(expirationDate!);
        const isWithinRefreshRange = hasTokenExpirationDate && expirationDate!.diff(now, 'minutes') < 3;
        return isTokenExpired || isWithinRefreshRange;
    }, [expirationDate, hasTokenExpirationDate, now]);

    async function signInAsync(parameters: ISignInParameters): Promise<ISignInResult> {
        const result = await signInInternalAsync(parameters);
        return processServiceResult(result);
    }

    async function signUpAsync(parameters: ISignUpParameters): Promise<ISignInResult> {
        const result = await signUpInternalAsync(parameters);
        return processServiceResult(result);
    }

    function signOutAsync(): void {
        context.setUserData(undefined);
    }

    async function refreshAccessTokenAsync(): Promise<boolean> {
        const parameters: IRefreshAccessTokenParameters = {
            accessToken: context.userData!.accessToken,
            refreshToken: context.userData!.refreshToken
        };
        const result = await refreshAccessTokenInternalAsync(parameters);

        if (result === undefined) {
            context.setUserData(undefined);
            return false;
        }

        context.setUserData({
            ...context.userData!,
            accessToken: result.accessToken,
            accessTokenExpiresAt: result.expiresAt
        });

        return true;
    }

    function processServiceResult(result: SignInServiceResult | undefined) {
        if (result === undefined) {
            return {
                isSuccess: false,
                isApiError: true,
                validationErrors: []
            };
        }

        if (!result.isSuccess) {
            return {
                isSuccess: false,
                isApiError: false,
                validationErrors: result.signInErrors!
            }
        }

        const userData = result.signInResult!;
        context.setUserData({
            id: userData.id,
            userName: userData.userName,
            firstName: userData.firstName,
            lastName: userData.lastName,
            accessToken: userData.accessToken,
            refreshToken: userData.refreshToken,
            accessTokenExpiresAt: userData.accessTokenExpiresAt
        });

        return {
            isSuccess: true,
            isApiError: false,
            validationErrors: []
        };
    }

    return {
        isAuthenticated,
        canRefreshAccessToken,
        signUpAsync,
        signInAsync,
        signOutAsync,
        refreshAccessTokenAsync
    };
}