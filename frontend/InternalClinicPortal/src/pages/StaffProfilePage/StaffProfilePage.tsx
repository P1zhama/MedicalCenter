import { useState } from 'react';
import {
  Alert,
  Button,
  Card,
  ConfirmDialog,
  DateInput,
  Field,
  PageSpinner,
  PhotoUploader,
  Select,
  TextInput,
  confirmMessages,
  formatDate,
  notificationMessages,
  profilesApi,
  required,
  todayIso,
  useAsync,
  useAuth,
  useDictionaries,
  useForm,
  useToast,
  validatePastDate,
  validationMessages,
  type Doctor,
  type Receptionist,
} from '@mmc/shared';
import styles from './StaffProfilePage.module.css';

export function StaffProfilePage() {
  const { session } = useAuth();

  return session?.role === 'Doctor' ? <DoctorProfileLoader /> : <ReceptionistProfileLoader />;
}

function DoctorProfileLoader() {
  const { data, isLoading, error, reload } = useAsync(() => profilesApi.getMyDoctorProfile(), []);

  if (isLoading) {
    return <PageSpinner label="Loading profile" />;
  }

  if (error !== null || data === null) {
    return <Alert variant="error">{notificationMessages.unexpectedError}</Alert>;
  }

  return <DoctorProfile profile={data} onSaved={reload} />;
}

function ReceptionistProfileLoader() {
  const { data, isLoading, error, reload } = useAsync(
    () => profilesApi.getMyReceptionistProfile(),
    [],
  );

  if (isLoading) {
    return <PageSpinner label="Loading profile" />;
  }

  if (error !== null || data === null) {
    return <Alert variant="error">{notificationMessages.unexpectedError}</Alert>;
  }

  return <ReceptionistProfile profile={data} onSaved={reload} />;
}

