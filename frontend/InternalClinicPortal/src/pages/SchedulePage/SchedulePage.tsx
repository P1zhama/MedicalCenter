import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Button,
  DateInput,
  Field,
  StatusBadge,
  Table,
  appointmentsApi,
  formatFullName,
  formatTimeRange,
  todayIso,
  useAsync,
  type AppointmentListItem,
  type TableColumn,
} from '@mmc/shared';
import { ResultModal } from '../../features/results/ResultModal';
import styles from './SchedulePage.module.css';

export function SchedulePage() {
  const [date, setDate] = useState(todayIso());
  const [editing, setEditing] = useState<AppointmentListItem | null>(null);

  const { data, isLoading, reload } = useAsync(
    () => appointmentsApi.getDoctorSchedule(date),
    [date],
  );

  const rows = [...(data ?? [])].sort((left, right) => left.startTime.localeCompare(right.startTime));

  const columns: TableColumn<AppointmentListItem>[] = [
    { key: 'time', header: 'Time', render: (row) => formatTimeRange(row.startTime, row.endTime) },
    {
      key: 'patient',
      header: 'Patient',
      render: (row) => {
        const name = formatFullName({
          firstName: row.patientFirstName,
          lastName: row.patientLastName,
          middleName: row.patientMiddleName,
        });

        return row.status === 'Approved' ? (
          <Link to={`/patients/${row.patientId}`}>{name}</Link>
        ) : (
          <span className={styles.inactive}>{name}</span>
        );
      },
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
      header: 'Medical results',
      render: (row) =>
        row.status === 'Approved' ? (
          <Button variant="ghost" onClick={() => setEditing(row)}>
            Add / view result
          </Button>
        ) : (
          <span className={styles.inactive}>Available after approval</span>
        ),
    },
  ];

  return (
    <section>
      <h2 className={styles.title}>My schedule</h2>

      <div className={styles.filters}>
        <Field label="Date">
          {({ id }) => <DateInput id={id} value={date} onValueChange={setDate} />}
        </Field>
      </div>

      <Table
        columns={columns}
        rows={rows}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="There are no appointments for this day"
      />

      {editing !== null ? (
        <ResultModal
          appointment={editing}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null);
            reload();
          }}
        />
      ) : null}
    </section>
  );
}
