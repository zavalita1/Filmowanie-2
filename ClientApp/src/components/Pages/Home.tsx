import * as React from 'react';
import { connect } from 'react-redux';
import * as User from '../../store/User'
import { ApplicationState } from '../../store';
import SubPageWrapper from '../SubPageWrapper';
import { Typography } from '@mui/material';
import { useTheme } from '@mui/material/styles';

import BottomNavigation from '@mui/material/BottomNavigation';
import { Basic, Login } from '../Login';

type HomeProps = User.UserState &
typeof User.actionCreators;

const Home = (props: HomeProps) => {
  const theme = useTheme();
  
  return (
  <React.Fragment>
    {!props.user?.username
    ?  <LoggingView {...props}></LoggingView>
    : <LoggedView {...props}></LoggedView>}
    
    <BottomNavigation showLabels>
      </BottomNavigation>
    
  </React.Fragment>
)};

const HomeWrapper = (props: HomeProps) => {
  const subWrapperProps = {
      child: Home,
      childProps: props
  };

  return (<SubPageWrapper {...subWrapperProps} ></SubPageWrapper>);
}

const LoggedView = (props: HomeProps) => {
  
  
  return (
    <>
    <Typography>Dzień dobry {props.user?.username}</Typography>

      {
        !props.user?.hasBasicAuthSetup ? (
          <div className='sign-up-container'>
            <Typography>Strudzeni szukaniem kodu za każdym razem? Możecie wstukać mail + hasło, co by łatwiej pamiętać.</Typography>
            <Basic sendMailPwd={props.signUp}></Basic>
          </div>
          )
          : <></>
      }
    </>
  );
}

const LoggingView = (props: HomeProps) => {
  return (
    <Login sendCode={(code) => props.login(code)} sendMailPwd={(mail, password) => props.basicLogin(mail, password)}></Login>
  );
}

// TODO add UI for this
async function revokePushNotificationPermissions() {
  const registration = await navigator.serviceWorker.getRegistration();

  if (!registration) return;

  const subscription = await registration.pushManager.getSubscription();

  if (!subscription) return;

  fetch('/remove-subscription', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ endpoint: subscription.endpoint })
  });
  const unsubscribed = await subscription.unsubscribe();
  if (unsubscribed) {
    console.info('Successfully unsubscribed from push notifications.');
  }
}


export default connect(
  (state: ApplicationState) => state.user,
  User.actionCreators
  )(HomeWrapper as any);
