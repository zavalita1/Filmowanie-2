import React from 'react';

import { AppComponentProps, Layout } from '../Layout';
import { Separator } from '../../components/ui/separator';
import { BasicLogin } from '../../components/ui/BasicLogin';
import { useSignUpMutation } from "../../store/apis/User/userApi";
import { LoginWithBasicAuthOutgoingDTO } from '../../store/apis/User/types';

const Home: React.FC<AppComponentProps> = (props) => {
  if (props.userData === undefined) {
    return <div>loading...</div>
  }
  
  return (
  <div> {props.userData === null ? <p className="text-3xl">"Witaj anonimowy użytkowniku. Zapraszam do logowania."</p>: <LoggedView {...props} />}</div>
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