import React from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../../components/ui/tabs";
import { Input } from "../../components/ui/input";
import { Label } from "../../components/ui/label";
import { Button } from "../../components/ui/button";
import { Layout } from '../Layout';

const Home: React.FC = () => {
  return (
    <div className="flex min-h-svh flex-col items-center justify-center">
      <Tabs defaultValue="account" className="w-full max-w-md h-full">
        <TabsList>
          <TabsTrigger value="account">Logowanie kodem</TabsTrigger>
          <TabsTrigger value="password">Logowanie hasłem</TabsTrigger>
        </TabsList>
        <TabsContent value="account"><CodeLogin /></TabsContent>
        <TabsContent value="password"><BasicLogin /></TabsContent>
      </Tabs>
    </div>
  );
};

const CodeLogin: React.FC = () => {
  return (<div className="grid w-full max-w-sm items-center gap-3">
      <Label htmlFor="email">Wpisuj kod</Label>
      <Input type="email" id="email" placeholder="Kod, masa znaczków, które dostaliście wieki temu" />
      <Button className="mt-4" type="submit">Zaloguj</Button>
    </div>);
};

const BasicLogin: React.FC = () => {
  return (<div className="grid w-full max-w-sm items-center gap-3">
      <Label htmlFor="email">Mail</Label>
      <Input type="email" id="email" placeholder="Mail" />
      <Label htmlFor="password">Hasło</Label>
      <Input type="password" id="password" placeholder="Ustawione hasło" />
    </div>);
};

const wrappedHome: React.FC = () => { return <Layout><Home/></Layout>}

export default wrappedHome;