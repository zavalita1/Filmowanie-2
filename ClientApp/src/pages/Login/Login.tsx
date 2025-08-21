import { Tabs, TabsContent, TabsList, TabsTrigger } from "../../components/ui/tabs";
import { Input } from "../../components/ui/input";
import { Label } from "../../components/ui/label";
import { Button } from "../../components/ui/button";
import { BasicLogin } from "../../components/ui/BasicLogin";
import { useLoginWithCodeMutation, useLoginWithBasicAuthMutation, useGetUserQuery } from "../../store/apis/User/userApi";
import { LoginWithBasicAuthOutgoingDTO, LoginWithCodeOutgoingDTO } from "../../store/apis/User/types";
import { useNavigate } from "react-router";

import { AppComponentProps, Layout } from '../Layout';
import { useEffect, useState } from "react";

const Login: React.FC = () => {
    const {data, error, isLoading } = useGetUserQuery();
    const navigate = useNavigate();
    useEffect(() => {
        // what are you doing here, you're already logged
        if (!error && !isLoading && !!data?.username)
            navigate('/');
    }, [data, error, isLoading]);
    const [useLogin, result] = useLoginWithBasicAuthMutation();

    return (
        <div className="flex min-h-svh flex-col items-center justify-center">
            <Tabs defaultValue="account" className="w-full max-w-3xl">
                <TabsList className="w-full">
                    <TabsTrigger value="account">Logowanie kodem</TabsTrigger>
                    <TabsTrigger value="password">Logowanie hasłem</TabsTrigger>
                </TabsList>
                <div className="min-h-[300px] relative mt-4">
                    <TabsContent value="account" className="absolute top-0 left-0 w-full">
                        <CodeLogin />
                    </TabsContent>
                    <TabsContent value="password" className="absolute top-0 left-0 w-full">
                        <BasicLogin submitText="Zaloguj" onSubmit={basicAuthLogin} pwdPlaceholderText="Ustawione hasło" />
                    </TabsContent>
                </div>
            </Tabs>
        </div>
    );

    function basicAuthLogin(email: string, password: string) {
        const dto: LoginWithBasicAuthOutgoingDTO = { email, password };
        useLogin(dto);
    }
};

const CodeLogin: React.FC = () => {
  const [useLogin, result] = useLoginWithCodeMutation();
  const [inputValue, setInputValue] = useState('');
  
  return (<div className="grid w-full max-w-3xl items-center gap-3">
      <Label htmlFor="email">Wpisuj kod</Label>
      <Input type="email" id="email" placeholder="Kod, masa znaczków" value={inputValue} onInput={(e: React.ChangeEvent<HTMLInputElement>)=> setInputValue(e.target.value)} />
      <Button className="mt-4" type="submit" onClick={login}>Zaloguj</Button>
    </div>);

    function login() {
        const dto : LoginWithCodeOutgoingDTO = { code: inputValue };
        useLogin(dto);
    }
};


const wrappedLogin: React.FC = () => { return <Layout><Login/></Layout>}

export default wrappedLogin;