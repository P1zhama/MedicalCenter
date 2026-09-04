import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Alert,
  EmptyState,
  Field,
  PageSpinner,
  Select,
  TextInput,
  formatFullName,
  notificationMessages,
  profilesApi,
  useAsync,
  useDebounce,
  useDictionaries,
} from '@mmc/shared';
import styles from './DoctorsPage.module.css';

export function DoctorsPage() {
  const { offices, specializations, officeAddress, specializationName } = useDictionaries();

  const [search, setSearch] = useState('');
  const [specializationId, setSpecializationId] = useState('');
  const [officeId, setOfficeId] = useState('');

  const debouncedSearch = useDebounce(search);

  const { data: doctors, isLoading, error } = useAsync(
    () =>
      profilesApi.getDoctorCards({
        search: debouncedSearch || undefined,
        specializationId: specializationId || undefined,
        officeId: officeId || undefined,
      }),
    [debouncedSearch, specializationId, officeId],
  );

  return (
    <section>
      <h2 className={styles.title}>Our doctors</h2>

      <div className={styles.filters}>
        <Field label="Search by name">
          {({ id }) => (
            <TextInput
              id={id}
              value={search}
              placeholder="Enter the doctor's name"
              onValueChange={setSearch}
            />
          )}
        </Field>

        <Field label="Specialization">
          {({ id }) => (
            <Select
              id={id}
              value={specializationId}
              placeholder="All specializations"
              options={specializations.map((item) => ({ value: item.id, label: item.name }))}
              onValueChange={setSpecializationId}
            />
          )}
        </Field>

        <Field label="Office">
          {({ id }) => (
            <Select
              id={id}
              value={officeId}
              placeholder="All offices"
              options={offices.map((item) => ({ value: item.id, label: item.address }))}
              onValueChange={setOfficeId}
            />
          )}
        </Field>
      </div>

      {error !== null ? <Alert variant="error">{notificationMessages.unexpectedError}</Alert> : null}

      {isLoading ? (
        <PageSpinner label="Loading doctors" />
      ) : doctors === null || doctors.length === 0 ? (
        <EmptyState message="There are no doctors matching this filtration" />
      ) : (
        <div className={styles.cards}>
          {doctors.map((doctor) => (
            <Link key={doctor.id} to={`/doctors/${doctor.id}`} className={styles.card}>
              <div className={styles.photo}>
                {doctor.photoUrl.length > 0 ? (
                  <img src={doctor.photoUrl} alt="" className={styles.image} />
                ) : (
                  <span className={styles.placeholder} aria-hidden="true" />
                )}
              </div>

              <div className={styles.info}>
                <h3 className={styles.name}>{formatFullName(doctor)}</h3>
                <p className={styles.specialization}>
                  {specializationName(doctor.specializationId)}
                </p>
                <p className={styles.meta}>Experience: {doctor.experienceYears} years</p>
                <p className={styles.meta}>{officeAddress(doctor.officeId)}</p>
              </div>
            </Link>
          ))}
        </div>
      )}
    </section>
  );
}
