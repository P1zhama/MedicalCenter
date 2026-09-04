import { useState } from 'react';
import {
  Button,
  Table,
  notificationMessages,
  servicesApi,
  useAsync,
  useDictionaries,
  useToast,
  type ActivityStatus,
  type Specialization,
  type SpecializationListItem,
  type TableColumn,
} from '@mmc/shared';
import { SpecializationModal } from '../../features/specializations/SpecializationModal';
import styles from './SpecializationsPage.module.css';

export function SpecializationsPage() {
  const toast = useToast();
  const dictionaries = useDictionaries();

  const [editing, setEditing] = useState<Specialization | null>(null);
  const [isCreating, setIsCreating] = useState(false);

  const { data, isLoading, reload } = useAsync(() => servicesApi.getSpecializations(), []);

  const changeStatus = async (row: SpecializationListItem, status: ActivityStatus) => {
    try {
      await servicesApi.changeSpecializationStatus(row.id, status);
      toast.showSuccess(notificationMessages.savedSuccessfully);
      reload();
      dictionaries.reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const open = async (row: SpecializationListItem) => {
    try {
      setEditing(await servicesApi.getSpecializationById(row.id));
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const columns: TableColumn<SpecializationListItem>[] = [
    { key: 'name', header: 'Specialization', render: (row) => row.name },
    {
      key: 'status',
      header: 'Status',
      render: (row) => (
        <div className={styles.statusCell}>
          <label className={styles.radio}>
            <input
              type="radio"
              name={`spec-${row.id}`}
              checked={row.status === 'Active'}
              onChange={() => void changeStatus(row, 'Active')}
            />
            Active
          </label>

          <label className={styles.radio}>
            <input
              type="radio"
              name={`spec-${row.id}`}
              checked={row.status === 'Inactive'}
              onChange={() => void changeStatus(row, 'Inactive')}
            />
            Inactive
          </label>
        </div>
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
        <h2 className={styles.title}>Specializations</h2>
        <Button onClick={() => setIsCreating(true)}>Create specialization</Button>
      </div>

      <Table
        columns={columns}
        rows={data ?? []}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="There are no specializations yet"
      />

      {isCreating || editing !== null ? (
        <SpecializationModal
          specialization={editing}
          onSaved={() => {
            setIsCreating(false);
            setEditing(null);
            reload();
            dictionaries.reload();
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
