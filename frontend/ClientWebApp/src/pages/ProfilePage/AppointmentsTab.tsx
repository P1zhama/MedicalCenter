import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  AppointmentModal,
  Button,
  Pagination,
  StatusBadge,
  Table,
  appointmentsApi,
  formatDate,
  formatFullName,
  formatTimeRange,
  notificationMessages,
  useAsync,
  useToast,
  type AppointmentFormValues,
  type AppointmentListItem,
  type TableColumn,
} from '@mmc/shared';

export function AppointmentsTab() {
  const toast = useToast();
  const [rescheduling, setRescheduling] = useState<AppointmentListItem | null>(null);
  const [page, setPage] = useState(1);

  const { data, isLoading, reload } = useAsync(
    () => appointmentsApi.getMyAppointments({ page }),
    [page],
  );

  const appointments = useMemo(() => {
    const rows = data?.items ?? [];

    return [...rows].sort((left, right) => {
      if (left.date !== right.date) {
        return right.date.localeCompare(left.date);
      }

      return left.startTime.localeCompare(right.startTime);
    });
  }, [data]);

  const columns: TableColumn<AppointmentListItem>[] = [
    { key: 'date', header: 'Date', render: (row) => formatDate(row.date) },
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
      key: 'result',
      header: 'Result',
      render: (row) => <Link to={`/appointments/${row.id}/result`}>View result</Link>,
    },
    {
      key: 'actions',
      header: '',
      render: (row) =>
        row.status === 'Approved' ? null : (
          <Button variant="ghost" onClick={() => setRescheduling(row)}>
            Reschedule
          </Button>
        ),
    },
  ];

  const handleReschedule = async (values: AppointmentFormValues) => {
    if (rescheduling === null) {
      return;
    }

    await appointmentsApi.rescheduleMyAppointment(rescheduling.id, {
      doctorId: values.doctorId,
      date: values.date,
      startTime: values.startTime,
    });

    setRescheduling(null);
    toast.showSuccess(notificationMessages.appointmentRescheduled);
    reload();
  };

  return (
    <>
      <Table
        columns={columns}
        rows={appointments}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="You have no appointments yet"
      />

      {data !== null ? (
        <Pagination
          page={data.page}
          pageSize={data.pageSize}
          totalCount={data.totalCount}
          onPageChange={setPage}
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
          onSubmit={handleReschedule}
          onClose={() => setRescheduling(null)}
        />
      ) : null}
    </>
  );
}
