import React from 'react';

import { AppComponentProps, Layout } from '../Layout';
import { Separator } from '../../components/ui/separator';
import { BasicLogin } from '../Login/BasicLogin';
import { useSignUpMutation } from "../../store/apis/1-User/userApi";
import { LoginWithBasicAuthOutgoingDTO } from '../../store/apis/1-User/types';

const Home: React.FC<AppComponentProps> = (props) => {
  return (
    <div className='-mt-30'> {props.userData === null ?
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
  const [useSignUp, result] = useSignUpMutation();

  return (<>
    <p className='text-3xl'>{`Witaj ${props.userData!.username}!`}</p>

    {props.userData!.hasBasicAuthSetup === false ? <></> :
      <>
        <Separator className='mt-10 mb-10' />
        <div> Strudzeni szukaniem kodu za każdym razem? Możecie wstukać mail + hasło, co by łatwiej pamiętać.</div>
        <br/>
        <BasicLogin submitText='Wstukaj' onSubmit={signUp} pwdPlaceholderText="Twoje hasło"></BasicLogin>
      </>
    }
  </>);

  function signUp(email: string, password: string) {
    const dto: LoginWithBasicAuthOutgoingDTO = { email, password };
    useSignUp(dto);
  }
}

const wrappedHome: React.FC<AppComponentProps> = (props) => { return <Layout><Home {...props}/></Layout>}

export default wrappedHome;