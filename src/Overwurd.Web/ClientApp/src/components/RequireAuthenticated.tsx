import * as React from 'react';
import { useAuth } from '../hooks/use-auth';
import { NavigateOptions, useLocation } from 'react-router-dom';
import { CenteredCircularProgress } from './CenteredCircularProgress';
import dayjs from 'dayjs';

interface IRequireAuthenticatedProps {
    onUnauthenticated: (options?: NavigateOptions) => void;
}

export const RequireAuthenticated: React.FunctionComponent<IRequireAuthenticatedProps> = ({
    onUnauthenticated,
    children,
}: React.PropsWithChildren<IRequireAuthenticatedProps>) => {
    const { isAuthenticated } = useAuth(dayjs().utc());
    const [isLoading, setIsLoading] = React.useState(true);
    const location = useLocation();

    React.useEffect(() => {
        if (!isAuthenticated) {
            onUnauthenticated({
                state: { from: location },
                replace: true
            });
        }
        setIsLoading(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    if (isLoading) {
        return <CenteredCircularProgress />;
    }

    return <>{children}</>;
}