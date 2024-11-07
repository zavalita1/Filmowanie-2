import * as React from 'react';
import { connect } from 'react-redux';
import { Container } from 'reactstrap';
import NavMenu from './NavMenu';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import { Outlet } from 'react-router-dom';

import Box from '@mui/material/Box';
import CssBaseline   from '@mui/material/CssBaseline';
import { ApplicationState } from '../store';

interface ILayoutProps {
    theme: string;
}

function Layout(props: ILayoutProps): React.JSX.Element {
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
            <>
                <ThemeProvider theme={theme}>
                    <Box>
                        <CssBaseline />
                        <div id="theme-container" className={themeContainerClassName}>
                            <NavMenu />
                            <Container>
                                <Outlet />
                            </Container>
                        </div>
                    </Box>
                </ThemeProvider>
            </>
        );
}

export default connect(
    (state: ApplicationState) => ({ theme: state.app?.theme }),
)(Layout);
