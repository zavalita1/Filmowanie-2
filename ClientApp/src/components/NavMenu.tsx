import * as React from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './css/NavMenu.css';
import { connect } from 'react-redux';
import { ApplicationState } from '../store';
import { VotingState } from '../store/votingState';
import { IUser } from '../store/User';
import { Typography } from '@mui/material';
import * as App from '../store/App';
import * as User from '../store/User';
import { styled } from '@mui/material/styles';
import Switch from '@mui/material/Switch';

type NavMenuProps = User.UserState &
typeof App.actionCreators &
typeof User.actionCreators &
{ state: VotingState, isMobile?: boolean } &
{ theme?: App.SupportedTheme}

const NavBarTheme = React.createContext<App.SupportedTheme | undefined>(undefined);

function NavMenu(props: NavMenuProps) {
    const [isOpen, setIsOpen] = React.useState(false);
 
    const toggle = () => {
        setIsOpen(!isOpen);
    }

    return (
            <header>
                <Navbar className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3" light>
                    <Container>
                    <NavbarBrand tag={Link} to="/"><Typography variant='h4' id="Topbar_Name" className={props.theme}>Filmowanie</Typography></NavbarBrand>
                    <MaterialUISwitch sx={{ m: 1 }} defaultChecked={props.theme ==='dark'} onChange={changeTheme(props)} />
                    <NavbarToggler onClick={toggle} className={`mr-2 ${props.theme}`}/>
                        <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={isOpen} navbar>
                            <ul className="navbar-nav flex-grow">
                                <NavBarTheme.Provider value={props.theme}>
                                <NavItemWrapper to="/" desc='Home' user={props.user}></NavItemWrapper>
                                <NavItemWrapper to="/movies-list" desc='Lista filmów' user={props.user} isEnabled={isListEnabled(props)}></NavItemWrapper>
                                <NavItemWrapper to="/results" desc='Wyniki' user={props.user} isEnabled={isResultsEnabled(props)}></NavItemWrapper>
                                <NavItemWrapper to="/nominate" desc='Nominuj' user={props.user} isEnabled={isNominateEnabled(props)}></NavItemWrapper>
                                <NavItemWrapper to="/admin" desc='Admin Panel' user={props.user}></NavItemWrapper>
                                <NavItemWrapper to="/history" desc='Historia' user={props.user} isEnabled={isHistoryEnabled(props)}></NavItemWrapper>
                                <NavItemWrapper to="/" desc='Wyloguj' user={props.user} isEnabled={isLogoutEnabled(props)} onClick={() => props.logOut()}></NavItemWrapper>
                                </NavBarTheme.Provider>
                            </ul>
                        </Collapse>
                    </Container>
                </Navbar>
            </header>
        );

function NavItemWrapper(itemProps: {to:string, desc: string, isEnabled?: boolean, user?: IUser, onClick?: () => void}) {
    const theme = React.useContext(NavBarTheme);

    if (itemProps.isEnabled === true || itemProps.user?.isAdmin === true) {
      return <NavItem>
        <NavLink tag={Link} className="text-dark" to={itemProps.to} onClick={onClick}><Typography className={`Topbar_MenuItem ${theme}`}>{itemProps.desc}</Typography></NavLink>
            </NavItem>
    }

    return <></>;

    function onClick() {
      if (props.isMobile) {
        toggle();
      }

      if (itemProps.onClick !== undefined) {
        itemProps.onClick()
      }
      
    }
}
}

function isListEnabled(props: NavMenuProps) {
    return props.user !== undefined && props.state !== VotingState.Results;
}

function isResultsEnabled(props: NavMenuProps) {
    return props.state === VotingState.Results;
}

function isNominateEnabled(props: NavMenuProps) {
    return props.state !== VotingState.Results && props.user?.hasNominations === true;
}

function isHistoryEnabled(props: NavMenuProps) {
  return props.user !== undefined;
}

function isLogoutEnabled(props: NavMenuProps) {
  return props.user !== undefined;
}

const MaterialUISwitch = styled(Switch)(({ theme }) => ({
    width: 62,
    height: 34,
    padding: 7,
    '& .MuiSwitch-switchBase': {
      margin: 1,
      padding: 0,
      transform: 'translateX(6px)',
      '&.Mui-checked': {
        color: '#fff',
        transform: 'translateX(22px)',
        '& .MuiSwitch-thumb:before': {
          backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="20" width="20" viewBox="0 0 20 20"><path fill="${encodeURIComponent(
            '#fff',
          )}" d="M4.2 2.5l-.7 1.8-1.8.7 1.8.7.7 1.8.6-1.8L6.7 5l-1.9-.7-.6-1.8zm15 8.3a6.7 6.7 0 11-6.6-6.6 5.8 5.8 0 006.6 6.6z"/></svg>')`,
        },
        '& + .MuiSwitch-track': {
          opacity: 1,
          backgroundColor: theme.palette.mode === 'dark' ? '#8796A5' : '#aab4be',
        },
      },
    },
    '& .MuiSwitch-thumb': {
      backgroundColor: theme.palette.mode === 'dark' ? '#003892' : '#001e3c',
      width: 32,
      height: 32,
      '&:before': {
        content: "''",
        position: 'absolute',
        width: '100%',
        height: '100%',
        left: 0,
        top: 0,
        backgroundRepeat: 'no-repeat',
        backgroundPosition: 'center',
        backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="20" width="20" viewBox="0 0 20 20"><path fill="${encodeURIComponent(
          '#fff',
        )}" d="M9.305 1.667V3.75h1.389V1.667h-1.39zm-4.707 1.95l-.982.982L5.09 6.072l.982-.982-1.473-1.473zm10.802 0L13.927 5.09l.982.982 1.473-1.473-.982-.982zM10 5.139a4.872 4.872 0 00-4.862 4.86A4.872 4.872 0 0010 14.862 4.872 4.872 0 0014.86 10 4.872 4.872 0 0010 5.139zm0 1.389A3.462 3.462 0 0113.471 10a3.462 3.462 0 01-3.473 3.472A3.462 3.462 0 016.527 10 3.462 3.462 0 0110 6.528zM1.665 9.305v1.39h2.083v-1.39H1.666zm14.583 0v1.39h2.084v-1.39h-2.084zM5.09 13.928L3.616 15.4l.982.982 1.473-1.473-.982-.982zm9.82 0l-.982.982 1.473 1.473.982-.982-1.473-1.473zM9.305 16.25v2.083h1.389V16.25h-1.39z"/></svg>')`,
      },
    },
    '& .MuiSwitch-track': {
      opacity: 1,
      backgroundColor: theme.palette.mode === 'dark' ? '#8796A5' : '#aab4be',
      borderRadius: 20 / 2,
    },
  }));



  const changeTheme = (props: NavMenuProps) => () => {
    if (props.theme === 'dark') {
      props.setTheme('light');
    }
    else {
      props.setTheme('dark');
    }
  }


export default connect(
    (state: ApplicationState) => ({...state.user, state: state.state?.state, theme: state.app?.theme, isMobile: state.state?.isMobile}),
    { ...App.actionCreators, ...User.actionCreators }
)(NavMenu as any);
