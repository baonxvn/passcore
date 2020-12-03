import './vendor';

import createMuiTheme from '@material-ui/core/styles/createMuiTheme';
import responsiveFontSizes from '@material-ui/core/styles/responsiveFontSizes';
import ThemeProvider from '@material-ui/styles/ThemeProvider';
import * as React from 'react';
import { render } from 'react-dom';
import { Main } from './Main';

const theme = createMuiTheme({
    palette: {
        error: {
            main: '#F44336',
        },
        primary: {
            main: '#F47F24',
            //main: '#304FF3',
        },
        secondary: {
            main: '#fff',
        },
        text: {
            primary: '#191919',
            secondary: '#000',
        },
    },
    zIndex: {
        appBar: 1201,
    },
});

const passcoreTheme = responsiveFontSizes(theme);

render(
    <ThemeProvider theme={passcoreTheme}>
        <Main />
    </ThemeProvider>,
    document.getElementById('rootNode'),
);
