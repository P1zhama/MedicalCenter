import { useState } from 'react';
import {
  Alert,
  Button,
  ConfirmDialog,
  Field,
  Modal,
  PhotoUploader,
  Select,
  TextInput,
  confirmMessages,
  notificationMessages,
  profilesApi,
  required,
  useDictionaries,
  useForm,
  useToast,
  validateEmail,
  validationMessages,
  type ActivityStatus,
  type Receptionist,
} from '@mmc/shared';

const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export interface ReceptionistModalProps {
  receptionist: Receptionist | null;
  onSaved: () => void;
  onClose: () => void;
}

export function ReceptionistModal({ receptionist, onSaved, onClose }: ReceptionistModalProps) {
  const toast = useToast();
  const { offices } = useDictionaries();

  const isCreate = receptionist === null;

  const [photoUrl, setPhotoUrl] = useState(receptionist?.photoUrl ?? '');
  const [status, setStatus] = useState<ActivityStatus>(receptionist?.status ?? 'Active');
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  const { values, errors, isValid, setValue, setError: setFieldError, handleBlur, validateAll } =
    useForm({
      initialValues: {
        firstName: receptionist?.firstName ?? '',
        lastName: receptionist?.lastName ?? '',
        middleName: receptionist?.middleName ?? '',
        email: '',
        officeId: receptionist?.officeId ?? '',
      },
      validators: {
        firstName: required(validationMessages.firstNameRequired),
        lastName: required(validationMessages.lastNameRequired),
        email: (value) => (isCreate ? validateEmail(value) : null),
        officeId: required(validationMessages.officeRequired),
      },
    });

  const save = async () => {
    if (!validateAll() || isSaving) {
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      if (isCreate) {
        await profilesApi.createReceptionist({
          firstName: values.firstName,
          lastName: values.lastName,
          middleName: values.middleName,
          email: values.email,
          officeId: values.officeId,
          photoUrl,
        });

        toast.showSuccess(notificationMessages.createdSuccessfully);
      } else {
        await profilesApi.updateReceptionist(receptionist.id, {
          firstName: values.firstName,
          lastName: values.lastName,
          middleName: values.middleName,
          officeId: values.officeId,
          status,
          photoUrl,
        });

        toast.showSuccess(notificationMessages.savedSuccessfully);
      }

      onSaved();
    } catch (cause) {
      const conflict =
        typeof cause === 'object' && cause !== null && 'isConflict' in cause
          ? (cause as { isConflict: boolean }).isConflict
          : false;

      if (conflict && isCreate) {
        setFieldError('email', validationMessages.emailAlreadyExists);
      } else {
        setError(notificationMessages.unexpectedError);
      }

      setIsSaving(false);
    }
  };

  return (
    <>
      <Modal
        title={isCreate ? 'Create receptionist' : 'Edit receptionist'}
        size="medium"
        onClose={() => setIsCancelling(true)}
        closeOnOverlayClick={false}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsCancelling(true)}>
              Cancel
            </Button>
            <Button loading={isSaving} disabled={!isValid} onClick={() => void save()}>
              {isCreate ? 'Confirm' : 'Save changes'}
            </Button>
          </>
        }
      >
        {error !== null ? <Alert variant="error">{error}</Alert> : null}

        <PhotoUploader value={photoUrl} kind="ReceptionistPhoto" onValueChange={setPhotoUrl} />

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

        {isCreate ? (
          <Field label="E-mail" required error={errors.email}>
            {({ id, hasError, describedBy }) => (
              <TextInput
                id={id}
                type="email"
                value={values.email}
                invalid={hasError}
                aria-describedby={describedBy}
                onValueChange={(value) => setValue('email', value)}
                onBlur={() => handleBlur('email')}
              />
            )}
          </Field>
        ) : null}

        <Field label="Office" required error={errors.officeId}>
          {({ id, hasError }) => (
            <Select
              id={id}
              value={values.officeId}
              options={offices.map((office) => ({ value: office.id, label: office.address }))}
              placeholder="Choose the office"
              invalid={hasError}
              onValueChange={(value) => setValue('officeId', value)}
              onBlur={() => handleBlur('officeId')}
            />
          )}
        </Field>

        {!isCreate ? (
          <Field label="Status" required>
            {({ id }) => (
              <Select
                id={id}
                value={status}
                options={STATUS_OPTIONS}
                onValueChange={(value) => setStatus(value as ActivityStatus)}
              />
            )}
          </Field>
        ) : null}
      </Modal>

      {isCancelling ? (
        <ConfirmDialog
          message={isCreate ? confirmMessages.cancelEntered : confirmMessages.cancelChanges}
          onConfirm={onClose}
          onCancel={() => setIsCancelling(false)}
        />
      ) : null}
    </>
  );
}
