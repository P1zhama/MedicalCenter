import type { ReactNode } from 'react';
import { NavLink } from 'react-router-dom';
import { AppFooter, AppHeader, Button, useAuth } from '@mmc/shared';
import styles from './AppLayout.module.css';

interface MenuItem {
  to: string;
  label: string;
}

const RECEPTIONIST_MENU: MenuItem[] = [
  { to: '/appointments', label: 'Appointments' },
  { to: '/doctors', label: 'Doctors' },
  { to: '/patients', label: 'Patients' },
  { to: '/receptionists', label: 'Receptionists' },
  { to: '/offices', label: 'Offices' },
  { to: '/specializations', label: 'Specializations' },
];

const DOCTOR_MENU: MenuItem[] = [{ to: '/schedule', label: 'My schedule' }];

export interface AppLayoutProps {
  children: ReactNode;
}

export function AppLayout({ children }: AppLayoutProps) {
  const { session, signOut } = useAuth();

  const menu = session?.role === 'Doctor' ? DOCTOR_MENU : RECEPTIONIST_MENU;

  return (
    <div className={styles.layout}>
      <AppHeader brand={<h1>Medical Center</h1>} subtitle="Clinic portal">
        {session?.role != null ? <span className={styles.role}>{session.role}</span> : null}

        <NavLink to="/profile" className={styles.profileLink}>
          My profile
        </NavLink>

        <Button variant="outlineWhite" onClick={() => void signOut()}>
          Sign out
        </Button>
      </AppHeader>

      <div className={styles.content}>
        <nav className={styles.sidebar}>
          {menu.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) => `${styles.menuItem} ${isActive ? styles.active : ''}`}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <main className={styles.main}>{children}</main>
      </div>

      <AppFooter />
    </div>
  );
}
