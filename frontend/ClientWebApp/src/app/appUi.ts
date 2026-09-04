import { createContext } from 'react';
import type { AppointmentFormValues } from '@mmc/shared';

export interface AppUiValue {
  openSignIn: () => void;
  openSignUp: () => void;
  openAppointment: (initial?: Partial<AppointmentFormValues>) => void;
}

export const AppUiContext = createContext<AppUiValue | null>(null);
