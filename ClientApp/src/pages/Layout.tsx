import React, { createContext, ReactElement, ReactNode } from 'react';
import { LuLogIn, LuMenu, LuLogOut } from 'react-icons/lu';
import { NavLink } from 'react-router';
import penguinSvg from '../components/ui/footerIcon.svg';
import { useGetUserQuery, useLogoutMutation } from '../store/apis/1-User/userApi';
import { useGetStateQuery } from '../store/apis/2-Voting/votingApi';
import Spinner from '../components/ui/Spinner';
import { UserState } from '@/store/apis/1-User/types';
import { useAppSelector } from '../hooks/redux';
import clsx from 'clsx';
import { VotingStatus } from '../consts/votingStatus';
import { Toaster } from '../components/ui/sonner';

export type BaseLayoutProps = {
  children?: ReactElement<AppComponentProps>;
};

type DisableCenterVertically = {
  disableCenterVertically: boolean;
}

export type LayoutProps = BaseLayoutProps | (BaseLayoutProps & DisableCenterVertically);

export type AppComponentProps = {
  userData: UserState | null;
  votingStatus: VotingStatus;
  isMobile: boolean;
}

export const LayoutContext = createContext<string>("TODO");

export const Layout: React.FC<LayoutProps> = (props: LayoutProps) => {
  const [isNavMenuVisible, setIsNavMenuVisible] = React.useState(false);
  const isLoading = useAppSelector(s => s.global.isLoading);
  const { data: userData, error: userError, isLoading: userDataIsLoading } = useGetUserQuery();
  const { data: votingState, error: votingStateError, isLoading: votingStateIsLoading } = useGetStateQuery();
  const isUserLogged = userData !== undefined && userData !== null
  const [useLogout, result] = useLogoutMutation();

  const handleClick = () => setIsNavMenuVisible(!isNavMenuVisible);
  const handleClose = () => setIsNavMenuVisible(false);

  const isMovieListEnabled = (isUserLogged && votingState === VotingStatus.Voting) || userData?.isAdmin;
  const isResultsEnabled = (isUserLogged && votingState !== VotingStatus.Voting) || userData?.isAdmin;

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
    <LayoutContext.Provider value="TODO">
      <Header />
      <div id="container" className={containerClasses}><Spinner isLoading={isLoading}></Spinner></div>
      { RenderBody(isLoading) }
      <Toaster />
      <Footer />
    </LayoutContext.Provider>
  );

  function Header() {
    return <div className='w-screen bg-gray-100 h-[70px] fixed drop-shadow-lg relative'>
      <div className='px-2 flex justify-between items-center w-full h-full'>
        <div className='flex items-center'>
          <h1 className='text-3xl font-bold text-black mr-4 sm:text-4xl'>Filmowanie.</h1>
          <ul className='hidden text-black md:flex items-center gap-1'>
            <MenuLink text='Home' url='/'/>
            <MenuLink text='About' url='/about'/>
            <MenuLink text='Lista filmów' url='/moviesList' isDisabled={!isMovieListEnabled}/>
            <MenuLink text='Wyniki' url='/results' isDisabled={!isResultsEnabled}/>
            <MenuLink text='Admin' url='/admin' isDisabled={!userData?.isAdmin}/>
          </ul>
        </div>
        <div className='hidden md:flex pr-4'>
          <LoginLogoutLink isUserLogged={isUserLogged} onLogout={logout}/>
        </div>
      {/* mobile screens */}
        <div className='md:hidden mr-4' onClick={handleClick}>
          <LuMenu className='w-5 text-black' />
        </div>
      </div>
      <ul className={!isNavMenuVisible ? 'hidden' : 'absolute bg-zinc-200 w-full px-8'}>
       <MenuLink isMobile={true} text='Home' url='/'/>
       <MenuLink isMobile={true} text='About' url='/about'/>
       <MenuLink isMobile={true} text='Lista filmów' url='/moviesList' isDisabled={!isMovieListEnabled}/>
       <MenuLink isMobile={true} text='Wyniki' url='/results' isDisabled={!isResultsEnabled}/>
       <MenuLink isMobile={true} text='Admin' url='/admin' isDisabled={!userData?.isAdmin}/>
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

    if (userError !== undefined || (votingStateError !== undefined && userData !== null))
      return DisplayFatalError();

    if (props.children === undefined)
      return <></>;

    if (userDataIsLoading || votingStateIsLoading)
      return (<div>Loading...</div>);

    const containerClassName = clsx(
      'flex',
      'flex-row',
      'flex-wrap',
      'min-h-screen',
      'justify-center',
       (props as DisableCenterVertically)?.disableCenterVertically ? "" : "items-center",
      addOpacity ? 'opacity-15' : '',
      isMobile? 'ml-5 mr-5' :''
    )

    const childProps = { userData: userData === null ? null : userData!, votingStatus: votingState!, isMobile: isMobile } satisfies AppComponentProps;

    return (
      <div id="container" className={containerClassName}>
        {React.cloneElement(props.children, childProps)}
      </div>);
  }

  function DisplayFatalError() {
    return (<div className="flex flex-row min-h-screen justify-center items-center relative">Coś się zesrało w fatalny sposób. Strona jest do wyjebania, prosze odświeżyć.</div>);
  }

  function Footer() {
    return <section className="relative bg-white mt-10">
      <footer className="bg-gray-50">
        <div className="mx-auto max-w-screen-xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="sm:flex sm:items-center sm:justify-between">
            <div className="flex justify-center text-teal-600 sm:justify-start">
              <img src={penguinSvg} className='h-full w-full' />
            </div>

            <p className="mt-4 text-center text-sm text-gray-500 lg:mt-0 lg:text-right">
              Copyright &copy; 2025. <br />
              W istocie nie ma tu żadnego kopirajta, proszę się zgłosić po nagrodę za uważne czytanie niepotrzebnych stopek.
            </p>
          </div>
        </div>
      </footer>
    </section>
  }

  function logout() {
    useLogout();
  }
};

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
    <li className='cursor-pointer px-4 py-2 hover:bg-white hover:text-green-600 hover:rounded-lg transition-colors'>
      {props.text}
    </li>
  </NavLink>
  );
}

const LoginLogoutLink: React.FC<{ isUserLogged: boolean, onLogout: () => void, isMobile?: boolean }> = props => {
  if (props.isMobile) {
    if (!props.isUserLogged) {
      return (<NavLink to="/login"><div className='flex items-center text-black'>
        <li className="text-sm font-medium min-h-10 place-content-center">
          Login
        </li>
        <LuLogIn className='lg:w-5 lg:h-5 mx-2' />
      </div>
      </NavLink>);
    }

    return (
      <div className='flex items-center text-black' onClick={props.onLogout}>
        <li className="text-sm font-medium min-h-10 place-content-center">
          Logout
        </li>
          <LuLogOut className='lg:w-5 lg:h-5 mx-2' />
      </div>
    );
  }

  if (!props.isUserLogged) return (
    <NavLink to="/login">
      <div className="flex text-center cursor-pointer items-center mx-4 text-black hover:text-green-600">
        <LuLogIn className='lg:w-5 lg:h-5 mx-2' />
        <span className="text-sm font-medium">
          Login
        </span>
      </div>
    </NavLink>
  );

  return (
    <div className="flex text-center cursor-pointer items-center mx-4 text-black hover:text-green-600" onClick={props.onLogout}>
      <LuLogOut className='lg:w-5 lg:h-5 mx-2' />
      <span className="text-sm font-medium">
        Logout
      </span>
    </div>
  );
}

