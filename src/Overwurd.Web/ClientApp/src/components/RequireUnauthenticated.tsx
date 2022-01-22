import * as React from 'react';
import dayjs from 'dayjs';
import { useAuth } from '../hooks/use-auth';
import { CenteredCircularProgress } from './CenteredCircularProgress';
import { NavigateOptions } from 'react-router-dom';

interface IRequireAuthProps {
    onAuthenticated: (options?: NavigateOptions) => void;
}

export const RequireUnauthenticated: React.FunctionComponent<IRequireAuthProps> = ({
    onAuthenticated,
    children,
}: React.PropsWithChildren<IRequireAuthProps>) => {
    const { isAuthenticated } = useAuth(dayjs().utc());
    const [isLoading, setIsLoading] = React.useState(true);

    React.useEffect(() => {
        if (isAuthenticated) {
            onAuthenticated();
        }
        setIsLoading(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    if (isLoading) {
        return <CenteredCircularProgress />;
    }

    return <>{children}</>;
}