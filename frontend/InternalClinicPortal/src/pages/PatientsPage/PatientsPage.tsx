import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Button,
  ConfirmDialog,
  Field,
  Table,
  TextInput,
  confirmMessages,
  formatFullName,
  notificationMessages,
  profilesApi,
  useAsync,
  useDebounce,
  useToast,
  type PatientListItem,
  type TableColumn,
} from '@mmc/shared';
import { CreatePatientModal } from '../../features/patients/CreatePatientModal';
import styles from './PatientsPage.module.css';

export function PatientsPage() {
  const toast = useToast();
  const navigate = useNavigate();

  const [search, setSearch] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [deleting, setDeleting] = useState<PatientListItem | null>(null);

  const debouncedSearch = useDebounce(search);

  const { data, isLoading, reload } = useAsync(
    () => profilesApi.getPatients(debouncedSearch || undefined),
    [debouncedSearch],
  );

  const remove = async () => {
    if (deleting === null) {
      return;
    }

    try {
      await profilesApi.deletePatient(deleting.id);
      toast.showSuccess(notificationMessages.profileDeleted);
      reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    } finally {
      setDeleting(null);
    }
  };

  const columns: TableColumn<PatientListItem>[] = [
    { key: 'name', header: 'Full name', render: (row) => formatFullName(row) },
    { key: 'phone', header: 'Phone number', render: (row) => row.phoneNumber },
    {
      key: 'actions',
      header: '',
      render: (row) => (
        <div className={styles.actions}>
          <Button variant="ghost" onClick={() => navigate(`/patients/${row.id}`)}>
            Open
          </Button>
          <Button variant="ghost" onClick={() => setDeleting(row)}>
            Delete
          </Button>
        </div>
      ),
    },
  ];

  return (
    <section>
      <div className={styles.header}>
        <h2 className={styles.title}>Patients</h2>
        <Button onClick={() => setIsCreating(true)}>Create patient</Button>
      </div>

      <div className={styles.filters}>
        <Field label="Search by name">
          {({ id }) => (
            <TextInput
              id={id}
              value={search}
              placeholder="Enter the patient's name"
              onValueChange={setSearch}
            />
          )}
        </Field>
      </div>

      <Table
        columns={columns}
        rows={data ?? []}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="No matches found"
      />

      {isCreating ? (
        <CreatePatientModal
          onCreated={() => {
            setIsCreating(false);
            toast.showSuccess(notificationMessages.createdSuccessfully);
            reload();
          }}
          onClose={() => setIsCreating(false)}
        />
      ) : null}

      {deleting !== null ? (
        <ConfirmDialog
          message={confirmMessages.deleteProfile}
          destructive
          onConfirm={() => void remove()}
          onCancel={() => setDeleting(null)}
        />
      ) : null}
    </section>
  );
}
