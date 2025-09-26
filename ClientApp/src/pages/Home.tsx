import React from 'react';
import { Button, Separator } from '../components/ui';
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "../components/ui/accordion";

import { AppComponentProps, Layout } from './Layout';
import { BasicLogin } from '../components/BasicLogin';
import { useSignUpMutation } from "../store/apis/1-User/userApi";
import { LoginWithBasicAuthOutgoingDTO } from '../store/apis/1-User/types';
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
  const [useSignUp, result] = useSignUpMutation();
  const navigate = useNavigate();

  return (<>
    <div>
    <p className='text-3xl'>{`Witaj ${props.userData!.username}!`}</p>
    <Button className='mt-5 ' onClick={() => navigate('moviesList')}>Do głosowania!</Button>
    </div>

    {props.userData!.hasRegisteredBasicAuth === true ? <></> :
      <>
        <Separator className='mt-10 mb-40' />
       <Accordion
      type="single"
      collapsible
      className="w-full"
      defaultValue="item-2"
    >
      <AccordionItem className='h-[200px]' value="item-1">
        <AccordionTrigger>Opcjonalne ustawiania swojego hasła - możesz, nie musisz.</AccordionTrigger>
        <AccordionContent className="flex flex-col gap-4 text-balance">
          <p>
            Strudzeni szukaniem kodu za każdym razem? Możecie wstukać mail + hasło, co by łatwiej pamiętać.
          </p>
         <BasicLogin submitText='Wstukaj' onSubmit={signUp} pwdPlaceholderText="Twoje hasło"></BasicLogin>
        </AccordionContent>
      </AccordionItem>
    </Accordion>
    </>
    }
  </>);

  async function signUp(email: string, password: string) {
    const dto: LoginWithBasicAuthOutgoingDTO = { email, password };
    await useSignUp(dto).unwrap();
    navigate('/login');
  }
}

const wrappedHome: React.FC<AppComponentProps> = (props) => { return <Layout><Home {...props}/></Layout>}

export default wrappedHome;