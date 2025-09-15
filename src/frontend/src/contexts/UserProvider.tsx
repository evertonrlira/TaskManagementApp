import { createContext, useContext, useState } from 'react';
import type { ReactNode } from 'react';

interface UserContextValue {
  currentUserId: string | null;
  setCurrentUserId: (userId: string | null) => void;
  isUserSelected: boolean;
}

const UserContext = createContext<UserContextValue | undefined>(undefined);

interface UserProviderProps {
  children: ReactNode;
}

export function UserProvider({ children }: UserProviderProps) {
  const [currentUserId, setCurrentUserId] = useState<string | null>(null);

  const value: UserContextValue = {
    currentUserId,
    setCurrentUserId,
    isUserSelected: currentUserId !== null,
  };

  return (
    <UserContext.Provider value={value}>
      {children}
    </UserContext.Provider>
  );
}

export function useUser(): UserContextValue {
  const context = useContext(UserContext);
  if (context === undefined) {
    throw new Error('useUser must be used within a UserProvider');
  }
  return context;
}

// Hook for components that require a selected user
export function useRequiredUser(): { currentUserId: string } {
  const { currentUserId, isUserSelected } = useUser();
  
  if (!isUserSelected || !currentUserId) {
    throw new Error('This component requires a user to be selected');
  }
  
  return { currentUserId };
}
