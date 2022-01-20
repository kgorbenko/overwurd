import * as React from 'react';
import * as dayjs from 'dayjs';
import ReactDOM from 'react-dom';
import utc from 'dayjs/plugin/utc'
import { App } from './App';

import './index.css';

import '@fontsource/roboto/300.css';
import '@fontsource/roboto/400.css';
import '@fontsource/roboto/500.css';
import '@fontsource/roboto/700.css';

const rootElement = document.getElementById('root');

dayjs.extend(utc);

ReactDOM.render(
    <App />,
    rootElement);
