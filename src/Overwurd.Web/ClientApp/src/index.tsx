import * as React from 'react';
import * as dayjs from 'dayjs';
import ReactDOM from 'react-dom';
import utc from 'dayjs/plugin/utc'
import { BrowserRouter } from 'react-router-dom';
import { App } from './App';
import { AppContextProvider } from './AppContextProvider';

import './index.css';

import '@fontsource/roboto/300.css';
import '@fontsource/roboto/400.css';
import '@fontsource/roboto/500.css';
import '@fontsource/roboto/700.css';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href') ?? undefined;
const rootElement = document.getElementById('root');

dayjs.extend(utc);

ReactDOM.render(
    <BrowserRouter basename={baseUrl}>
        <AppContextProvider>
            <App />
        </AppContextProvider>
    </BrowserRouter>,
    rootElement);
