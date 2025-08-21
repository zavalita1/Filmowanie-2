import React from 'react';

import { AppComponentProps, Layout } from '../Layout';

const Home: React.FC<AppComponentProps> = (props) => {
  if (props.userData === undefined) {
    return <div>loading...</div>
  }
  
  return (
  <div> {props.userData === null ? "Witaj anonimowy użytkowniku. Zapraszam do logowania." : `Witaj ${props.userData!.username}!`}</div>
  );
}

const wrappedHome: React.FC = () => { return <Layout><Home/></Layout>}

export default wrappedHome;