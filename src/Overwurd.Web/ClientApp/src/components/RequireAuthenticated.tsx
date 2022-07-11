import * as React from 'react';
import dayjs from 'dayjs';
import { useAuth } from '../hooks/UseAuth';
import { useLocation } from 'react-router-dom';
import { Navigate } from 'react-router-dom';
import { CenteredCircularProgress } from './CenteredCircularProgress';

export interface IRequireAuthenticatedProps {
    navigateTo: string;
}

export const RequireAuthenticated: React.FunctionComponent<IRequireAuthenticatedProps> =
    (props: React.PropsWithChildren<IRequireAuthenticatedProps>) => {
        const { isAuthenticated } = useAuth(dayjs().utc());
        const [isLoading, setIsLoading] = React.useState(true);
        const [shouldNavigate, setShouldNavigate] = React.useState(false);
        const location = useLocation();

        React.useEffect(() => {
            if (!isAuthenticated) {
                setShouldNavigate(true);
            }
            setIsLoading(false);
        // eslint-disable-next-line react-hooks/exhaustive-deps
        }, []);

        if (isLoading) {
            return <CenteredCircularProgress />;
        }

        if (shouldNavigate) {
            return <Navigate to={props.navigateTo} state={{ from: location }} replace />;
        }

        return <>{props.children}</>;
    };