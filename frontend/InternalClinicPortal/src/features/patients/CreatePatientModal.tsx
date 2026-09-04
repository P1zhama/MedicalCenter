import { useState } from 'react';
import {
  Alert,
  Button,
  ConfirmDialog,
  DateInput,
  Field,
  Modal,
  TextInput,
  confirmMessages,
  notificationMessages,
  profilesApi,
  required,
  todayIso,
  useForm,
  validatePastDate,
  validationMessages,
} from '@mmc/shared';

export interface CreatePatientModalProps {
  onCreated: (patientId: string, fullName: string) => void;
  onClose: () => void;
}

export function CreatePatientModal({ onCreated, onClose }: CreatePatientModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  const { values, errors, isValid, setValue, handleBlur, validateAll } = useForm({
    initialValues: { firstName: '', lastName: '', middleName: '', dateOfBirth: '' },
    validators: {
      firstName: required(validationMessages.firstNameRequired),
      lastName: required(validationMessages.lastNameRequired),
      dateOfBirth: (value) => validatePastDate(value, validationMessages.dateRequired),
    },
  });

  const submit = async () => {
    if (!validateAll() || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const response = await profilesApi.createPatientByReceptionist({
        firstName: values.firstName,
        lastName: values.lastName,
        middleName: values.middleName,
        dateOfBirth: values.dateOfBirth,
      });

      onCreated(
        response.profileId,
        [values.lastName, values.firstName, values.middleName].filter((part) => part !== '').join(' '),
      );
    } catch {
      setError(notificationMessages.unexpectedError);
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <Modal
        title="Create patient"
        onClose={() => setIsCancelling(true)}
        closeOnOverlayClick={false}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsCancelling(true)}>
              Cancel
            </Button>
            <Button loading={isSubmitting} disabled={!isValid} onClick={() => void submit()}>
              Confirm
            </Button>
          </>
        }
      >
        {error !== null ? <Alert variant="error">{error}</Alert> : null}

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
      </Modal>

      {isCancelling ? (
        <ConfirmDialog
          message={confirmMessages.cancelEntered}
          onConfirm={onClose}
          onCancel={() => setIsCancelling(false)}
        />
      ) : null}
    </>
  );
}
