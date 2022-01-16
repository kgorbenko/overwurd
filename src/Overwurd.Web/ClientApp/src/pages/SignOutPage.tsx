import * as React from 'react';
import { useAuth } from '../hooks/use-auth';
import { Navigate } from 'react-router-dom';
import { CenteredCircularProgress } from '../components/CenteredCircularProgress';
import dayjs from 'dayjs';

export const SignOutPage = () => {
    const now = dayjs.utc();
    const { isAuthenticated, signOutAsync } = useAuth(now);
    const [shouldRedirectToHome, setShouldRedirectToHome] = React.useState<boolean>(false);

    React.useEffect(() => {
        if (isAuthenticated) {
            signOutAsync();
        }
        setShouldRedirectToHome(true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    if (shouldRedirectToHome) {
        return <Navigate to="/" replace />;
    }

    return <CenteredCircularProgress />;
}