import React, { createContext } from 'react';


export type LayoutProps = {
  children?: React.ReactNode;
};

export const LayoutContext = createContext<string>("TODO");

export const Layout: React.FC<LayoutProps> = (props: LayoutProps) => {
  return (
    <LayoutContext.Provider value="TODO">
      <div>Navigation</div>
      {props.children}
    </LayoutContext.Provider>
  );
};
