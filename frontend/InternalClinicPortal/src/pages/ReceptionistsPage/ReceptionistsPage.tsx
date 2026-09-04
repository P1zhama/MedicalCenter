import { useState } from 'react';
import {
  Button,
  ConfirmDialog,
  Table,
  confirmMessages,
  formatFullName,
  notificationMessages,
  profilesApi,
  useAsync,
  useDictionaries,
  useToast,
  type Receptionist,
  type ReceptionistListItem,
  type TableColumn,
} from '@mmc/shared';
import { ReceptionistModal } from '../../features/receptionists/ReceptionistModal';
import styles from './ReceptionistsPage.module.css';

export function ReceptionistsPage() {
  const toast = useToast();
  const { officeAddress } = useDictionaries();

  const [isCreating, setIsCreating] = useState(false);
  const [editing, setEditing] = useState<Receptionist | null>(null);
  const [deleting, setDeleting] = useState<ReceptionistListItem | null>(null);

  const { data, isLoading, reload } = useAsync(() => profilesApi.getReceptionists(), []);

  const open = async (row: ReceptionistListItem) => {
    try {
      setEditing(await profilesApi.getReceptionistById(row.id));
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    }
  };

  const remove = async () => {
    if (deleting === null) {
      return;
    }

    try {
      await profilesApi.deleteReceptionist(deleting.id);
      toast.showSuccess(notificationMessages.profileDeleted);
      reload();
    } catch {
      toast.showError(notificationMessages.unexpectedError);
    } finally {
      setDeleting(null);
    }
  };

  const columns: TableColumn<ReceptionistListItem>[] = [
    { key: 'name', header: 'Full name', render: (row) => formatFullName(row) },
    { key: 'office', header: 'Office', render: (row) => officeAddress(row.officeId) },
    { key: 'status', header: 'Status', render: (row) => row.status },
    {
      key: 'actions',
      header: '',
      render: (row) => (
        <div className={styles.actions}>
          <Button variant="ghost" onClick={() => void open(row)}>
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
        <h2 className={styles.title}>Receptionists</h2>
        <Button onClick={() => setIsCreating(true)}>Create receptionist</Button>
      </div>

      <Table
        columns={columns}
        rows={data ?? []}
        rowKey={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="There are no receptionists yet"
      />

      {isCreating || editing !== null ? (
        <ReceptionistModal
          receptionist={editing}
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
