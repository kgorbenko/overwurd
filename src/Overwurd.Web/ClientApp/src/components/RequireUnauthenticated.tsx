import * as React from 'react';
import dayjs from 'dayjs';
import { useAuth } from '../hooks/UseAuth';
import { Navigate } from 'react-router-dom';
import { CenteredCircularProgress } from './CenteredCircularProgress';

export interface IRequireUnauthenticatedProps {
    navigateTo: string;
}

export const RequireUnauthenticated: React.FunctionComponent<IRequireUnauthenticatedProps> =
    (props: React.PropsWithChildren<IRequireUnauthenticatedProps>) => {
        const { isAuthenticated } = useAuth(dayjs().utc());
        const [isLoading, setIsLoading] = React.useState(true);
        const [shouldNavigate, setShouldNavigate] = React.useState<boolean>(false);

        React.useEffect(() => {
            if (isAuthenticated) {
                setShouldNavigate(true);
            }
            setIsLoading(false);
        // eslint-disable-next-line react-hooks/exhaustive-deps
        }, []);

        if (isLoading) {
            return <CenteredCircularProgress />;
        }

        if (shouldNavigate) {
            return <Navigate to={props.navigateTo} replace />;
        }

        return <>{props.children}</>;
    };