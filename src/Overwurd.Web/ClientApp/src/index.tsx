import * as React from 'react';
import * as dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc'
import { render } from 'react-dom';
import { App } from './App';

import './index.css';

import '@fontsource/roboto/300.css';
import '@fontsource/roboto/400.css';
import '@fontsource/roboto/500.css';
import '@fontsource/roboto/700.css';

dayjs.extend(utc);

const rootElement = document.getElementById('root');
render(<App />, rootElement);