function DoctorProfile({
  profile,
  onSaved,
}: {
  profile: Doctor;
  onSaved: () => void;
}) {
  const toast = useToast();
  const { offices, specializations, officeAddress, specializationName } = useDictionaries();

  const [isEditing, setIsEditing] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [photoUrl, setPhotoUrl] = useState(profile.photoUrl);

  const { values, errors, isValid, setValue, handleBlur, validateAll } = useForm({
    initialValues: {
      firstName: profile.firstName,
      lastName: profile.lastName,
      middleName: profile.middleName,
      dateOfBirth: profile.dateOfBirth,
      specializationId: profile.specializationId,
      officeId: profile.officeId,
      careerStartYear: `${profile.careerStartYear}`,
    },
    validators: {
      firstName: required(validationMessages.firstNameRequired),
      lastName: required(validationMessages.lastNameRequired),
      dateOfBirth: (value) => validatePastDate(value, validationMessages.dateRequired),
      specializationId: required(validationMessages.specializationRequired),
      officeId: required(validationMessages.officeRequired),
      careerStartYear: (value) => (value !== '' ? null : validationMessages.yearRequired),
    },
  });

  const save = async () => {
    if (!validateAll() || isSaving) {
      return;
    }

    setIsSaving(true);

    try {
      await profilesApi.updateMyDoctorProfile({
        firstName: values.firstName,
        lastName: values.lastName,
        middleName: values.middleName,
        dateOfBirth: values.dateOfBirth,
        specializationId: values.specializationId,
        officeId: values.officeId,
        careerStartYear: Number(values.careerStartYear),
        photoUrl,
      });

      toast.showSuccess(notificationMessages.savedSuccessfully);
      setIsEditing(false);
      onSaved();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <section>
      <h2 className={styles.title}>My profile</h2>

      <Card>
        {isEditing ? (
          <>
            <PhotoUploader value={photoUrl} kind="DoctorPhoto" onValueChange={setPhotoUrl} />

            <Field label="First name" required error={errors.firstName}>
              {({ id, hasError }) => (
                <TextInput
                  id={id}
                  value={values.firstName}
                  invalid={hasError}
                  onValueChange={(value) => setValue('firstName', value)}
                  onBlur={() => handleBlur('firstName')}
                />
              )}
            </Field>

            <Field label="Last name" required error={errors.lastName}>
              {({ id, hasError }) => (
                <TextInput
                  id={id}
                  value={values.lastName}
                  invalid={hasError}
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
              {({ id, hasError }) => (
                <DateInput
                  id={id}
                  value={values.dateOfBirth}
                  max={todayIso()}
                  invalid={hasError}
                  onValueChange={(value) => setValue('dateOfBirth', value)}
                  onBlur={() => handleBlur('dateOfBirth')}
                />
              )}
            </Field>

            <Field label="Specialization" required error={errors.specializationId}>
              {({ id, hasError }) => (
                <Select
                  id={id}
                  value={values.specializationId}
                  options={specializations.map((item) => ({ value: item.id, label: item.name }))}
                  invalid={hasError}
                  onValueChange={(value) => setValue('specializationId', value)}
                />
              )}
            </Field>

            <Field label="Office" required error={errors.officeId}>
              {({ id, hasError }) => (
                <Select
                  id={id}
                  value={values.officeId}
                  options={offices.map((item) => ({ value: item.id, label: item.address }))}
                  invalid={hasError}
                  onValueChange={(value) => setValue('officeId', value)}
                />
              )}
            </Field>

            <Field label="Career start year" required error={errors.careerStartYear}>
              {({ id, hasError }) => (
                <TextInput
                  id={id}
                  type="number"
                  value={values.careerStartYear}
                  invalid={hasError}
                  onValueChange={(value) => setValue('careerStartYear', value)}
                  onBlur={() => handleBlur('careerStartYear')}
                />
              )}
            </Field>

            <div className={styles.footer}>
              <Button variant="outline" onClick={() => setIsCancelling(true)}>
                Cancel
              </Button>
              <Button loading={isSaving} disabled={!isValid} onClick={() => void save()}>
                Save changes
              </Button>
            </div>
          </>
        ) : (
          <>
            <div className={styles.view}>
              <div className={styles.avatar}>
                {profile.photoUrl.length > 0 ? (
                  <img src={profile.photoUrl} alt="" className={styles.avatarImage} />
                ) : (
                  <span className={styles.avatarPlaceholder} aria-hidden="true" />
                )}
              </div>

              <dl className={styles.details}>
                <dt>First name</dt>
                <dd>{profile.firstName}</dd>
                <dt>Last name</dt>
                <dd>{profile.lastName}</dd>
                <dt>Middle name</dt>
                <dd>{profile.middleName.length > 0 ? profile.middleName : '—'}</dd>
                <dt>Date of birth</dt>
                <dd>{formatDate(profile.dateOfBirth)}</dd>
                <dt>Specialization</dt>
                <dd>{specializationName(profile.specializationId)}</dd>
                <dt>Office</dt>
                <dd>{officeAddress(profile.officeId)}</dd>
                <dt>Career start year</dt>
                <dd>{profile.careerStartYear}</dd>
              </dl>
            </div>

            <Button variant="outline" onClick={() => setIsEditing(true)}>
              Edit
            </Button>
          </>
        )}

        {isCancelling ? (
          <ConfirmDialog
            message={confirmMessages.cancelChanges}
            onConfirm={() => {
              setIsEditing(false);
              setIsCancelling(false);
            }}
            onCancel={() => setIsCancelling(false)}
          />
        ) : null}
      </Card>
    </section>
  );
}

function ReceptionistProfile({
  profile,
  onSaved,
}: {
  profile: Receptionist;
  onSaved: () => void;
}) {
  const toast = useToast();
  const { offices, officeAddress } = useDictionaries();

  const [isEditing, setIsEditing] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [photoUrl, setPhotoUrl] = useState(profile.photoUrl);

  const { values, errors, isValid, setValue, handleBlur, validateAll } = useForm({
    initialValues: {
      firstName: profile.firstName,
      lastName: profile.lastName,
      middleName: profile.middleName,
      officeId: profile.officeId,
    },
    validators: {
      firstName: required(validationMessages.firstNameRequired),
      lastName: required(validationMessages.lastNameRequired),
      officeId: required(validationMessages.officeRequired),
    },
  });

  const save = async () => {
    if (!validateAll() || isSaving) {
      return;
    }

    setIsSaving(true);

    try {
      await profilesApi.updateMyReceptionistProfile({
        firstName: values.firstName,
        lastName: values.lastName,
        middleName: values.middleName,
        officeId: values.officeId,
        photoUrl,
      });

      toast.showSuccess(notificationMessages.savedSuccessfully);
      setIsEditing(false);
      onSaved();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <section>
      <h2 className={styles.title}>My profile</h2>

      <Card>
        {isEditing ? (
          <>
            <PhotoUploader value={photoUrl} kind="ReceptionistPhoto" onValueChange={setPhotoUrl} />

            <Field label="First name" required error={errors.firstName}>
              {({ id, hasError }) => (
                <TextInput
                  id={id}
                  value={values.firstName}
                  invalid={hasError}
                  onValueChange={(value) => setValue('firstName', value)}
                  onBlur={() => handleBlur('firstName')}
                />
              )}
            </Field>

            <Field label="Last name" required error={errors.lastName}>
              {({ id, hasError }) => (
                <TextInput
                  id={id}
                  value={values.lastName}
                  invalid={hasError}
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

            <Field label="Office" required error={errors.officeId}>
              {({ id, hasError }) => (
                <Select
                  id={id}
                  value={values.officeId}
                  options={offices.map((item) => ({ value: item.id, label: item.address }))}
                  invalid={hasError}
                  onValueChange={(value) => setValue('officeId', value)}
                />
              )}
            </Field>

            <div className={styles.footer}>
              <Button variant="outline" onClick={() => setIsCancelling(true)}>
                Cancel
              </Button>
              <Button loading={isSaving} disabled={!isValid} onClick={() => void save()}>
                Save changes
              </Button>
            </div>
          </>
        ) : (
          <>
            <div className={styles.view}>
              <div className={styles.avatar}>
                {profile.photoUrl.length > 0 ? (
                  <img src={profile.photoUrl} alt="" className={styles.avatarImage} />
                ) : (
                  <span className={styles.avatarPlaceholder} aria-hidden="true" />
                )}
              </div>

              <dl className={styles.details}>
                <dt>First name</dt>
                <dd>{profile.firstName}</dd>
                <dt>Last name</dt>
                <dd>{profile.lastName}</dd>
                <dt>Middle name</dt>
                <dd>{profile.middleName.length > 0 ? profile.middleName : '—'}</dd>
                <dt>Office</dt>
                <dd>{officeAddress(profile.officeId)}</dd>
              </dl>
            </div>

            <Button variant="outline" onClick={() => setIsEditing(true)}>
              Edit
            </Button>
          </>
        )}

        {isCancelling ? (
          <ConfirmDialog
            message={confirmMessages.cancelChanges}
            onConfirm={() => {
              setIsEditing(false);
              setIsCancelling(false);
            }}
            onCancel={() => setIsCancelling(false)}
          />
        ) : null}
      </Card>
    </section>
  );
}
