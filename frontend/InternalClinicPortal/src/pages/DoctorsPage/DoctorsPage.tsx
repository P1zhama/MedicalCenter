import { useState } from 'react';
import {
  Button,
  Field,
  Select,
  Table,
  TextInput,
  formatDate,
  formatFullName,
  notificationMessages,
  profilesApi,
  useAsync,
  useDebounce,
  useDictionaries,
  useToast,
  type Doctor,
  type DoctorListItem,
  type DoctorStatus,
  type TableColumn,
} from '@mmc/shared';
import { DoctorModal } from '../../features/doctors/DoctorModal';
import { DOCTOR_STATUSES } from '../../features/doctors/doctorStatuses';
import styles from './DoctorsPage.module.css';

export function DoctorsPage() {
  const toast = useToast();
  const { offices, specializations, officeAddress, specializationName } = useDictionaries();

  const [search, setSearch] = useState('');
  const [specializationId, setSpecializationId] = useState('');
  const [officeId, setOfficeId] = useState('');
  const [editing, setEditing] = useState<Doctor | null>(null);
  const [isCreating, setIsCreating] = useState(false);

  const debouncedSearch = useDebounce(search);

  const { data, isLoading, reload } = useAsync(
    () =>
      profilesApi.getDoctors({
        search: debouncedSearch || undefined,
        specializationId: specializationId || undefined,
        officeId: officeId || undefined,
      }),
    [debouncedSearch, specializationId, officeId],
  );

  const changeStatus = async (row: DoctorListItem, status: DoctorStatus) => {
    try {
      await profilesApi.changeDoctorStatus(row.id, status);
      toast.showSuccess(notificationMessages.savedSuccessfully);
      reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const open = async (row: DoctorListItem) => {
    try {
      setEditing(await profilesApi.getDoctorById(row.id));
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const columns: TableColumn<DoctorListItem>[] = [
    { key: 'name', header: 'Full name', render: (row) => formatFullName(row) },
    {
      key: 'specialization',
      header: 'Specialization',
      render: (row) => specializationName(row.specializationId),
    },
    { key: 'birth', header: 'Date of birth', render: (row) => formatDate(row.dateOfBirth) },
    { key: 'office', header: 'Office', render: (row) => officeAddress(row.officeId) },
    {
      key: 'status',
      header: 'Status',
      width: '200px',
      render: (row) => (
        <Select
          value={row.status}
          options={DOCTOR_STATUSES}
          onValueChange={(value) => void changeStatus(row, value as DoctorStatus)}
        />
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (row) => (
        <Button variant="ghost" onClick={() => void open(row)}>
          Open
        </Button>
      ),
    },
  ];

  return (
    <section>
      <div className={styles.header}>
        <h2 className={styles.title}>Doctors</h2>
        <Button onClick={() => setIsCreating(true)}>Create doctor</Button>
      </div>

      <div className={styles.filters}>
        <Field label="Search by name">
          {({ id }) => (
            <TextInput
              id={id}
              value={search}
              placeholder="Enter the doctor's name"
              onValueChange={setSearch}
            />
          )}
        </Field>

        <Field label="Specialization">
          {({ id }) => (
            <Select
              id={id}
              value={specializationId}
              placeholder="All specializations"
              options={specializations.map((item) => ({ value: item.id, label: item.name }))}
              onValueChange={setSpecializationId}
            />
          )}
        </Field>

        <Field label="Office">
          {({ id }) => (
            <Select
              id={id}
              value={officeId}
              placeholder="All offices"
              options={offices.map((item) => ({ value: item.id, label: item.address }))}
              onValueChange={setOfficeId}
            />
          )}
        </Field>
      </div>

      <Table
        columns={columns}
        rows={data ?? []}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="There are no doctors matching this filtration"
      />

      {isCreating || editing !== null ? (
        <DoctorModal
          doctor={editing}
          onSaved={() => {
            setIsCreating(false);
            setEditing(null);
            reload();
          }}
          onClose={() => {
            setIsCreating(false);
            setEditing(null);
          }}
        />
      ) : null}
    </section>
  );
}
