import { useState } from 'react';
import {
  Alert,
  Button,
  Card,
  ConfirmDialog,
  DateInput,
  Field,
  PhotoUploader,
  TextInput,
  confirmMessages,
  formatDate,
  notificationMessages,
  profilesApi,
  required,
  todayIso,
  useForm,
  useToast,
  validatePastDate,
  validatePhoneNumber,
  validationMessages,
  type Patient,
} from '@mmc/shared';
import styles from './ProfilePage.module.css';

export interface PersonalInfoTabProps {
  patient: Patient;
  onSaved: () => void;
}

export function PersonalInfoTab({ patient, onSaved }: PersonalInfoTabProps) {
  const toast = useToast();

  const [isEditing, setIsEditing] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [photoUrl, setPhotoUrl] = useState(patient.photoUrl);

  const { values, errors, isValid, setValue, handleBlur, validateAll, reset } = useForm({
    initialValues: {
      firstName: patient.firstName,
      lastName: patient.lastName,
      middleName: patient.middleName,
      phoneNumber: patient.phoneNumber,
      dateOfBirth: patient.dateOfBirth,
    },
    validators: {
      firstName: required(validationMessages.firstNameRequired),
      lastName: required(validationMessages.lastNameRequired),
      phoneNumber: validatePhoneNumber,
      dateOfBirth: (value) => validatePastDate(value, validationMessages.dateRequired),
    },
  });

  const save = async () => {
    if (!validateAll() || isSaving) {
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      await profilesApi.updateMyPatientProfile({
        firstName: values.firstName,
        lastName: values.lastName,
        middleName: values.middleName,
        phoneNumber: values.phoneNumber,
        dateOfBirth: values.dateOfBirth,
        photoUrl,
      });

      setIsEditing(false);
      toast.showSuccess('Your profile has been updated');
      onSaved();
    } catch {
      setError(notificationMessages.unexpectedError);
    } finally {
      setIsSaving(false);
    }
  };

  const discard = () => {
    reset({
      firstName: patient.firstName,
      lastName: patient.lastName,
      middleName: patient.middleName,
      phoneNumber: patient.phoneNumber,
      dateOfBirth: patient.dateOfBirth,
    });
    setPhotoUrl(patient.photoUrl);
    setIsEditing(false);
    setIsCancelling(false);
  };

  if (!isEditing) {
    return (
      <Card>
        <div className={styles.viewHeader}>
          <div className={styles.avatar}>
            {patient.photoUrl.length > 0 ? (
              <img src={patient.photoUrl} alt="" className={styles.avatarImage} />
            ) : (
              <span className={styles.avatarPlaceholder} aria-hidden="true" />
            )}
          </div>

          <dl className={styles.details}>
            <dt>First name</dt>
            <dd>{patient.firstName}</dd>
            <dt>Last name</dt>
            <dd>{patient.lastName}</dd>
            <dt>Middle name</dt>
            <dd>{patient.middleName.length > 0 ? patient.middleName : '—'}</dd>
            <dt>Phone number</dt>
            <dd>{patient.phoneNumber}</dd>
            <dt>Date of birth</dt>
            <dd>{formatDate(patient.dateOfBirth)}</dd>
          </dl>
        </div>

        <Button variant="outline" onClick={() => setIsEditing(true)}>
          Edit
        </Button>
      </Card>
    );
  }

  return (
    <Card>
      {error !== null ? <Alert variant="error">{error}</Alert> : null}

      <PhotoUploader value={photoUrl} kind="PatientPhoto" onValueChange={setPhotoUrl} />

      <Field label="First name" required error={errors.firstName}>
        {({ id, hasError, describedBy }) => (
          <TextInput
            id={id}
            value={values.firstName}
            invalid={hasError}
            aria-describedby={describedBy}
            onValueChange={(value) => setValue('firstName', value)}
            onBlur={() => handleBlur('firstName')}
          />
        )}
      </Field>

      <Field label="Last name" required error={errors.lastName}>
        {({ id, hasError, describedBy }) => (
          <TextInput
            id={id}
            value={values.lastName}
            invalid={hasError}
            aria-describedby={describedBy}
            onValueChange={(value) => setValue('lastName', value)}
            onBlur={() => handleBlur('lastName')}
          />
        )}
      </Field>

      <Field label="Middle name">
        {({ id }) => (
          <TextInput
            id={id}
            value={values.middleName}
            onValueChange={(value) => setValue('middleName', value)}
          />
        )}
      </Field>

      <Field label="Phone number" required error={errors.phoneNumber}>
        {({ id, hasError, describedBy }) => (
          <TextInput
            id={id}
            value={values.phoneNumber}
            invalid={hasError}
            aria-describedby={describedBy}
            onValueChange={(value) =>
              setValue('phoneNumber', value.startsWith('+') ? value : `+${value}`)
            }
            onBlur={() => handleBlur('phoneNumber')}
          />
        )}
      </Field>

      <Field label="Date of birth" required error={errors.dateOfBirth}>
        {({ id, hasError, describedBy }) => (
          <DateInput
            id={id}
            value={values.dateOfBirth}
            max={todayIso()}
            invalid={hasError}
            aria-describedby={describedBy}
            onValueChange={(value) => setValue('dateOfBirth', value)}
            onBlur={() => handleBlur('dateOfBirth')}
          />
        )}
      </Field>

      <div className={styles.formFooter}>
        <Button variant="outline" onClick={() => setIsCancelling(true)}>
          Cancel
        </Button>
        <Button loading={isSaving} disabled={!isValid} onClick={() => void save()}>
          Save changes
        </Button>
      </div>

      {isCancelling ? (
        <ConfirmDialog
          message={confirmMessages.cancelChanges}
          onConfirm={discard}
          onCancel={() => setIsCancelling(false)}
        />
      ) : null}
    </Card>
  );
}
