import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Alert,
  Button,
  Card,
  PageSpinner,
  appointmentsApi,
  documentsApi,
  formatDate,
  formatFullName,
  formatTimeRange,
  notificationMessages,
  useAsync,
  useToast,
} from '@mmc/shared';
import styles from './AppointmentResultPage.module.css';

export function AppointmentResultPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const toast = useToast();

  const [isDownloading, setIsDownloading] = useState(false);
  const [isEmailing, setIsEmailing] = useState(false);

  const { data: result, isLoading, error } = useAsync(() => appointmentsApi.getResult(id), [id]);

  const download = async () => {
    setIsDownloading(true);

    try {
      const blob = await documentsApi.downloadResultPdf(id);
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');

      link.href = url;
      link.download = `appointment-result-${id}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch {
      toast.showError('Could not download the result');
    } finally {
      setIsDownloading(false);
    }
  };

  const sendEmail = async () => {
    setIsEmailing(true);

    try {
      await documentsApi.emailResult(id);
      toast.showSuccess('The result has been sent to your email');
    } catch {
      toast.showError('Could not send the result');
    } finally {
      setIsEmailing(false);
    }
  };

  if (isLoading) {
    return <PageSpinner label="Loading result" />;
  }

  if (error !== null && error.isNotFound) {
    return (
      <Card title="Appointment result">
        <Alert variant="info">The result for this appointment has not been added yet</Alert>
        <Button variant="outline" onClick={() => navigate(-1)}>
          Back
        </Button>
      </Card>
    );
  }

  if (error !== null || result === null) {
    return <Alert variant="error">{notificationMessages.unexpectedError}</Alert>;
  }

  return (
    <section className={styles.page}>
      <h2 className={styles.title}>Appointment result</h2>

      <Card>
        <dl className={styles.details}>
          <dt>Date</dt>
          <dd>
            {formatDate(result.date)}, {formatTimeRange(result.startTime, result.endTime)}
          </dd>

          <dt>Patient</dt>
          <dd>
            {formatFullName({
              firstName: result.patientFirstName,
              lastName: result.patientLastName,
              middleName: result.patientMiddleName,
            })}
          </dd>

          <dt>Date of birth</dt>
          <dd>{formatDate(result.patientDateOfBirth)}</dd>

          <dt>Doctor</dt>
          <dd>
            {formatFullName({
              firstName: result.doctorFirstName,
              lastName: result.doctorLastName,
              middleName: result.doctorMiddleName,
            })}
          </dd>

          <dt>Specialization</dt>
          <dd>{result.specializationName}</dd>

          <dt>Service</dt>
          <dd>{result.serviceName}</dd>
        </dl>

        <div className={styles.sections}>
          <section>
            <h3 className={styles.sectionTitle}>Complaints</h3>
            <p>{result.complaints}</p>
          </section>

          {result.diagnosis.length > 0 ? (
            <section>
              <h3 className={styles.sectionTitle}>Diagnosis</h3>
              <p>{result.diagnosis}</p>
            </section>
          ) : null}

          <section>
            <h3 className={styles.sectionTitle}>Conclusion</h3>
            <p>{result.conclusion}</p>
          </section>

          <section>
            <h3 className={styles.sectionTitle}>Recommendations</h3>
            <p>{result.recommendations}</p>
          </section>
        </div>

        <div className={styles.actions}>
          <Button variant="outline" onClick={() => navigate(-1)}>
            Back
          </Button>
          <Button variant="outline" loading={isEmailing} onClick={() => void sendEmail()}>
            Send to email
          </Button>
          <Button loading={isDownloading} onClick={() => void download()}>
            Download
          </Button>
        </div>
      </Card>
    </section>
  );
}
