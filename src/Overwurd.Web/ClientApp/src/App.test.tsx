import * as React from 'react';
import * as dayjs from 'dayjs';
import ReactDOM from 'react-dom';
import utc from 'dayjs/plugin/utc';
import { MemoryRouter } from 'react-router-dom';
import { App } from './App';
import { AppContextProvider } from './AppContextProvider';
import { findByText } from '@testing-library/react';
import '@testing-library/jest-dom';


it('renders without crashing', async () => {
    dayjs.extend(utc);
    const div = document.createElement('div');
    ReactDOM.render(
        <MemoryRouter>
            <AppContextProvider>
                <App />
            </AppContextProvider>
        </MemoryRouter>, div);
    const hello = await findByText(div, 'Hello, world! Testing new version.');
    expect(hello).toBeTruthy();
});
