import * as React from 'react';
import { connect } from 'react-redux';
import { Container } from 'reactstrap';
import NavMenu from './NavMenu';
import { createTheme, ThemeProvider } from '@mui/material/styles';

import Box from '@mui/material/Box';
import CssBaseline   from '@mui/material/CssBaseline';
import { ApplicationState } from '../store';

function Layout(props: any) {
    const darkTheme = createTheme({
        typography: { fontFamily: "Lora" },
        palette: { mode: "dark" }
    });

    const lightTheme = createTheme({
        typography: { fontFamily: "Lora" }
    });

    const [theme, themeContainerClassName] = props.theme === 'dark' 
    ? [darkTheme, 'dark'] 
    : [lightTheme, ''];

        return (
            <React.Fragment>
                <ThemeProvider theme={theme}>
                    <Box>
                        <CssBaseline />
                        <div id="theme-container" className={themeContainerClassName}>
                            <NavMenu />
                            <Container>
                                {props.children}
                            </Container>
                        </div>
                    </Box>
                </ThemeProvider>
            </React.Fragment>
        );
}

export default connect(
    (state: ApplicationState) => ({ theme: state.app?.theme }),
    // CounterStore.actionCreators
)(Layout as any);
