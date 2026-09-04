import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth, useAuth } from '@mmc/shared';
import { AppLayout } from './components/AppLayout/AppLayout';
import { AccessDenied } from './features/auth/AccessDenied';
import { SignInGate } from './features/auth/SignInGate';
import { AppointmentsPage } from './pages/AppointmentsPage/AppointmentsPage';
import { DoctorsPage } from './pages/DoctorsPage/DoctorsPage';
import { OfficesPage } from './pages/OfficesPage/OfficesPage';
import { PatientPage } from './pages/PatientPage/PatientPage';
import { PatientsPage } from './pages/PatientsPage/PatientsPage';
import { ReceptionistsPage } from './pages/ReceptionistsPage/ReceptionistsPage';
import { SchedulePage } from './pages/SchedulePage/SchedulePage';
import { SpecializationsPage } from './pages/SpecializationsPage/SpecializationsPage';
import { StaffProfilePage } from './pages/StaffProfilePage/StaffProfilePage';

const STAFF_ROLES = ['Doctor', 'Receptionist'] as const;

function PortalRoutes() {
  const { session } = useAuth();
  const isDoctor = session?.role === 'Doctor';
  const home = isDoctor ? '/schedule' : '/appointments';

  return (
    <Routes>
      <Route path="/" element={<Navigate to={home} replace />} />
      <Route path="/profile" element={<StaffProfilePage />} />
      <Route path="/patients/:id" element={<PatientPage />} />

      {isDoctor ? (
        <Route path="/schedule" element={<SchedulePage />} />
      ) : (
        <>
          <Route path="/appointments" element={<AppointmentsPage />} />
          <Route path="/doctors" element={<DoctorsPage />} />
          <Route path="/patients" element={<PatientsPage />} />
          <Route path="/receptionists" element={<ReceptionistsPage />} />
          <Route path="/offices" element={<OfficesPage />} />
          <Route path="/specializations" element={<SpecializationsPage />} />
        </>
      )}

      <Route path="*" element={<Navigate to={home} replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <RequireAuth fallback={<SignInGate />} forbidden={<AccessDenied />} allowedRoles={STAFF_ROLES}>
      <AppLayout>
        <PortalRoutes />
      </AppLayout>
    </RequireAuth>
  );
}
