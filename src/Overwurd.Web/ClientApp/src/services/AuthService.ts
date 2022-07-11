import dayjs, { Dayjs } from 'dayjs';

export interface ISignInResult {
    isSuccess: boolean;
    signInResult: IAuthData | undefined;
    signInErrors: string[] | undefined;
}

export interface ISignInParameters {
    login: string;
    password: string;
}

export interface ISignUpParameters {
    login: string;
    password: string;
}

export interface IRefreshAccessTokenParameters {
    accessToken: string;
    refreshToken: string;
}

interface IAuthData {
    id: number;
    login: string;
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: Dayjs;
}

interface IAuthDataRaw {
    id: number;
    login: string;
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: string;
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
            signInErrors: (await response.json() as any).errors
        }
    }

    return undefined;
}

export async function refreshAccessTokenAsync(parameters: IRefreshAccessTokenParameters): Promise<IAuthData | undefined> {
    try {
        const response = await fetch(refreshUrl, {
            method: 'POST',
            body: JSON.stringify(parameters),
            headers: {
                'Content-Type': 'application/json'
            }
        });

        return response.ok
            ? mapRawSignInData(await response.json())
            : undefined;
    } catch {
        return undefined;
    }
}

function mapRawSignInData(rawData: IAuthDataRaw): IAuthData {
    return {
        id: rawData.id,
        login: rawData.login,
        refreshToken: rawData.refreshToken,
        accessToken: rawData.accessToken,
        accessTokenExpiresAt: dayjs(rawData.accessTokenExpiresAt)
    };
}