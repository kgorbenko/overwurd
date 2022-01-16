import dayjs, { Dayjs } from 'dayjs';
import { getModelErrors } from '../utils/misc';

export interface ISignInResult {
    isSuccess: boolean;
    signInResult: ISignInData | undefined;
    signInErrors: string[] | undefined;
}

export interface ISignInParameters {
    userName: string;
    password: string;
}

interface ISignInData {
    id: number;
    userName: string;
    firstName: string;
    lastName: string;
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: Dayjs;
}

interface ISignInDataRaw {
    id: number;
    userName: string;
    firstName: string;
    lastName: string;
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: string;
}

export interface ISignUpParameters {
    userName: string;
    password: string;
    firstName: string;
    lastName: string;
}

export interface IRefreshAccessTokenParameters {
    accessToken: string;
    refreshToken: string;
}

interface IRefreshAccessTokenResultRaw {
    accessToken: string;
    expiresAt: string;
}

export interface IRefreshAccessTokenResult {
    accessToken: string;
    expiresAt: Dayjs;
}

const signUpUrl = '/api/auth/signup';
const signInUrl = '/api/auth/signin';
const refreshUrl = '/api/auth/refresh';

export async function signInAsync(parameters: ISignInParameters): Promise<ISignInResult | undefined> {
    try {
        return await processSignInAsync(signInUrl, parameters);
    } catch {
        return undefined;
    }
}

export async function signUpAsync(parameters: ISignUpParameters): Promise<ISignInResult | undefined> {
    try {
        return await processSignInAsync(signUpUrl, parameters);
    } catch {
        return undefined;
    }
}

async function processSignInAsync(url: string, body: object): Promise<ISignInResult | undefined> {
    const response = await fetch(url, {
        method: 'POST',
        body: JSON.stringify(body),
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        return {
            isSuccess: true,
            signInResult: mapRawSignInData(await response.json()),
            signInErrors: undefined
        };
    }

    if (response.status === 400) {
        return {
            isSuccess: false,
            signInResult: undefined,
            signInErrors: getModelErrors(await response.json())
        }
    }

    return undefined;
}

export async function refreshAccessTokenAsync(parameters: IRefreshAccessTokenParameters): Promise<IRefreshAccessTokenResult | undefined> {
    try {
        const response = await fetch(refreshUrl, {
            method: 'POST',
            body: JSON.stringify(parameters),
            headers: {
                'Content-Type': 'application/json'
            }
        });

        return response.ok
            ? mapRawRefreshAccessTokenResult(await response.json())
            : undefined;
    } catch {
        return undefined;
    }
}

function mapRawSignInData(resultRaw: ISignInDataRaw): ISignInData {
    return {
        id: resultRaw.id,
        userName: resultRaw.userName,
        firstName: resultRaw.firstName,
        lastName: resultRaw.lastName,
        accessToken: resultRaw.accessToken,
        refreshToken: resultRaw.refreshToken,
        accessTokenExpiresAt: dayjs(resultRaw.accessTokenExpiresAt)
    };
}

function mapRawRefreshAccessTokenResult({ accessToken, expiresAt }: IRefreshAccessTokenResultRaw): IRefreshAccessTokenResult {
    return {
        accessToken: accessToken,
        expiresAt: dayjs(expiresAt)
    };
}