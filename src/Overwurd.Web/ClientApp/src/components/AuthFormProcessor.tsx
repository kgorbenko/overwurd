import * as React from 'react';
import dayjs from 'dayjs';
import { ISignInResult, useAuth } from '../hooks/UseAuth';
import { Navigate } from 'react-router-dom';
import { AuthFormContainer } from './AuthFormContainer';
import { withLoadingAsync } from '../utils/Misc';
import { TopPageLinearProgress } from './TopPageLinearProgress';

export interface IValidationResult {
    isValid: boolean;
    errors: string[];
}

interface IAuthFormProcessorProps<TData> {
    title: string;
    from: string;
    getDataFromForm: (formData: FormData) => TData;
    onSubmit: (data: TData) => Promise<ISignInResult>;
    validate?: (data: TData) => IValidationResult;
}

export const AuthFormProcessor = <T extends object>({
    title,
    from,
    getDataFromForm,
    validate,
    onSubmit,
    children
}: React.PropsWithChildren<IAuthFormProcessorProps<T>>) => {
    const now = dayjs.utc();
    const { isAuthenticated } = useAuth(now);

    const [isLoading, setIsLoading] = React.useState<boolean>(false);
    const [isApiError, setIsApiError] = React.useState<boolean>(false);
    const [validationErrors, setValidationErrors] = React.useState<string[]>([]);
    const [shouldRedirectToPreviousPage, setShouldRedirectToPreviousPage] = React.useState<boolean>(isAuthenticated);

    const handleSignIn = async (data: T) => {
        const { isSuccess, isApiError, validationErrors } = await onSubmit(data);

        if (isSuccess) {
            setShouldRedirectToPreviousPage(true);
        } else {
            setIsApiError(isApiError);
            setValidationErrors(validationErrors);
        }
    }

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        await withLoadingAsync(setIsLoading, async () => {
            event.preventDefault();

            const formData = new FormData(event.currentTarget);
            const data = getDataFromForm(formData);

            const validationResult = validate !== undefined
                ? validate(data)
                : { isValid: true, errors: [] };

            if (!validationResult.isValid) {
                setValidationErrors(validationResult.errors);
            } else {
                await handleSignIn(data);
            }
        });
    }

    if (!isLoading && shouldRedirectToPreviousPage) {
        return <Navigate to={from} replace />;
    }

    return (
        <AuthFormContainer title={title} onSubmit={handleSubmit} apiError={isApiError} alerts={validationErrors}>
            {isLoading && <TopPageLinearProgress />}
            {children}
        </AuthFormContainer>
    );
}