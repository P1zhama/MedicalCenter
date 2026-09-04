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
  officesApi,
  required,
  useForm,
  useToast,
  validatePhoneNumber,
  validationMessages,
  type ActivityStatus,
  type Office,
} from '@mmc/shared';

const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export interface OfficeModalProps {
  office: Office | null;
  onSaved: () => void;
  onClose: () => void;
}

export function OfficeModal({ office, onSaved, onClose }: OfficeModalProps) {
  const toast = useToast();

  const [photoUrl, setPhotoUrl] = useState(office?.photoUrl ?? '');
  const [status, setStatus] = useState<ActivityStatus>(office?.status ?? 'Active');
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  const { values, errors, isValid, setValue, handleBlur, validateAll } = useForm({
    initialValues: {
      city: office?.city ?? '',
      street: office?.street ?? '',
      houseNumber: office?.houseNumber ?? '',
      officeNumber: office?.officeNumber ?? '',
      registryPhoneNumber: office?.registryPhoneNumber ?? '+',
    },
    validators: {
      city: required(validationMessages.cityRequired),
      street: required(validationMessages.streetRequired),
      houseNumber: required(validationMessages.houseNumberRequired),
      registryPhoneNumber: validatePhoneNumber,
    },
  });

  const save = async () => {
    if (!validateAll() || isSaving) {
      return;
    }

    setIsSaving(true);
    setError(null);

    const payload = {
      city: values.city,
      street: values.street,
      houseNumber: values.houseNumber,
      officeNumber: values.officeNumber,
      registryPhoneNumber: values.registryPhoneNumber,
      photoUrl,
      status,
    };

    try {
      if (office === null) {
        await officesApi.create(payload);
        toast.showSuccess(notificationMessages.createdSuccessfully);
      } else {
        await officesApi.update(office.id, payload);
        toast.showSuccess(notificationMessages.savedSuccessfully);
      }

      onSaved();
    } catch {
      setError(notificationMessages.unexpectedError);
      setIsSaving(false);
    }
  };

  return (
    <>
      <Modal
        title={office === null ? 'Create office' : 'Edit office'}
        size="medium"
        onClose={() => setIsCancelling(true)}
        closeOnOverlayClick={false}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsCancelling(true)}>
              Cancel
            </Button>
            <Button loading={isSaving} disabled={!isValid} onClick={() => void save()}>
              {office === null ? 'Confirm' : 'Save changes'}
            </Button>
          </>
        }
      >
        {error !== null ? <Alert variant="error">{error}</Alert> : null}

        <PhotoUploader value={photoUrl} kind="OfficePhoto" onValueChange={setPhotoUrl} />

        <Field label="City" required error={errors.city}>
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              value={values.city}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={(value) => setValue('city', value)}
              onBlur={() => handleBlur('city')}
            />
          )}
        </Field>

        <Field label="Street" required error={errors.street}>
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              value={values.street}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={(value) => setValue('street', value)}
              onBlur={() => handleBlur('street')}
            />
          )}
        </Field>

        <Field label="House number" required error={errors.houseNumber}>
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              value={values.houseNumber}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={(value) => setValue('houseNumber', value)}
              onBlur={() => handleBlur('houseNumber')}
            />
          )}
        </Field>

        <Field label="Office number">
          {({ id }) => (
            <TextInput
              id={id}
              value={values.officeNumber}
              onValueChange={(value) => setValue('officeNumber', value)}
            />
          )}
        </Field>

        <Field label="Registry phone number" required error={errors.registryPhoneNumber}>
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              value={values.registryPhoneNumber}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={(value) =>
                setValue('registryPhoneNumber', value.startsWith('+') ? value : `+${value}`)
              }
              onBlur={() => handleBlur('registryPhoneNumber')}
            />
          )}
        </Field>

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
      </Modal>

      {isCancelling ? (
        <ConfirmDialog
          message={office === null ? confirmMessages.cancelEntered : confirmMessages.cancelChanges}
          onConfirm={onClose}
          onCancel={() => setIsCancelling(false)}
        />
      ) : null}
    </>
  );
}
