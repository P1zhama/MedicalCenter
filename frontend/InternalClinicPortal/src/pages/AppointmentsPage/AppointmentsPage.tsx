import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  AppointmentModal,
  Button,
  Combobox,
  ConfirmDialog,
  DateInput,
  Field,
  MAX_PAGE_SIZE,
  Select,
  StatusBadge,
  Table,
  appointmentsApi,
  confirmMessages,
  formatFullName,
  formatTimeRange,
  notificationMessages,
  profilesApi,
  todayIso,
  useAsync,
  useDictionaries,
  useToast,
  type AppointmentFormValues,
  type AppointmentListItem,
  type AppointmentStatus,
  type TableColumn,
} from '@mmc/shared';
import { CreatePatientModal } from '../../features/patients/CreatePatientModal';
import styles from './AppointmentsPage.module.css';

interface Filters {
  date: string;
  doctorId: string;
  serviceId: string;
  officeId: string;
  status: AppointmentStatus | '';
}

const STATUS_OPTIONS = [
  { value: 'Approved', label: 'Approved' },
  { value: 'NotApproved', label: 'Not approved' },
];

export function AppointmentsPage() {
  const toast = useToast();
  const { offices, catalog } = useDictionaries();

  const initialFilters: Filters = {
    date: todayIso(),
    doctorId: '',
    serviceId: '',
    officeId: '',
    status: '',
  };

  const [draft, setDraft] = useState<Filters>(initialFilters);
  const [applied, setApplied] = useState<Filters>(initialFilters);

  const [isCreating, setIsCreating] = useState(false);
  const [creatingPatient, setCreatingPatient] = useState(false);
  const [selectedPatient, setSelectedPatient] = useState<{ id: string; name: string } | null>(null);
  const [rescheduling, setRescheduling] = useState<AppointmentListItem | null>(null);
  const [cancelling, setCancelling] = useState<AppointmentListItem | null>(null);

  const { data: doctors } = useAsync(
    () => profilesApi.getDoctors({ pageSize: MAX_PAGE_SIZE }),
    [],
  );
  const { data: patients } = useAsync(
    () => profilesApi.getPatients({ pageSize: MAX_PAGE_SIZE }),
    [],
  );

  const { data, isLoading, reload } = useAsync(
    () =>
      appointmentsApi.getAppointments({
        date: applied.date,
        doctorId: applied.doctorId || undefined,
        serviceId: applied.serviceId || undefined,
        officeId: applied.officeId || undefined,
        status: applied.status,
      }),
    [applied],
  );

  const serviceOptions = useMemo(
    () =>
      (catalog?.categories ?? []).flatMap((category) =>
        category.specializations.flatMap((specialization) =>
          specialization.services.map((service) => ({ value: service.id, label: service.name })),
        ),
      ),
    [catalog],
  );

  const doctorOptions = (doctors?.items ?? []).map((doctor) => ({
    value: doctor.id,
    label: formatFullName(doctor),
  }));

  const patientOptions = (patients?.items ?? []).map((patient) => ({
    value: patient.id,
    label: formatFullName(patient),
  }));

  const rows = useMemo(() => {
    const list = data ?? [];

    return [...list].sort((left, right) => {
      if (left.startTime !== right.startTime) {
        return left.startTime.localeCompare(right.startTime);
      }

      if (left.doctorLastName !== right.doctorLastName) {
        return left.doctorLastName.localeCompare(right.doctorLastName);
      }

      if (left.doctorFirstName !== right.doctorFirstName) {
        return left.doctorFirstName.localeCompare(right.doctorFirstName);
      }

      return left.serviceName.localeCompare(right.serviceName);
    });
  }, [data]);

  const approve = async (row: AppointmentListItem) => {
    try {
      await appointmentsApi.approve(row.id);
      toast.showSuccess(notificationMessages.appointmentApproved);
      reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const cancel = async () => {
    if (cancelling === null) {
      return;
    }

    try {
      await appointmentsApi.cancel(cancelling.id);
      toast.showSuccess(notificationMessages.appointmentCancelled);
      reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    } finally {
      setCancelling(null);
    }
  };

  const createAppointment = async (values: AppointmentFormValues) => {
    if (selectedPatient === null) {
      return;
    }

    await appointmentsApi.createAppointment({
      patientId: selectedPatient.id,
      serviceId: values.serviceId,
      doctorId: values.doctorId,
      officeId: values.officeId,
      date: values.date,
      startTime: values.startTime,
    });

    setIsCreating(false);
    setSelectedPatient(null);
    toast.showSuccess(notificationMessages.appointmentCreated);
    reload();
  };

  const reschedule = async (values: AppointmentFormValues) => {
    if (rescheduling === null) {
      return;
    }

    await appointmentsApi.rescheduleAppointment(rescheduling.id, {
      doctorId: values.doctorId,
      date: values.date,
      startTime: values.startTime,
    });

    setRescheduling(null);
    toast.showSuccess(notificationMessages.appointmentRescheduled);
    reload();
  };

  const columns: TableColumn<AppointmentListItem>[] = [
    {
      key: 'time',
      header: 'Time',
      render: (row) => formatTimeRange(row.startTime, row.endTime),
    },
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
    {
      key: 'patient',
      header: 'Patient',
      render: (row) => (
        <Link to={`/patients/${row.patientId}`}>
          {formatFullName({
            firstName: row.patientFirstName,
            lastName: row.patientLastName,
            middleName: row.patientMiddleName,
          })}
        </Link>
      ),
    },
    { key: 'phone', header: 'Phone', render: (row) => row.patientPhoneNumber },
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
    {
      key: 'actions',
      header: 'Actions',
      render: (row) => (
        <div className={styles.actions}>
          <Button
            variant="ghost"
            disabled={row.status === 'Approved'}
            onClick={() => void approve(row)}
          >
            Approve
          </Button>

          <Button
            variant="ghost"
            disabled={row.status === 'Approved'}
            onClick={() => setRescheduling(row)}
          >
            Reschedule
          </Button>

          <Button variant="ghost" onClick={() => setCancelling(row)}>
            Cancel
          </Button>
        </div>
      ),
    },
  ];

  return (
    <section>
      <div className={styles.header}>
        <h2 className={styles.title}>Appointments</h2>

        <Button onClick={() => setIsCreating(true)}>Create an appointment</Button>
      </div>

      <div className={styles.filters}>
        <Field label="Date">
          {({ id }) => (
            <DateInput
              id={id}
              value={draft.date}
              onValueChange={(value) => setDraft({ ...draft, date: value })}
            />
          )}
        </Field>

        <Field label="Doctor">
          {({ id }) => (
            <Combobox
              id={id}
              value={draft.doctorId}
              options={doctorOptions}
              placeholder="All doctors"
              onValueChange={(value) => setDraft({ ...draft, doctorId: value })}
            />
          )}
        </Field>

        <Field label="Service">
          {({ id }) => (
            <Combobox
              id={id}
              value={draft.serviceId}
              options={serviceOptions}
              placeholder="All services"
              onValueChange={(value) => setDraft({ ...draft, serviceId: value })}
            />
          )}
        </Field>

        <Field label="Office">
          {({ id }) => (
            <Select
              id={id}
              value={draft.officeId}
              placeholder="All offices"
              options={offices.map((office) => ({ value: office.id, label: office.address }))}
              onValueChange={(value) => setDraft({ ...draft, officeId: value })}
            />
          )}
        </Field>

        <Field label="Status">
          {({ id }) => (
            <Select
              id={id}
              value={draft.status}
              placeholder="All"
              options={STATUS_OPTIONS}
              onValueChange={(value) =>
                setDraft({ ...draft, status: value as AppointmentStatus | '' })
              }
            />
          )}
        </Field>

        <div className={styles.generate}>
          <Button onClick={() => setApplied(draft)}>Generate</Button>
        </div>
      </div>

      <Table
        columns={columns}
        rows={rows}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="There are no appointments matching this filtration"
        rowClassName={(row) => (row.status === 'Approved' ? styles.approvedRow : undefined)}
      />

      {isCreating ? (
        <AppointmentModal
          title="Create an appointment"
          isPatientSelected={selectedPatient !== null}
          patientField={
            <div className={styles.patientField}>
              <Field label="Patient" required>
                {({ id }) => (
                  <Combobox
                    id={id}
                    value={selectedPatient?.id ?? ''}
                    options={patientOptions}
                    placeholder="Choose the patient"
                    onValueChange={(value) => {
                      const option = patientOptions.find((item) => item.value === value);

                      setSelectedPatient(
                        option !== undefined ? { id: option.value, name: option.label } : null,
                      );
                    }}
                  />
                )}
              </Field>

              <Button variant="outline" onClick={() => setCreatingPatient(true)}>
                Create patient
              </Button>
            </div>
          }
          onSubmit={createAppointment}
          onClose={() => {
            setIsCreating(false);
            setSelectedPatient(null);
          }}
        />
      ) : null}

      {creatingPatient ? (
        <CreatePatientModal
          onCreated={(patientId, fullName) => {
            setSelectedPatient({ id: patientId, name: fullName });
            setCreatingPatient(false);
            toast.showSuccess(notificationMessages.createdSuccessfully);
          }}
          onClose={() => setCreatingPatient(false)}
        />
      ) : null}

      {rescheduling !== null ? (
        <AppointmentModal
          title="Reschedule appointment"
          mode="reschedule"
          initialValues={{
            doctorId: rescheduling.doctorId,
            serviceId: rescheduling.serviceId,
            date: rescheduling.date,
            startTime: rescheduling.startTime,
          }}
          onSubmit={reschedule}
          onClose={() => setRescheduling(null)}
        />
      ) : null}

      {cancelling !== null ? (
        <ConfirmDialog
          message={confirmMessages.cancelAppointment}
          destructive
          onConfirm={() => void cancel()}
          onCancel={() => setCancelling(null)}
        />
      ) : null}
    </section>
  );
}
