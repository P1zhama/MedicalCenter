import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Alert,
  Button,
  Card,
  ConfirmDialog,
  DateInput,
  Field,
  PageSpinner,
  PhotoUploader,
  StatusBadge,
  Table,
  Tabs,
  TextInput,
  appointmentsApi,
  confirmMessages,
  formatDate,
  formatFullName,
  formatTimeRange,
  notificationMessages,
  profilesApi,
  required,
  todayIso,
  useAsync,
  useAuth,
  useForm,
  useToast,
  validatePastDate,
  validatePhoneNumber,
  validationMessages,
  type AppointmentListItem,
  type TableColumn,
} from '@mmc/shared';
import styles from './PatientPage.module.css';

export function PatientPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const toast = useToast();
  const { session } = useAuth();

  const isDoctor = session?.role === 'Doctor';

  const [activeTab, setActiveTab] = useState('personal');
  const [isEditing, setIsEditing] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [photoUrl, setPhotoUrl] = useState('');

  const { data: patient, isLoading, error, reload } = useAsync(
    () => profilesApi.getPatientById(id),
    [id],
  );

  const { values, errors, isValid, setValue, handleBlur, validateAll, reset } = useForm({
    initialValues: {
      firstName: patient?.firstName ?? '',
      lastName: patient?.lastName ?? '',
      middleName: patient?.middleName ?? '',
      phoneNumber: patient?.phoneNumber ?? '+',
      dateOfBirth: patient?.dateOfBirth ?? '',
    },
    validators: {
      firstName: required(validationMessages.firstNameRequired),
      lastName: required(validationMessages.lastNameRequired),
      phoneNumber: validatePhoneNumber,
      dateOfBirth: (value) => validatePastDate(value, validationMessages.dateRequired),
    },
  });

  const startEditing = () => {
    if (patient === null) {
      return;
    }

    reset({
      firstName: patient.firstName,
      lastName: patient.lastName,
      middleName: patient.middleName,
      phoneNumber: patient.phoneNumber,
      dateOfBirth: patient.dateOfBirth,
    });
    setPhotoUrl(patient.photoUrl);
    setIsEditing(true);
  };

  const save = async () => {
    if (!validateAll() || isSaving) {
      return;
    }

    setIsSaving(true);

    try {
      await profilesApi.updatePatient(id, {
        firstName: values.firstName,
        lastName: values.lastName,
        middleName: values.middleName,
        phoneNumber: values.phoneNumber,
        dateOfBirth: values.dateOfBirth,
        photoUrl,
      });

      toast.showSuccess(notificationMessages.savedSuccessfully);
      setIsEditing(false);
      reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return <PageSpinner label="Loading patient" />;
  }

  if (error !== null || patient === null) {
    return <Alert variant="error">{notificationMessages.unexpectedError}</Alert>;
  }

  const tabs = isDoctor
    ? [
        { id: 'personal', label: 'Personal information' },
        { id: 'appointments', label: 'Appointment results' },
      ]
    : [{ id: 'personal', label: 'Personal information' }];

  return (
    <section>
      <Button variant="ghost" onClick={() => navigate(-1)}>
        &larr; Back
      </Button>

      <h2 className={styles.title}>{formatFullName(patient)}</h2>

      <Tabs items={tabs} activeId={activeTab} onChange={setActiveTab} />

      {activeTab === 'personal' ? (
        <Card>
          {isEditing ? (
            <>
              <PhotoUploader
                value={photoUrl}
                kind="PatientPhoto"
                ownerProfileId={patient.id}
                onValueChange={setPhotoUrl}
              />

              <Field label="First name" required error={errors.firstName}>
                {({ id: fieldId, hasError, describedBy }) => (
                  <TextInput
                    id={fieldId}
                    value={values.firstName}
                    invalid={hasError}
                    aria-describedby={describedBy}
                    onValueChange={(value) => setValue('firstName', value)}
                    onBlur={() => handleBlur('firstName')}
                  />
                )}
              </Field>

              <Field label="Last name" required error={errors.lastName}>
                {({ id: fieldId, hasError, describedBy }) => (
                  <TextInput
                    id={fieldId}
                    value={values.lastName}
                    invalid={hasError}
                    aria-describedby={describedBy}
                    onValueChange={(value) => setValue('lastName', value)}
                    onBlur={() => handleBlur('lastName')}
                  />
                )}
              </Field>

              <Field label="Middle name">
                {({ id: fieldId }) => (
                  <TextInput
                    id={fieldId}
                    value={values.middleName}
                    onValueChange={(value) => setValue('middleName', value)}
                  />
                )}
              </Field>

              <Field label="Phone number" required error={errors.phoneNumber}>
                {({ id: fieldId, hasError, describedBy }) => (
                  <TextInput
                    id={fieldId}
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
                {({ id: fieldId, hasError, describedBy }) => (
                  <DateInput
                    id={fieldId}
                    value={values.dateOfBirth}
                    max={todayIso()}
                    invalid={hasError}
                    aria-describedby={describedBy}
                    onValueChange={(value) => setValue('dateOfBirth', value)}
                    onBlur={() => handleBlur('dateOfBirth')}
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

              {!isDoctor ? (
                <Button variant="outline" onClick={startEditing}>
                  Edit
                </Button>
              ) : null}
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
      ) : (
        <PatientAppointments patientId={id} />
      )}
    </section>
  );
}

function PatientAppointments({ patientId }: { patientId: string }) {
  const { data, isLoading } = useAsync(
    () => appointmentsApi.getPatientAppointments(patientId),
    [patientId],
  );

  const rows = [...(data ?? [])].sort((left, right) => {
    if (left.date !== right.date) {
      return right.date.localeCompare(left.date);
    }

    return left.startTime.localeCompare(right.startTime);
  });

  const columns: TableColumn<AppointmentListItem>[] = [
    { key: 'date', header: 'Date', render: (row) => formatDate(row.date) },
    { key: 'time', header: 'Time', render: (row) => formatTimeRange(row.startTime, row.endTime) },
    {
      key: 'doctor',
      header: 'Doctor',
      render: (row) =>
        formatFullName({
          firstName: row.doctorFirstName,
          lastName: row.doctorLastName,
          middleName: row.doctorMiddleName,
        }),
    },
    { key: 'service', header: 'Service', render: (row) => row.serviceName },
    {
      key: 'status',
      header: 'Status',
      render: (row) =>
        row.status === 'Approved' ? (
          <StatusBadge label="Approved" tone="success" />
        ) : (
          <StatusBadge label="Not approved" />
        ),
    },
  ];

  return (
    <Table
      columns={columns}
      rows={rows}
      rowKey={(row) => row.id}
      isLoading={isLoading}
      emptyMessage="This patient has no appointments yet"
    />
  );
}
