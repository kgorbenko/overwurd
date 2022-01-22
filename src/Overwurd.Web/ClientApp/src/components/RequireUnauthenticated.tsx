import * as React from 'react';
import { useAuth } from '../hooks/use-auth';
import { Navigate } from 'react-router-dom';
import dayjs from 'dayjs';
import { CenteredCircularProgress } from './CenteredCircularProgress';

interface IRequireAuthProps {
    navigateTo: string;
}

export const RequireUnauthenticated: React.FunctionComponent<IRequireAuthProps> = ({
    navigateTo,
    children,
}: React.PropsWithChildren<IRequireAuthProps>) => {
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
        return <Navigate to={navigateTo} replace />;
    }

    return <>{children}</>;
}