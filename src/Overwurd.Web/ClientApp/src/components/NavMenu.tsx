import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/use-auth';
import { Dayjs } from 'dayjs';
import { Context } from '../AppContextProvider';

interface INavMenuProps {
    now: Dayjs;
}

export const NavMenu: React.FunctionComponent<INavMenuProps> = ({ now }: React.PropsWithChildren<INavMenuProps>) => {
    const context = React.useContext(Context);
    const { isAuthenticated } = useAuth(now);

    const user =
        isAuthenticated
            ? <span>Signed In as {context.userData!.firstName} {context.userData!.lastName} #{context.userData!.id}</span>
            : <span>Signed Out</span>;

    return (
        <header>
            <p><Link to="/">Home</Link></p>
            <p><Link to="/protected">Protected</Link></p>
            <p><Link to="/auth/signin">Sign In</Link></p>
            <p><Link to="/auth/signup">Sign Up</Link></p>
            <p><Link to="/auth/signout">Sign Out</Link></p>
            <p>{user}</p>
        </header>
    );
}
