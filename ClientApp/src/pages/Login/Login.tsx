import { Tabs, TabsContent, TabsList, TabsTrigger } from "../../components/ui/tabs";
import { Input } from "../../components/ui/input";
import { Label } from "../../components/ui/label";
import { Button } from "../../components/ui/button";
import { useLoginWithCodeMutation, useGetUserQuery } from "../../store/apis/User/userApi";
import { LoginWithCodeOutgoingDTO } from "../../store/apis/User/types";
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
    }, [data, error, isLoading])

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
            <BasicLogin />
          </TabsContent>
        </div>
      </Tabs>
    </div>
  );
};

const CodeLogin: React.FC = () => {
  const [useLogin, result] = useLoginWithCodeMutation({ fixedCacheKey: 'looo'});
  const [inputValue, setInputValue] = useState('');
  
  return (<div className="grid w-full max-w-3xl items-center gap-3">
      <Label htmlFor="email">Wpisuj kod</Label>
      <Input type="email" id="email" placeholder="Kod, masa znaczków" value={inputValue} onInput={e => setInputValue(e.target.value)} />
      <Button className="mt-4" type="submit" onClick={() => login(inputValue)}>Zaloguj</Button>
    </div>);

    function login(code: string) {
        const dto : LoginWithCodeOutgoingDTO = { code };
        useLogin(dto);
    }
};

const BasicLogin: React.FC = () => {
  return (<div className="grid w-full max-w-3xl items-center gap-3">
      <Label htmlFor="email">Mail</Label>
      <Input type="email" id="email" placeholder="Mail" />
      <Label htmlFor="password">Hasło</Label>
      <Input type="password" id="password" placeholder="Ustawione hasło" />
      <Button className="mt-4" type="submit">Zaloguj</Button>
    </div>);
};

const wrappedLogin: React.FC = () => { return <Layout><Login/></Layout>}

export default wrappedLogin;