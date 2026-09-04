import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Button,
  Card,
  DateInput,
  Field,
  Modal,
  PhotoUploader,
  TextInput,
  confirmMessages,
  formatDate,
  formatFullName,
  notificationMessages,
  profilesApi,
  todayIso,
  useForm,
  useToast,
  validatePastDate,
  validatePhoneNumber,
  validationMessages,
  type CreatePatientProfileRequest,
  type MatchedProfile,
} from '@mmc/shared';
import { required } from '@mmc/shared';
import styles from './CreateProfilePage.module.css';

export function CreateProfilePage() {
  const navigate = useNavigate();
  const toast = useToast();

  const [photoUrl, setPhotoUrl] = useState('');
  const [matched, setMatched] = useState<MatchedProfile | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const { values, errors, isValid, setValue, handleBlur, validateAll } = useForm({
    initialValues: { firstName: '', lastName: '', middleName: '', phoneNumber: '+', dateOfBirth: '' },
    validators: {
      firstName: required(validationMessages.firstNameRequired),
      lastName: required(validationMessages.lastNameRequired),
      phoneNumber: validatePhoneNumber,
      dateOfBirth: (value) => validatePastDate(value, validationMessages.dateRequired),
    },
  });

  const buildRequest = (): CreatePatientProfileRequest => ({
    firstName: values.firstName,
    lastName: values.lastName,
    middleName: values.middleName,
    phoneNumber: values.phoneNumber,
    dateOfBirth: values.dateOfBirth,
    photoUrl,
  });

  const finish = () => {
    toast.showSuccess('Your profile has been created');
    navigate('/profile');
  };

  const handleSubmit = async () => {
    if (!validateAll() || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setSubmitError(null);

    try {
      const response = await profilesApi.createMyPatientProfile(buildRequest());

      if (response.isMatched && response.matchedProfile !== null) {
        setMatched(response.matchedProfile);
      } else {
        finish();
      }
    } catch {
      setSubmitError(notificationMessages.unexpectedError);
    } finally {
      setIsSubmitting(false);
    }
  };

  const linkExisting = async () => {
    if (matched === null) {
      return;
    }

    setIsSubmitting(true);

    try {
      await profilesApi.linkMyPatientProfile(matched.profileId);
      setMatched(null);
      finish();
    } catch {
      setSubmitError(notificationMessages.unexpectedError);
      setIsSubmitting(false);
    }
  };

  const createNew = async () => {
    setIsSubmitting(true);

    try {
      await profilesApi.forceCreateMyPatientProfile(buildRequest());
      setMatched(null);
      finish();
    } catch {
      setSubmitError(notificationMessages.unexpectedError);
      setIsSubmitting(false);
    }
  };

  return (
    <section className={styles.page}>
      <h2 className={styles.title}>Create your profile</h2>

      <Card>
        {submitError !== null ? <Alert variant="error">{submitError}</Alert> : null}

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

        <Button fullWidth loading={isSubmitting} disabled={!isValid} onClick={() => void handleSubmit()}>
          Confirm
        </Button>
      </Card>

      {matched !== null ? (
        <Modal
          title="Similar profile found"
          onClose={() => setMatched(null)}
          showCloseButton={false}
          closeOnOverlayClick={false}
          footer={
            <>
              <Button variant="outline" onClick={() => void createNew()}>
                No, it&apos;s not me
              </Button>
              <Button onClick={() => void linkExisting()}>Yes, it&apos;s me</Button>
            </>
          }
        >
          <p className={styles.matchMessage}>{confirmMessages.similarProfileFound}</p>

          <dl className={styles.matchDetails}>
            <dt>Full name</dt>
            <dd>{formatFullName(matched)}</dd>
            <dt>Date of birth</dt>
            <dd>{formatDate(matched.dateOfBirth)}</dd>
          </dl>
        </Modal>
      ) : null}
    </section>
  );
}
