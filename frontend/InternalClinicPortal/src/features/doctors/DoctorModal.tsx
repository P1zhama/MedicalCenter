import { useState } from 'react';
import {
  Alert,
  Button,
  Combobox,
  ConfirmDialog,
  DateInput,
  Field,
  Modal,
  PhotoUploader,
  Select,
  TextInput,
  confirmMessages,
  notificationMessages,
  profilesApi,
  required,
  todayIso,
  useDictionaries,
  useForm,
  useToast,
  validateEmail,
  validatePastDate,
  validationMessages,
  type Doctor,
  type DoctorStatus,
} from '@mmc/shared';
import { DOCTOR_STATUSES } from './doctorStatuses';

export interface DoctorModalProps {
  doctor: Doctor | null;
  onSaved: () => void;
  onClose: () => void;
}

export function DoctorModal({ doctor, onSaved, onClose }: DoctorModalProps) {
  const toast = useToast();
  const { offices, specializations } = useDictionaries();

  const [photoUrl, setPhotoUrl] = useState(doctor?.photoUrl ?? '');
  const [status, setStatus] = useState<DoctorStatus>(doctor?.status ?? 'At work');
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  const isCreate = doctor === null;

  const { values, errors, isValid, setValue, setError: setFieldError, handleBlur, validateAll } =
    useForm({
      initialValues: {
        firstName: doctor?.firstName ?? '',
        lastName: doctor?.lastName ?? '',
        middleName: doctor?.middleName ?? '',
        dateOfBirth: doctor?.dateOfBirth ?? '',
        email: '',
        specializationId: doctor?.specializationId ?? '',
        officeId: doctor?.officeId ?? '',
        careerStartYear: doctor !== null ? `${doctor.careerStartYear}` : '',
      },
      validators: {
        firstName: required(validationMessages.firstNameRequired),
        lastName: required(validationMessages.lastNameRequired),
        dateOfBirth: (value) => validatePastDate(value, validationMessages.dateRequired),
        email: (value) => (isCreate ? validateEmail(value) : null),
        specializationId: required(validationMessages.specializationRequired),
        officeId: required(validationMessages.officeRequired),
        careerStartYear: (value) => {
          const year = Number(value);
          const current = new Date().getFullYear();

          return value !== '' && Number.isInteger(year) && year >= 1950 && year <= current
            ? null
            : validationMessages.yearRequired;
        },
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
        await profilesApi.createDoctor({
          firstName: values.firstName,
          lastName: values.lastName,
          middleName: values.middleName,
          dateOfBirth: values.dateOfBirth,
          email: values.email,
          specializationId: values.specializationId,
          officeId: values.officeId,
          careerStartYear: Number(values.careerStartYear),
          status,
          photoUrl,
        });

        toast.showSuccess(notificationMessages.createdSuccessfully);
      } else {
        await profilesApi.updateDoctor(doctor.id, {
          firstName: values.firstName,
          lastName: values.lastName,
          middleName: values.middleName,
          dateOfBirth: values.dateOfBirth,
          specializationId: values.specializationId,
          officeId: values.officeId,
          careerStartYear: Number(values.careerStartYear),
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
        title={isCreate ? 'Create doctor' : 'Edit doctor'}
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

        <PhotoUploader value={photoUrl} kind="DoctorPhoto" onValueChange={setPhotoUrl} />

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

        <Field label="Specialization" required error={errors.specializationId}>
          {({ id, hasError, describedBy }) => (
            <Combobox
              id={id}
              value={values.specializationId}
              options={specializations.map((item) => ({ value: item.id, label: item.name }))}
              placeholder="Choose the specialization"
              invalid={hasError}
              describedBy={describedBy}
              onValueChange={(value) => setValue('specializationId', value)}
              onInvalidInput={() =>
                setFieldError('specializationId', validationMessages.specializationInvalid)
              }
              onBlur={() => handleBlur('specializationId')}
            />
          )}
        </Field>

        <Field label="Office" required error={errors.officeId}>
          {({ id, hasError }) => (
            <Select
              id={id}
              value={values.officeId}
              options={offices.map((item) => ({ value: item.id, label: item.address }))}
              placeholder="Choose the office"
              invalid={hasError}
              onValueChange={(value) => setValue('officeId', value)}
              onBlur={() => handleBlur('officeId')}
            />
          )}
        </Field>

        <Field label="Career start year" required error={errors.careerStartYear}>
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              type="number"
              min="1950"
              max={`${new Date().getFullYear()}`}
              value={values.careerStartYear}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={(value) => setValue('careerStartYear', value)}
              onBlur={() => handleBlur('careerStartYear')}
            />
          )}
        </Field>

        <Field label="Status" required>
          {({ id }) => (
            <Select
              id={id}
              value={status}
              options={DOCTOR_STATUSES}
              onValueChange={(value) => setStatus(value as DoctorStatus)}
            />
          )}
        </Field>
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
