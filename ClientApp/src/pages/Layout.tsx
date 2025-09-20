import clsx from 'clsx';
import React, { createContext, ReactElement } from 'react';
import { LuLogIn, LuMenu, LuLogOut } from 'react-icons/lu';
import { Moon, Sun } from "lucide-react"
import { NavLink, useNavigate } from 'react-router';
import penguinSvg from '../components/ui/footerIcon.svg';
import { Toaster, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger, Button } from '../components/ui';
import { ThemeProvider, useTheme } from '../components/ThemeProvider';
import { useGetUserQuery, useLogoutMutation } from '../store/apis/1-User/userApi';
import { useLazyGetStateQuery } from '../store/apis/2-Voting/votingApi';
import { useLazyGetNominationsQuery } from '../store/apis/4-Nomination/api'
import Spinner from '../components/Spinner';
import { UserStateWithNominations } from '@/store/apis/1-User/types';
import { useAppSelector } from '../hooks/redux';
import { VotingStatus } from '../consts/votingStatus';

export type BaseLayoutProps = {
  children?: ReactElement<AppComponentProps>;
};

type DisableCenterVertically = {
  disableCenterVertically: boolean;
}

export type LayoutProps = BaseLayoutProps | (BaseLayoutProps & DisableCenterVertically);

export type AppComponentProps = {
  userData: UserStateWithNominations | null;
  votingStatus: VotingStatus;
  isMobile: boolean;
}

