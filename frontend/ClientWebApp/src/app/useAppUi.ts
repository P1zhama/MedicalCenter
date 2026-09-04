import { useContext } from 'react';
import { AppUiContext, type AppUiValue } from './appUi';

export function useAppUi(): AppUiValue {
  const context = useContext(AppUiContext);

  if (context === null) {
    throw new Error('useAppUi must be used within AppUiProvider');
  }

  return context;
}
