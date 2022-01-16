import React from 'react';
import { Link } from 'react-router-dom';
import { Context } from '../AppContextProvider';

export const NavMenu = () => {
    const context = React.useContext(Context);

    const userData = context.userData;

    const user =
        userData === undefined
            ? <span>Signed Out</span>
            : <span>Signed In as {userData.firstName} {userData.lastName} #{userData.id}</span>

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
