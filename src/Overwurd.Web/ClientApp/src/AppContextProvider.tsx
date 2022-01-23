import * as React from 'react';
import { useLocalStorage } from './hooks/UseLocalStorage';
import dayjs, { Dayjs } from 'dayjs';

interface IUserData {
    id: number;
    userName: string;
    firstName: string;
    lastName: string;
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: Dayjs;
}

interface IUserDataRaw {
    id: number;
    userName: string;
    firstName: string;
    lastName: string;
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: string;
}

export interface IAppContext {
    userData: IUserData | undefined;
    setUserData: (userData: (IUserData | undefined)) => void;
}

interface IAppContextData {
    userData: IUserData | undefined;
}

interface IAppContextDataRaw {
    userData: IUserDataRaw | undefined;
}

const emptyContext = {
    userData: undefined,
    setUserData: (_: IUserData | undefined) => { throw new Error('An unassigned AppContext is used.'); }
};
export const Context = React.createContext<IAppContext>(emptyContext);

export const AppContextProvider: React.FunctionComponent = ({ children }: React.PropsWithChildren<{}>) => {
    const [appContextData, setAppContextData] = useLocalStorage<IAppContextData, IAppContextDataRaw>('OverwurdAppContext', emptyContext, (raw: IAppContextDataRaw) => {
        const isUserDataPresent = raw.userData !== undefined;
        const expiresAtRaw = raw.userData?.accessTokenExpiresAt;
        const expiresAt = expiresAtRaw !== undefined ? dayjs.utc(expiresAtRaw) : undefined;

        return {
            userData: isUserDataPresent
                ? {
                    id: raw.userData!.id,
                    userName: raw.userData!.userName,
                    firstName: raw.userData!.firstName,
                    lastName: raw.userData!.lastName,
                    accessToken: raw.userData!.accessToken,
                    refreshToken: raw.userData!.refreshToken,
                    accessTokenExpiresAt: expiresAt!
                }
                : undefined
        }
    });

    function setUserData(userData: IUserData | undefined) {
        setAppContextData({
            ...appContextData,
            userData
        });
    }

    const userData = React.useMemo<IUserData | undefined>(() => {
        return appContextData.userData;
    }, [appContextData]);

    const contextValue = {
        userData: userData,
        setUserData: setUserData
    };

    return (
        <Context.Provider value={contextValue}>
            {children}
        </Context.Provider>
    );
}