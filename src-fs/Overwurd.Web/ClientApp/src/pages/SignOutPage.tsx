import * as React from 'react';
import dayjs from 'dayjs';
import { useAuth } from '../hooks/UseAuth';
import { CenteredCircularProgress } from '../components/CenteredCircularProgress';

export interface ISignOutPageProps {
    onPostSignOut : () => void;
}

export const SignOutPage: React.FunctionComponent<ISignOutPageProps> =
    (props: ISignOutPageProps) => {
        const now = dayjs.utc();
        const { signOutAsync } = useAuth(now);

        React.useEffect(() => {
            (async () => {
                await signOutAsync();
                props.onPostSignOut();
            })();
        // eslint-disable-next-line react-hooks/exhaustive-deps
        }, []);

        return <CenteredCircularProgress />;
    };