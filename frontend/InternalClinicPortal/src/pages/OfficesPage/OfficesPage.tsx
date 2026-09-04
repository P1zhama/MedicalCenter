import { useState } from 'react';
import {
  Button,
  Table,
  notificationMessages,
  officesApi,
  useAsync,
  useDictionaries,
  useToast,
  type ActivityStatus,
  type Office,
  type OfficeListItem,
  type TableColumn,
} from '@mmc/shared';
import { OfficeModal } from '../../features/offices/OfficeModal';
import styles from './OfficesPage.module.css';

export function OfficesPage() {
  const toast = useToast();
  const dictionaries = useDictionaries();

  const [editing, setEditing] = useState<Office | null>(null);
  const [isCreating, setIsCreating] = useState(false);

  const { data, isLoading, reload } = useAsync(() => officesApi.getAll(), []);

  const changeStatus = async (row: OfficeListItem, status: ActivityStatus) => {
    try {
      await officesApi.changeStatus(row.id, status);
      toast.showSuccess(notificationMessages.savedSuccessfully);
      reload();
      dictionaries.reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const openEditor = async (row: OfficeListItem) => {
    try {
      setEditing(await officesApi.getById(row.id));
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const columns: TableColumn<OfficeListItem>[] = [
    { key: 'address', header: 'Address', render: (row) => row.address },
    { key: 'phone', header: 'Registry phone', render: (row) => row.registryPhoneNumber },
    {
      key: 'status',
      header: 'Status',
      render: (row) => (
        <div className={styles.statusCell}>
          <label className={styles.radio}>
            <input
              type="radio"
              name={`status-${row.id}`}
              checked={row.status === 'Active'}
              onChange={() => void changeStatus(row, 'Active')}
            />
            Active
          </label>

          <label className={styles.radio}>
            <input
              type="radio"
              name={`status-${row.id}`}
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
        <Button variant="ghost" onClick={() => void openEditor(row)}>
          Open
        </Button>
      ),
    },
  ];

  return (
    <section>
      <div className={styles.header}>
        <h2 className={styles.title}>Offices</h2>
        <Button onClick={() => setIsCreating(true)}>Create office</Button>
      </div>

      <Table
        columns={columns}
        rows={data ?? []}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="There are no offices yet"
      />

      {isCreating || editing !== null ? (
        <OfficeModal
          office={editing}
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
