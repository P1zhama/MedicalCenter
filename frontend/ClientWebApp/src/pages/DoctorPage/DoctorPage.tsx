import { Link, useParams } from 'react-router-dom';
import {
  Alert,
  Button,
  Card,
  PageSpinner,
  formatFullName,
  formatPrice,
  notificationMessages,
  profilesApi,
  useAsync,
  useDictionaries,
} from '@mmc/shared';
import { useAppUi } from '../../app/useAppUi';
import styles from './DoctorPage.module.css';

export function DoctorPage() {
  const { id = '' } = useParams();
  const { officeAddress, specializationName, catalog } = useDictionaries();
  const { openAppointment } = useAppUi();

  const { data: doctor, isLoading, error } = useAsync(
    () => profilesApi.getDoctorCardById(id),
    [id],
  );

  if (isLoading) {
    return <PageSpinner label="Loading doctor" />;
  }

  if (error !== null || doctor === null) {
    return <Alert variant="error">{notificationMessages.unexpectedError}</Alert>;
  }

  const services = (catalog?.categories ?? []).flatMap((category) =>
    category.specializations
      .filter((specialization) => specialization.id === doctor.specializationId)
      .flatMap((specialization) =>
        specialization.services.map((service) => ({ ...service, categoryName: category.name })),
      ),
  );

  return (
    <section>
      <Link to="/doctors" className={styles.back}>
        &larr; Back to all doctors
      </Link>

      <div className={styles.header}>
        <div className={styles.photo}>
          {doctor.photoUrl.length > 0 ? (
            <img src={doctor.photoUrl} alt="" className={styles.image} />
          ) : (
            <span className={styles.placeholder} aria-hidden="true" />
          )}
        </div>

        <div>
          <h2 className={styles.name}>{formatFullName(doctor)}</h2>
          <p className={styles.specialization}>{specializationName(doctor.specializationId)}</p>
          <p className={styles.meta}>Experience: {doctor.experienceYears} years</p>
          <p className={styles.meta}>{officeAddress(doctor.officeId)}</p>

          <div className={styles.action}>
            <Button
              onClick={() =>
                openAppointment({
                  doctorId: doctor.id,
                  specializationId: doctor.specializationId,
                  officeId: doctor.officeId,
                })
              }
            >
              Make an appointment with the doctor
            </Button>
          </div>
        </div>
      </div>

      <Card title="Services">
        {services.length === 0 ? (
          <p className={styles.meta}>There are no active services for this specialization</p>
        ) : (
          <ul className={styles.services}>
            {services.map((service) => (
              <li key={service.id} className={styles.service}>
                <span>
                  {service.name}
                  <span className={styles.category}> · {service.categoryName}</span>
                </span>
                <span className={styles.price}>{formatPrice(service.price)}</span>
              </li>
            ))}
          </ul>
        )}
      </Card>
    </section>
  );
}
