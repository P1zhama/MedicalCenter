import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { Alert, PageSpinner, Tabs, profilesApi, useAsync } from '@mmc/shared';
import { AppointmentsTab } from './AppointmentsTab';
import { PersonalInfoTab } from './PersonalInfoTab';
import styles from './ProfilePage.module.css';

const TABS = [
  { id: 'personal', label: 'Personal information' },
  { id: 'appointments', label: 'Appointment results' },
];

export function ProfilePage() {
  const [activeTab, setActiveTab] = useState('personal');
  const { data: patient, isLoading, error, reload } = useAsync(
    () => profilesApi.getMyPatientProfile(),
    [],
  );

  if (isLoading) {
    return <PageSpinner label="Loading profile" />;
  }

  if (error !== null && error.isNotFound) {
    return <Navigate to="/profile/create" replace />;
  }

  if (error !== null || patient === null) {
    return <Alert variant="error">Could not load your profile. Please try again later.</Alert>;
  }

  return (
    <section>
      <h2 className={styles.title}>My profile</h2>

      <Tabs items={TABS} activeId={activeTab} onChange={setActiveTab} />

      {activeTab === 'personal' ? (
        <PersonalInfoTab patient={patient} onSaved={reload} />
      ) : (
        <AppointmentsTab />
      )}
    </section>
  );
}
