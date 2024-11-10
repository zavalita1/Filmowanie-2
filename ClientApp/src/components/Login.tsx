import * as React from 'react';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';

type PossibleTab = 'Code' | 'Basic';

interface ILoginProps extends ILoginBasicProps {
  sendCode: (code: string) => void;
}

interface ILoginBasicProps {
  sendMailPwd: (email: string, password: string) => void;
}

export const Login = (props: ILoginProps) => {
  const [value, setValue] = React.useState<PossibleTab>('Code');

  const handleChange = (event: React.SyntheticEvent, newValue: PossibleTab) => {
      setValue(newValue);
  };

  return (<>
  <div className='tabs'>
      <Box sx={{ width: '100%' }}>
          <Tabs
              value={value}
              onChange={handleChange}
              textColor="secondary"
              indicatorColor="secondary"
              aria-label="secondary tabs example"
          >
              <Tab value="Code" label="Logowanie kodem" />
              <Tab value="Basic" label="Logowanie hasłem" />
          </Tabs>
      </Box>
  </div>
  <>
      {
          getTab(value, props)
      }
  </>
</>)
}


function getTab(tabName: PossibleTab, props: ILoginProps) {
  if (tabName === "Code") {
      return (
          <Code {...props}></Code>
      );
  }

  if (tabName === 'Basic') {
      return (
          <Basic {...props}></Basic>
      )
  }

  return <></>
}

const Code = (props: ILoginProps) => {
  const [code, setCode] = React.useState("");

  return <Box
    component="form"
    sx={{
      '& > :not(style)': { m: 1, width: '25ch' },
    }}
    noValidate
    autoComplete="off"
  >
      <TextField id="standard-basic" label="Wpisuj kod" variant="standard" value={code} onChange={e => setCode(e.target.value)} onKeyDown={onKeyDown} autoFocus={true} autoComplete="login-code" />
    <Button variant="contained" type='button' onClick={e => { e.preventDefault(); props.sendCode(code); }}>Ślij</Button>
  </Box>;


  function onKeyDown(e: React.KeyboardEvent<HTMLDivElement>) {
    if (e.key === 'Enter') {
      e.preventDefault();
      props.sendCode(code);
    }
  }
};

export const Basic = (props: ILoginBasicProps) => {
  const [mail, setMail] = React.useState("");
  const [password, setPassword] = React.useState("");

  return <Box
    component="form"
    sx={{
      '& > :not(style)': { m: 1, width: '25ch' },
    }}
    noValidate
    autoComplete="off"
  >
      <TextField id="standard-basic-username" label="E-mail" variant="standard" value={mail} onChange={e => setMail(e.target.value)} autoFocus={true} autoComplete="login-mail" />
      <br/>
      <TextField id="standard-basic-password" label="Hasło" variant="standard" value={password} onChange={e => setPassword(e.target.value)} onKeyDown={onKeyDown} autoFocus={false} autoComplete="login-pwd" type='password'/>
    <br/>
    <Button variant="contained" type='button' onClick={e => { e.preventDefault(); props.sendMailPwd(mail, password); }}>Ślij</Button>
  </Box>;


  function onKeyDown(e: React.KeyboardEvent<HTMLDivElement>) {
    if (e.key === 'Enter') {
      e.preventDefault();
      props.sendMailPwd(mail, password);
    }
  }
};
