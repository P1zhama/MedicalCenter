import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Alert,
  EmptyState,
  Field,
  PageSpinner,
  Pagination,
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
  const [page, setPage] = useState(1);

  const debouncedSearch = useDebounce(search);

  const { data, isLoading, error } = useAsync(
    () =>
      profilesApi.getDoctorCards({
        search: debouncedSearch || undefined,
        specializationId: specializationId || undefined,
        officeId: officeId || undefined,
        page,
      }),
    [debouncedSearch, specializationId, officeId, page],
  );

  const doctors = data?.items ?? null;

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
              onValueChange={(value) => {
                setSearch(value);
                setPage(1);
              }}
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
              onValueChange={(value) => {
                setSpecializationId(value);
                setPage(1);
              }}
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
              onValueChange={(value) => {
                setOfficeId(value);
                setPage(1);
              }}
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
        <>
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

        {data !== null ? (
          <Pagination
            page={data.page}
            pageSize={data.pageSize}
            totalCount={data.totalCount}
            onPageChange={setPage}
          />
        ) : null}
        </>
      )}
    </section>
  );
}
