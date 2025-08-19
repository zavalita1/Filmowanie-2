import React from 'react';

interface TabContentProps {
  activeTab: number;
}

const TabContent: React.FC<TabContentProps> = ({ activeTab }) => {
  return (
    <div className="p-4">
      {activeTab === 0 && (
        <div className="form-control w-full max-w-xs">
          <div className="relative">
            <input
              type="text"
              placeholder=" "
              className="input input-bordered w-full pt-4"
            />
            <label className="absolute text-sm text-gray-500 duration-300 transform -translate-y-4 scale-75 top-2 z-10 origin-[0] bg-white dark:bg-gray-900 px-2 peer-focus:px-2 peer-placeholder-shown:scale-100 peer-placeholder-shown:-translate-y-1/2 peer-placeholder-shown:top-1/2 peer-focus:top-2 peer-focus:scale-75 peer-focus:-translate-y-4 left-1">
              AAA
            </label>
          </div>
        </div>
      )}
      {activeTab === 1 && (
        <div className="text-center">Content for Tab 2</div>
      )}
    </div>
  );
};

export default TabContent;
