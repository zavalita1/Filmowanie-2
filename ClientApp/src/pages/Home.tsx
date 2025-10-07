import React from 'react';
import { Button } from '../components/ui';
import { AppComponentProps, Layout } from './Layout';
import { useNavigate } from 'react-router';

const Home: React.FC<AppComponentProps> = (props) => {
  return (
    <div className='-mt-30 md:w-2/5 flex flex-wrap justify-center-safe'> {props.userData === null 
      ? <UnloggedView {...props} />
      : <LoggedView {...props} />}
    </div>
  );
}

const LoggedView: React.FC<AppComponentProps> = props => {
  const navigate = useNavigate();
      
  return (<>
    <div className='flex flex-wrap justify-center-safe'>
    <p className='text-3xl'>{`Witaj ${props.userData!.username}!`}</p>
    <div className='basis-full h-0'></div>
    { props.userData?.nominations?.length ?? 0 > 0 
    ? <Button className='mt-5' onClick={() => navigate('nominate')}>Do nominacji!</Button> 
    : <Button className='mt-5' onClick={() => navigate('moviesList')}>Do głosowania!</Button>}
    </div>
  </>);
}

const UnloggedView: React.FC<AppComponentProps> = props => {
  const navigate = useNavigate();

  if (props.isMobile) {
    return <>
      <p className="text-2xl">Witaj anonimowy użytkowniku.</p>
      <br />
      <p className='text-2xl'> Zapraszam do logowania.</p>
      <Button className='mt-5' onClick={() => navigate('login')}>Do logowania!</Button>
    </>
  }

  return <><h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance">
    שָׁלוֹם
    <br />
    Zapraszam do logowania.
  </h1>
  <div className='basis-full h-0'></div>
    <Button className='mt-5' onClick={() => navigate('login')}>Do logowania!</Button>
  </>
}

const wrappedHome: React.FC<AppComponentProps> = (props) => { return <Layout><Home {...props}/></Layout>}

export default wrappedHome;