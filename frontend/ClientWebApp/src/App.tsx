import { Navigate, Route, Routes } from 'react-router-dom';
import { AppFooter, PageShell, RequireAuth } from '@mmc/shared';
import { AppUiProvider } from './app/AppUiProvider';
import { Navbar } from './components/Navbar/Navbar';
import { SignInRequired } from './features/auth/SignInRequired';
import { AppointmentResultPage } from './pages/AppointmentResultPage/AppointmentResultPage';
import { ConfirmEmailPage } from './pages/ConfirmEmailPage/ConfirmEmailPage';
import { CreateProfilePage } from './pages/CreateProfilePage/CreateProfilePage';
import { DoctorPage } from './pages/DoctorPage/DoctorPage';
import { DoctorsPage } from './pages/DoctorsPage/DoctorsPage';
import { HomePage } from './pages/HomePage/HomePage';
import { ProfilePage } from './pages/ProfilePage/ProfilePage';
import { ServicesPage } from './pages/ServicesPage/ServicesPage';

export default function App() {
  return (
    <AppUiProvider>
      <PageShell header={<Navbar />} footer={<AppFooter />}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/doctors" element={<DoctorsPage />} />
          <Route path="/doctors/:id" element={<DoctorPage />} />
          <Route path="/services" element={<ServicesPage />} />
          <Route path="/confirm-email" element={<ConfirmEmailPage />} />

          <Route
            path="/profile"
            element={
              <RequireAuth fallback={<SignInRequired />}>
                <ProfilePage />
              </RequireAuth>
            }
          />
          <Route
            path="/profile/create"
            element={
              <RequireAuth fallback={<SignInRequired />}>
                <CreateProfilePage />
              </RequireAuth>
            }
          />
          <Route
            path="/appointments/:id/result"
            element={
              <RequireAuth fallback={<SignInRequired />}>
                <AppointmentResultPage />
              </RequireAuth>
            }
          />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </PageShell>
    </AppUiProvider>
  );
}
