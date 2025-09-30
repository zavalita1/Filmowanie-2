import React from 'react';
import { Button } from '../components/ui';
import { AppComponentProps, Layout } from './Layout';
import { useNavigate } from 'react-router';

const Home: React.FC<AppComponentProps> = (props) => {
  return (
    <div className='-mt-30 md:w-2/5'> {props.userData === null ?
      (props.isMobile 
        ? <><p className="text-2xl">Witaj anonimowy użytkowniku.</p><br/> <p className='text-2xl'> Zapraszam do logowania.</p></>
        :  <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance mb-10">
                שָׁלוֹם 
                <br/>
                Zapraszam do logowania.
            </h1>)
      : <LoggedView {...props} />}</div>
  );
}

const LoggedView: React.FC<AppComponentProps> = props => {
  const navigate = useNavigate();
      
  return (<>
    <div>
    <p className='text-3xl'>{`Witaj ${props.userData!.username}!`}</p>
    <Button className='mt-5 ' onClick={() => navigate('moviesList')}>Do głosowania!</Button>
    </div>
  </>);

}

const wrappedHome: React.FC<AppComponentProps> = (props) => { return <Layout><Home {...props}/></Layout>}

export default wrappedHome;