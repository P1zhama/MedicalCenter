import { Link, NavLink } from 'react-router-dom';
import { AppHeader, Button, useAuth } from '@mmc/shared';
import { useAppUi } from '../../app/useAppUi';
import styles from './Navbar.module.css';

const LINKS = [
  { to: '/doctors', label: 'Doctors' },
  { to: '/services', label: 'Services' },
];

export function Navbar() {
  const { session, isRestoring, signOut } = useAuth();
  const { openSignIn, openSignUp, openAppointment } = useAppUi();

  return (
    <AppHeader
      brand={
        <Link to="/">
          <h1>Medical Center</h1>
        </Link>
      }
    >
      <nav className={styles.links}>
        {LINKS.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            className={({ isActive }) => `${styles.link} ${isActive ? styles.active : ''}`}
          >
            {link.label}
          </NavLink>
        ))}

        {session !== null ? (
          <NavLink
            to="/profile"
            className={({ isActive }) => `${styles.link} ${isActive ? styles.active : ''}`}
          >
            My profile
          </NavLink>
        ) : null}
      </nav>

      <Button variant="white" onClick={() => openAppointment()}>
        Make an appointment
      </Button>

      {isRestoring ? null : session === null ? (
        <>
          <Button variant="outlineWhite" onClick={openSignIn}>
            Sign in
          </Button>
          <Button variant="outlineWhite" onClick={openSignUp}>
            Sign up
          </Button>
        </>
      ) : (
        <Button variant="outlineWhite" onClick={() => void signOut()}>
          Sign out
        </Button>
      )}
    </AppHeader>
  );
}
