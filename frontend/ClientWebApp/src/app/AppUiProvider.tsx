import { useCallback, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import {
  AppointmentModal,
  appointmentsApi,
  notificationMessages,
  useAuth,
  useToast,
  type AppointmentFormValues,
} from '@mmc/shared';
import { AuthModals, type AuthModalMode } from '../features/auth/AuthModals';
import { AppUiContext, type AppUiValue } from './appUi';

export function AppUiProvider({ children }: { children: ReactNode }) {
  const { session } = useAuth();
  const toast = useToast();

  const [authMode, setAuthMode] = useState<AuthModalMode | null>(null);
  const [appointmentInitial, setAppointmentInitial] = useState<
    Partial<AppointmentFormValues> | null
  >(null);

  const openSignIn = useCallback(() => setAuthMode('signIn'), []);
  const openSignUp = useCallback(() => setAuthMode('signUp'), []);

  const openAppointment = useCallback(
    (initial?: Partial<AppointmentFormValues>) => {
      if (session === null) {
        toast.showInfo(notificationMessages.signInToMakeAppointment);
        setAuthMode('signIn');
        return;
      }

      setAppointmentInitial(initial ?? {});
    },
    [session, toast],
  );

  const value = useMemo<AppUiValue>(
    () => ({ openSignIn, openSignUp, openAppointment }),
    [openSignIn, openSignUp, openAppointment],
  );

  const handleSubmit = async (values: AppointmentFormValues) => {
    await appointmentsApi.createMyAppointment({
      serviceId: values.serviceId,
      doctorId: values.doctorId,
      officeId: values.officeId,
      date: values.date,
      startTime: values.startTime,
    });

    setAppointmentInitial(null);
    toast.showSuccess(notificationMessages.appointmentCreated);
  };

  return (
    <AppUiContext.Provider value={value}>
      {children}

      <AuthModals mode={authMode} onClose={() => setAuthMode(null)} onModeChange={setAuthMode} />

      {appointmentInitial !== null ? (
        <AppointmentModal
          initialValues={appointmentInitial}
          onSubmit={handleSubmit}
          onClose={() => setAppointmentInitial(null)}
        />
      ) : null}
    </AppUiContext.Provider>
  );
}
