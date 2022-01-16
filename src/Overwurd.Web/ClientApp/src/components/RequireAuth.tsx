import * as React from 'react';
import { useAuth } from '../hooks/use-auth';
import { useLocation } from 'react-router-dom';
import { Navigate } from 'react-router-dom';
import { Dayjs } from 'dayjs';

interface IRequireAuthProps {
    children: JSX.Element;
    now: Dayjs;
}

export const RequireAuth = ({ children, now }: IRequireAuthProps) => {
    const { isAuthenticated } = useAuth(now);
    const location = useLocation();

    if (isAuthenticated) {
        return children;
    }

    return <Navigate to="/auth/signin" state={{ from: location }} replace />
}