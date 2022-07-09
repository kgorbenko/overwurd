import * as React from 'react';
import * as dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { render } from 'react-dom';
import { findAllByText } from '@testing-library/react';
import { Overwurd } from './Overwurd';
import { MemoryRouter } from 'react-router-dom';
import { AppContextProvider } from './AppContextProvider';

import '@testing-library/jest-dom';

it('renders without crashing', async () => {
    dayjs.extend(utc);
    const div = document.createElement('div');
    render(
        <MemoryRouter>
            <AppContextProvider>
                <Overwurd />
            </AppContextProvider>
        </MemoryRouter>, div);
    const hello = await findAllByText(div, 'Overwurd');
    expect(hello.length).toBeGreaterThan(0);
});
