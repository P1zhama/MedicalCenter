import { Card, Hero, useAuth } from '@mmc/shared';
import styles from './HomePage.module.css';

const DOCTOR_SECTIONS = ['My schedule', 'Patients', 'Appointment results'];
const RECEPTIONIST_SECTIONS = ['Appointments', 'Doctors', 'Patients', 'Receptionists', 'Offices', 'Specializations'];

export function HomePage() {
  const { session } = useAuth();
  const sections = session?.role === 'Doctor' ? DOCTOR_SECTIONS : RECEPTIONIST_SECTIONS;

  return (
    <Hero title={`Signed in as ${session?.role ?? 'staff'}`}>
      <Card title="Coming next">
        <ul className={styles.sections}>
          {sections.map((section) => (
            <li key={section}>{section}</li>
          ))}
        </ul>
      </Card>
    </Hero>
  );
}
