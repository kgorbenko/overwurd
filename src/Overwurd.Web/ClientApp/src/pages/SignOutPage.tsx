import * as React from 'react';
import { useAuth } from '../hooks/UseAuth';
import { CenteredCircularProgress } from '../components/CenteredCircularProgress';
import dayjs from 'dayjs';

interface ISignOutPageProps {
    onPostSignOut : () => void;
}

export const SignOutPage: React.FunctionComponent<ISignOutPageProps> = ({
    onPostSignOut,
}: ISignOutPageProps) => {
    const now = dayjs.utc();
    const { signOutAsync } = useAuth(now);

    React.useEffect(() => {
        (async () => {
            await signOutAsync();
            onPostSignOut();
        })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return <CenteredCircularProgress />;
}