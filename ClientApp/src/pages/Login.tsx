import { Tabs, TabsContent, TabsList, TabsTrigger, Input, Label, Button } from "../components/ui";
import { BasicLogin } from "../components/BasicLogin";
import { GoogleLoginButton } from "../components/GoogleLogin";
import { useLoginWithCodeMutation, useLoginWithBasicAuthMutation, useLoginWithGoogleMutation, useGetUserQuery } from "../store/apis/1-User/userApi";
import { LoginWithBasicAuthOutgoingDTO, LoginWithCodeOutgoingDTO, LoginWithGoogleOutgoingDTO } from "../store/apis/1-User/types";
import { useNavigate } from "react-router";

import { AppComponentProps, Layout } from './Layout';
import { useEffect, useState } from "react";

const Login: React.FC = () => {
    const {data, error, isLoading } = useGetUserQuery();
    const navigate = useNavigate();
    useEffect(() => {
        // what are you doing here, you're already logged
        if (!error && !isLoading && !!data?.username)
            navigate('/');
    }, [data, error, isLoading]);
    const [useBasicLogin] = useLoginWithBasicAuthMutation();
    const [useGoogleLogin] = useLoginWithGoogleMutation();

    return (
        <div className="flex min-h-svh flex-col items-center justify-center">
            <Tabs defaultValue="account" className="w-3/4 md:w-full max-w-3xl">
                <TabsList className="w-full flex flex-wrap">
                    <TabsTrigger className="grow-0" value="account">Logowanie kodem</TabsTrigger>
                    <TabsTrigger className="flex-1/3" value="password">Logowanie hasłem</TabsTrigger>
                    <TabsTrigger className="grow-0" value="google">Logowanie z Google</TabsTrigger>
                </TabsList>
                <div className="min-h-[300px] relative mt-4">
                    <TabsContent value="account" className="absolute top-0 left-0 w-full">
                        <CodeLogin />
                    </TabsContent>
                    <TabsContent value="password" className="absolute top-0 left-0 w-full">
                        <BasicLogin submitText="Zaloguj" onSubmit={basicAuthLogin} pwdPlaceholderText="Ustawione hasło" />
                    </TabsContent>
                    <TabsContent value="google" className="absolute top-0 left-0 w-full">
                        <div className="w-full max-w-3xl items-center">
                            <GoogleLoginButton 
                                onSuccess={ googleLogin }
                                onError={() => { console.error("Google login failed");}}
                            />
                        </div>
                    </TabsContent>
                </div>
            </Tabs>
        </div>
    );

    function basicAuthLogin(email: string, password: string) {
        const dto: LoginWithBasicAuthOutgoingDTO = { email, password };
        useBasicLogin(dto);
    }

    function googleLogin(code: string, scope: string) {
        const dto: LoginWithGoogleOutgoingDTO = { code, scope };
        useGoogleLogin(dto);
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