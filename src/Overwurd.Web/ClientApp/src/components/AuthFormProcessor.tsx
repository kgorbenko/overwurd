import * as React from 'react';
import dayjs from 'dayjs';
import { ISignInResult, useAuth } from '../hooks/use-auth';
import { Navigate } from 'react-router-dom';
import { CenteredAuthForm } from './CenteredAuthForm';
import { withLoadingAsync } from '../utils/misc';
import { TopPageLinearProgress } from './TopPageLinearProgress';

interface IAuthFormProcessorProps {
    title: string;
    onSubmit: (formData: FormData) => Promise<ISignInResult>;
    from: string;
}

export const AuthFormProcessor = (props: React.PropsWithChildren<IAuthFormProcessorProps>) => {
    const now = dayjs.utc();
    const { isAuthenticated } = useAuth(now);

    const [isLoading, setIsLoading] = React.useState<boolean>(false);
    const [isApiError, setIsApiError] = React.useState<boolean>(false);
    const [validationErrors, setValidationErrors] = React.useState<string[]>([]);
    const [canRedirectToHome, setCanRedirectToHome] = React.useState<boolean>(true);
    const [shouldRedirectToPreviousPage, setShouldRedirectToPreviousPage] = React.useState<boolean>(isAuthenticated);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        await withLoadingAsync(setIsLoading, async () => {
            event.preventDefault();
            setCanRedirectToHome(false);

            const formData = new FormData(event.currentTarget);
            const { isSuccess, isApiError, validationErrors } = await props.onSubmit(formData);

            if (isSuccess) {
                setShouldRedirectToPreviousPage(true);
            } else {
                setIsApiError(isApiError);
                setValidationErrors(validationErrors);
            }
        });
    }

    if (!isLoading && shouldRedirectToPreviousPage) {
        return <Navigate to={props.from} replace />;
    }

    if (isAuthenticated && canRedirectToHome) {
        return <Navigate to="/" replace />;
    }

    return (
        <CenteredAuthForm title={props.title} onSubmit={handleSubmit} apiError={isApiError} alerts={validationErrors}>
            {isLoading && <TopPageLinearProgress />}
            {props.children}
        </CenteredAuthForm>
    );
}