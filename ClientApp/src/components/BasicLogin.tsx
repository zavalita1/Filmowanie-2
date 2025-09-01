import { useState } from "react";
import { Input, Label, Button } from "./ui";

export type BasicLoginProps = {
    submitText: string;
    onSubmit: (mail:string, pwd: string) => void;
    pwdPlaceholderText: string;
};

export const BasicLogin: React.FC<BasicLoginProps> = (props) => {
    const [mailValue, setMailValue] = useState('');
    const [pwdValue, setPwdValue] = useState('');

  return (<div className="grid w-full max-w-3xl items-center gap-3">
      <Label htmlFor="email">Mail</Label>
      <Input type="email" id="email" placeholder="Mail" value={mailValue} onInput={(e: React.ChangeEvent<HTMLInputElement>)=> setMailValue(e.target.value)}/>
      <Label htmlFor="password">Hasło</Label>
      <Input type="password" id="password" placeholder={props.pwdPlaceholderText} value={pwdValue} onInput={(e: React.ChangeEvent<HTMLInputElement>)=> setPwdValue(e.target.value)}/>
      <Button className="mt-4" type="submit" onClick={() => props.onSubmit(mailValue, pwdValue)}>{props.submitText}</Button>
    </div>);
};