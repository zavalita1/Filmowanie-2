import React, { createContext, ReactElement, ReactNode } from 'react';
import { LuLogIn, LuMenu } from 'react-icons/lu';
import { NavLink } from 'react-router';
import penguinSvg from '../components/ui/footerIcon.svg';
import { useGetUserQuery } from '../store/apis/User/userApi';
import Spinner from '../components/ui/Spinner';
import { UserState } from '@/store/apis/User/types';

export type LayoutProps = {
  children?: ReactElement<AppComponentProps>;
};

export type AppComponentProps = {
  userData?: UserState | null;
}

export const LayoutContext = createContext<string>("TODO");

export const Layout: React.FC<LayoutProps> = (props: LayoutProps) => {
  const [isNavMenuVisible, setIsNavMenuVisible] = React.useState(false);
  const { data, error, isLoading } = useGetUserQuery();

  const handleClick = () => setIsNavMenuVisible(!isNavMenuVisible);
  const handleClose = () => setIsNavMenuVisible(false);

  return (
    <LayoutContext.Provider value="TODO">
      <Header />
      { isLoading ? <Spinner /> : RenderBody() }
      <Footer />
    </LayoutContext.Provider>
  );

  function Header() {
    return <div className='w-screen bg-gray-100 h-[70px] z-10 fixed drop-shadow-lg'>
      <div className='px-2 flex justify-between items-center w-full h-full'>
        <div className='flex items-center'>
          <h1 className='text-3xl font-bold text-black mr-4 sm:text-4xl'>Filmowanie.</h1>
          <ul className='hidden text-black md:flex items-center gap-1'>
            <NavLink to="/">
            <li className='cursor-pointer px-4 py-2 hover:bg-white hover:text-green-600 hover:rounded-lg transition-colors'>
              Home
            </li>
            </NavLink>
            <NavLink to="/about">
            <li className='cursor-pointer px-4 py-2 relative group hover:bg-white hover:text-green-600 hover:rounded-lg transition-colors'>
              About
            </li>
            </NavLink>
          </ul>
        </div>
        <div className='hidden md:flex pr-4'>
          <NavLink to="/login">
          <div className="flex text-center cursor-pointer items-center mx-4 text-black hover:text-green-600" type="submit">
            <LuLogIn className='lg:w-5 lg:h-5 mx-2' />
            <span className="text-sm font-medium">
              Login
            </span>
          </div>
          </NavLink>
        </div>
        <div className='md:hidden mr-4' onClick={handleClick}>
          {!isNavMenuVisible ? <LuMenu className='w-5 text-black' /> : <div className='flex'>
            <div className="flex text-center cursor-pointer items-center mx-4 text-black hover:text-green-600" type="submit">
              <LuLogIn className='lg:w-5 lg:h-5 mx-2' />
              <span className="text-sm font-medium">
                Login
              </span>
            </div>
            <div className="block cursor-pointer shrink-0 rounded-lg bg-white mr-4 p-2.5 border border-gray-100 shadow-sm hover:bg-transparent hover:text-green-600 hover:border hover:border-green-600">
              <span className="sr-only">Account</span>
              <LuMenu className='lg:w-5 lg:h-5' />
            </div>
          </div>}
        </div>
      </div>
      {/* Navigation on small screens */}
      <ul className={!isNavMenuVisible ? 'hidden' : 'absolute bg-zinc-200 w-full px-8'}>
        <li onClick={handleClose} className='border-b-2 border-zinc-300 w-full'>
          Home
        </li>
        <li onClick={handleClose} className='border-b-2 border-zinc-300 w-full'>
          About Us
        </li>
        <li onClick={handleClose} className='border-b-2 border-zinc-300 w-full'>
          Contact Us
        </li>
        <li onClick={handleClose} className='border-b-2 border-zinc-300 w-full'>
          Services
        </li>
      </ul>
    </div>
  }

  function RenderBody() {
    return  (props.children === undefined ? 
    <></>
    : (error !== undefined ? DisplayFatalError() : <div id="container" className="flex flex-row min-h-screen justify-center items-center">{React.cloneElement(props.children, { userData: data, test:"TODO"} as any)}</div>));
  }

  function DisplayFatalError() {
    return (<div className="flex flex-row min-h-screen justify-center items-center">Coś się zesrało w fatalny sposób. Strona jest do wyjebania, prosze odświeżyć.</div>);
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
};