export const Layout: React.FC<LayoutProps> = (props: LayoutProps) => {
  const [isNavMenuVisible, setIsNavMenuVisible] = React.useState(false);
  const isLoading = useAppSelector(s => s.global.isLoading);
  const { data: userData, error: userError, isLoading: userDataIsLoading } = useGetUserQuery();
  const [votingStateQueryTrigger, votingState] = useLazyGetStateQuery();
  const [nominationQueryTrigger, nominations] = useLazyGetNominationsQuery();
  const isUserLogged = userData !== undefined && userData !== null
  const [useLogout, result] = useLogoutMutation();
  const navigate = useNavigate();

  if (isUserLogged && !votingState.isLoading && !votingState.isError && !votingState.isSuccess) {
    votingStateQueryTrigger();
  }
  if (isUserLogged && !nominations.isLoading && !nominations.isError && !nominations.isSuccess) {
    nominationQueryTrigger();
  }

  const handleClick = () => setIsNavMenuVisible(!isNavMenuVisible);
  const handleClose = () => setIsNavMenuVisible(false);

  const isHistoryEnabled = isUserLogged;
  const isMovieListEnabled = (isUserLogged && votingState?.currentData === VotingStatus.Voting) || userData?.isAdmin;
  const isResultsEnabled = (isUserLogged && votingState?.currentData === VotingStatus.Results) || userData?.isAdmin;
  const isNominateEnabled = (isUserLogged && (nominations.currentData?.length ?? 0) > 0) || userData?.isAdmin;

  const containerClasses = clsx([
    "flex",
    "flex-row",
    "min-h-screen",
    "justify-center",
    (props as DisableCenterVertically)?.disableCenterVertically ? "" : "items-center",
    "-z-10",
    "fixed",
    "w-full"
  ])

  return (
    <ThemeProvider>
      <div className='flex flex-col select-none'>
      <Header />
      <div id="container" className={containerClasses}><Spinner isLoading={isLoading}></Spinner></div>
      { RenderBody(isLoading) }
      <Toaster />
      <Footer />
      </div>
    </ThemeProvider>
  );

  function Header() {
    return <div className='w-screen bg-gradient-to-tr from-emerald-50 to-sky-100 dark:from-pink-900 dark:to-black h-[70px] drop-shadow-lg relative'>
      <div className='px-2 flex justify-between items-center w-full h-full'>
        <div className='flex items-center'>
          <h1 className='text-3xl font-bold text-black dark:text-white mr-4 sm:text-4xl'>Filmowanie.</h1>
          <ul className='hidden md:flex items-center gap-1'>
            <MenuLink text='Home' url='/'/>
            <MenuLink text='About' url='/about'/>
            <MenuLink text='Lista filmów' url='/moviesList' isDisabled={!isMovieListEnabled}/>
            <MenuLink text='Wyniki' url='/results' isDisabled={!isResultsEnabled}/>
            <MenuLink text='Admin' url='/admin' isDisabled={!userData?.isAdmin}/>
            <MenuLink text='Nominuj' url='/nominate' isDisabled={!isNominateEnabled}/>
            <MenuLink text='Historia' url='/history' isDisabled={!isHistoryEnabled}/>
            <ModeToggle />
          </ul>
          <div className='md:hidden'><ModeToggle /></div>
        </div>
        <div className='hidden md:flex pr-4'>
          <LoginLogoutLink isUserLogged={isUserLogged} onLogout={logout}/>
        </div>
      {/* mobile screens */}
        <div className='md:hidden mr-4' onClick={handleClick}>
          <LuMenu className='w-5 text-black dark:text-white' />
        </div>
      </div>
      <ul className={!isNavMenuVisible ? 'hidden' : 'absolute bg-gradient-to-tr from-emerald-50 to-sky-100 dark:from-pink-900 dark:to-black w-full px-8'}>
       <MenuLink isMobile={true} text='Home' url='/'/>
       <MenuLink isMobile={true} text='About' url='/about'/>
       <MenuLink isMobile={true} text='Lista filmów' url='/moviesList' isDisabled={!isMovieListEnabled}/>
       <MenuLink isMobile={true} text='Wyniki' url='/results' isDisabled={!isResultsEnabled}/>
       <MenuLink isMobile={true} text='Admin' url='/admin' isDisabled={!userData?.isAdmin}/>
       <MenuLink isMobile={true} text='Nominuj' url='/nominate' isDisabled={!isNominateEnabled}/>
       <MenuLink isMobile={true} text='Historia' url='/history' isDisabled={!isHistoryEnabled}/>
       <LoginLogoutLink isMobile={true} isUserLogged={isUserLogged} onLogout={logout}/>
      </ul>
    </div>
  }

  function RenderBody(addOpacity: boolean) {
    const mobileBreakpointPx = 768;
    const [isMobile, setIsMobile] = React.useState(window.innerWidth < mobileBreakpointPx);

    React.useEffect(() => {
      const mql = window.matchMedia(`(max-width: ${mobileBreakpointPx - 1}px)`);
      const onChange = () => {
        setIsMobile(window.innerWidth < mobileBreakpointPx);
      };
      mql.addEventListener("change", onChange);
      return () => {
        mql.removeEventListener("change", onChange);
      };
    });

    if (userError !== undefined || ((votingState.isError || nominations.isError) && userData !== null))
      return DisplayFatalError();

    if (props.children === undefined)
      return <></>;

    if (userDataIsLoading || votingState.isLoading || nominations.isLoading)
      return (<div className='min-h-screen text-center content-center'>Loading...</div>);

    const containerClassName = clsx(
      'flex',
      'flex-row',
      'flex-wrap',
      'justify-center',
      'min-h-screen',
       (props as DisableCenterVertically)?.disableCenterVertically ? "" : "items-center",
      addOpacity ? 'opacity-15' : '',
      isMobile ? 'ml-5 mr-5' : ''
    )

    const userDataForProps = userData === null ? null : {...userData!, nominations: nominations.currentData! };
    const childProps = { userData: userDataForProps, votingStatus: votingState!.currentData!, isMobile: isMobile } satisfies AppComponentProps;

    return (
      <div id="container" className={containerClassName}>
        {React.cloneElement(props.children, childProps)}
      </div>);
  }

  function DisplayFatalError() {
    return (<div className="flex flex-row justify-center items-center relative min-h-screen">Coś się zesrało w fatalny sposób. Strona jest do wyjebania, prosze odświeżyć.</div>);
  }

  function Footer() {
    return <section className="bg-gradient-to-tr from-emerald-50 to-sky-100 dark:from-black dark:to-pink-900 mt-10 w-full">
      <footer className="">
        <div className="mx-auto max-w-screen-xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="sm:flex sm:items-center sm:justify-between">
            <div className="flex justify-center sm:justify-start">
              <img src={penguinSvg} className='h-full w-full' />
            </div>

            <p className="mt-4 text-center text-sm text-gray-500 dark:text-amber-300 lg:mt-0 lg:text-right">
              Copyright &copy; 2025. <br />
              W istocie nie ma tu żadnego kopirajta, proszę się zgłosić po nagrodę za uważne czytanie niepotrzebnych stopek.
            </p>
          </div>
        </div>
      </footer>
    </section>
  }

  async function logout() {
    navigate('/');
    await useLogout().unwrap();
  }
};

