import { useState } from 'react';
import {
  Alert,
  Button,
  ConfirmDialog,
  Field,
  Modal,
  PageSpinner,
  Textarea,
  appointmentsApi,
  confirmMessages,
  formatDate,
  formatFullName,
  formatTimeRange,
  notificationMessages,
  required,
  useAsync,
  useForm,
  useToast,
  validationMessages,
  type AppointmentListItem,
} from '@mmc/shared';
import styles from './ResultModal.module.css';

export interface ResultModalProps {
  appointment: AppointmentListItem;
  onClose: () => void;
  onSaved: () => void;
}

export function ResultModal({ appointment, onClose, onSaved }: ResultModalProps) {
  const toast = useToast();

  const { data: existing, isLoading, error } = useAsync(
    () => appointmentsApi.getResult(appointment.id),
    [appointment.id],
  );

  const hasResult = existing !== null && error === null;

  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  const { values, errors, isValid, setValue, handleBlur, validateAll } = useForm({
    initialValues: {
      complaints: existing?.complaints ?? '',
      conclusion: existing?.conclusion ?? '',
      recommendations: existing?.recommendations ?? '',
      diagnosis: existing?.diagnosis ?? '',
    },
    validators: {
      complaints: required(validationMessages.complaintsRequired),
      conclusion: required(validationMessages.conclusionRequired),
      recommendations: required(validationMessages.recommendationsRequired),
    },
  });

  const editMode = !hasResult || isEditing;

  const save = async () => {
    if (!validateAll() || isSaving) {
      return;
    }

    setIsSaving(true);
    setSaveError(null);

    const payload = {
      complaints: values.complaints,
      conclusion: values.conclusion,
      recommendations: values.recommendations,
      diagnosis: values.diagnosis,
    };

    try {
      if (hasResult) {
        await appointmentsApi.updateResult(appointment.id, payload);
      } else {
        await appointmentsApi.createResult(appointment.id, payload);
      }

      toast.showSuccess(notificationMessages.resultSaved);
      onSaved();
    } catch {
      setSaveError(notificationMessages.unexpectedError);
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <Modal title="Appointment result" onClose={onClose}>
        <PageSpinner />
      </Modal>
    );
  }

  return (
    <>
      <Modal
        title="Appointment result"
        size="medium"
        onClose={editMode ? () => setIsCancelling(true) : onClose}
        closeOnOverlayClick={false}
        footer={
          editMode ? (
            <>
              <Button variant="outline" onClick={() => setIsCancelling(true)}>
                Cancel
              </Button>
              <Button loading={isSaving} disabled={!isValid} onClick={() => void save()}>
                {hasResult ? 'Save changes' : 'Confirm'}
              </Button>
            </>
          ) : (
            <Button variant="outline" onClick={() => setIsEditing(true)}>
              Edit
            </Button>
          )
        }
      >
        {saveError !== null ? <Alert variant="error">{saveError}</Alert> : null}

        <dl className={styles.summary}>
          <dt>Date</dt>
          <dd>
            {formatDate(appointment.date)},{' '}
            {formatTimeRange(appointment.startTime, appointment.endTime)}
          </dd>
          <dt>Patient</dt>
          <dd>
            {formatFullName({
              firstName: appointment.patientFirstName,
              lastName: appointment.patientLastName,
              middleName: appointment.patientMiddleName,
            })}
          </dd>
          <dt>Service</dt>
          <dd>{appointment.serviceName}</dd>
        </dl>

        {editMode ? (
          <>
            <Field label="Complaints" required error={errors.complaints}>
              {({ id, hasError, describedBy }) => (
                <Textarea
                  id={id}
                  value={values.complaints}
                  invalid={hasError}
                  aria-describedby={describedBy}
                  onValueChange={(value) => setValue('complaints', value)}
                  onBlur={() => handleBlur('complaints')}
                />
              )}
            </Field>

            <Field label="Diagnosis">
              {({ id }) => (
                <Textarea
                  id={id}
                  value={values.diagnosis}
                  onValueChange={(value) => setValue('diagnosis', value)}
                />
              )}
            </Field>

            <Field label="Conclusion" required error={errors.conclusion}>
              {({ id, hasError, describedBy }) => (
                <Textarea
                  id={id}
                  value={values.conclusion}
                  invalid={hasError}
                  aria-describedby={describedBy}
                  onValueChange={(value) => setValue('conclusion', value)}
                  onBlur={() => handleBlur('conclusion')}
                />
              )}
            </Field>

            <Field label="Recommendations" required error={errors.recommendations}>
              {({ id, hasError, describedBy }) => (
                <Textarea
                  id={id}
                  value={values.recommendations}
                  invalid={hasError}
                  aria-describedby={describedBy}
                  onValueChange={(value) => setValue('recommendations', value)}
                  onBlur={() => handleBlur('recommendations')}
                />
              )}
            </Field>
          </>
        ) : (
          <div className={styles.sections}>
            <section>
              <h3 className={styles.sectionTitle}>Complaints</h3>
              <p>{existing?.complaints}</p>
            </section>

            {existing !== null && existing.diagnosis.length > 0 ? (
              <section>
                <h3 className={styles.sectionTitle}>Diagnosis</h3>
                <p>{existing.diagnosis}</p>
              </section>
            ) : null}

            <section>
              <h3 className={styles.sectionTitle}>Conclusion</h3>
              <p>{existing?.conclusion}</p>
            </section>

            <section>
              <h3 className={styles.sectionTitle}>Recommendations</h3>
              <p>{existing?.recommendations}</p>
            </section>
          </div>
        )}
      </Modal>

      {isCancelling ? (
        <ConfirmDialog
          message={hasResult ? confirmMessages.cancelChanges : confirmMessages.cancelEntered}
          onConfirm={onClose}
          onCancel={() => setIsCancelling(false)}
        />
      ) : null}
    </>
  );
}