export function ModeToggle() {
  const { setTheme, theme } = useTheme()
 
  return (
        <Button variant="outline" size="icon" onClick={() => setTheme(theme === "light" ? "dark" : "light")}>
          <Sun  className="h-[1.2rem] w-[1.2rem] scale-100 rotate-0 transition-all dark:scale-0 dark:-rotate-90" />
          <Moon onClick={() => setTheme("light")} className="absolute h-[1.2rem] w-[1.2rem] scale-0 rotate-90 transition-all dark:scale-100 dark:rotate-0" />
        </Button>
  )
}

type MenuLinkProps = {
  text: string;
  url: string;
  isDisabled?: boolean;
  isMobile?: boolean;
}

const MenuLink: React.FC<MenuLinkProps> = (props: MenuLinkProps) => {
  if (props.isDisabled)
    return <></>;

  if (props.isMobile) {
    return (
      <NavLink to={props.url}>
        <li className='border-b-2 border-zinc-300 w-full min-h-10 text-sm font-medium mt-2'>
          {props.text}
        </li>
      </NavLink>
    );
  }

  return (<NavLink to={props.url}>
    <li className='cursor-pointer px-4 py-2 hover:bg-white dark:hover:bg-none dark:hover:text-amber-300 hover:text-green-600 hover:rounded-lg transition-colors'>
      {props.text}
    </li>
  </NavLink>
  );
}

const LoginLogoutLink: React.FC<{ isUserLogged: boolean, onLogout: () => void, isMobile?: boolean }> = props => {
  if (props.isMobile) {
    if (!props.isUserLogged) {
      return (<NavLink to="/login"><div className='flex items-center text-black dark:text-white'>
        <li className="text-sm font-medium min-h-10 place-content-center">
          Login
        </li>
        <LuLogIn className='lg:w-5 lg:h-5 mx-2' />
      </div>
      </NavLink>);
    }

    return (
      <div className='flex items-center text-black dark:text-white' onClick={props.onLogout}>
        <li className="text-sm font-medium min-h-10 place-content-center">
          Logout
        </li>
          <LuLogOut className='lg:w-5 lg:h-5 mx-2' />
      </div>
    );
  }

  if (!props.isUserLogged) return (
    <NavLink to="/login">
      <div className="flex text-center cursor-pointer items-center mx-4 text-black dark:text-white hover:text-green-600 dark:hover:text-amber-300">
        <LuLogIn className='lg:w-5 lg:h-5 mx-2' />
        <span className="text-sm font-medium">
          Login
        </span>
      </div>
    </NavLink>
  );

  return (
    <div className="flex text-center cursor-pointer items-center mx-4 text-black dark:text-white hover:text-green-600 dark:hover:text-amber-300" onClick={props.onLogout}>
      <LuLogOut className='lg:w-5 lg:h-5 mx-2' />
      <span className="text-sm font-medium">
        Logout
      </span>
    </div>
  );
}

