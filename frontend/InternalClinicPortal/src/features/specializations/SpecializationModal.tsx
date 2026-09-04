import { useState } from 'react';
import {
  Alert,
  Button,
  ConfirmDialog,
  Field,
  Modal,
  Select,
  TextInput,
  confirmMessages,
  formatPrice,
  notificationMessages,
  servicesApi,
  useDictionaries,
  useToast,
  validationMessages,
  type ActivityStatus,
  type NewServiceItem,
  type Specialization,
} from '@mmc/shared';
import { ServiceModal } from './ServiceModal';
import styles from './SpecializationModal.module.css';

const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export interface SpecializationModalProps {
  specialization: Specialization | null;
  onSaved: () => void;
  onClose: () => void;
}

export function SpecializationModal({
  specialization,
  onSaved,
  onClose,
}: SpecializationModalProps) {
  const toast = useToast();
  const { serviceCategories } = useDictionaries();

  const [name, setName] = useState(specialization?.name ?? '');
  const [nameTouched, setNameTouched] = useState(false);
  const [status, setStatus] = useState<ActivityStatus>(specialization?.status ?? 'Active');
  const [newServices, setNewServices] = useState<NewServiceItem[]>([]);
  const [isAddingService, setIsAddingService] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  const categories = serviceCategories.map((category) => ({
    value: category.id,
    label: category.name,
  }));

  const existingServices = specialization?.services ?? [];
  const isCreate = specialization === null;
  const hasServices = isCreate ? newServices.length > 0 : existingServices.length > 0;
  const canSave = name.trim().length > 0 && hasServices;

  const save = async () => {
    if (!canSave || isSaving) {
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      if (isCreate) {
        await servicesApi.createSpecialization({ name, status, services: newServices });
        toast.showSuccess(notificationMessages.createdSuccessfully);
      } else {
        await servicesApi.updateSpecialization(specialization.id, { name, status });

        for (const service of newServices) {
          await servicesApi.createService({
            name: service.name,
            price: service.price,
            specializationId: specialization.id,
            categoryId: service.categoryId,
            status: service.status,
          });
        }

        toast.showSuccess(notificationMessages.savedSuccessfully);
      }

      onSaved();
    } catch {
      setError(notificationMessages.unexpectedError);
      setIsSaving(false);
    }
  };

  const categoryName = (categoryId: string) =>
    categories.find((category) => category.value === categoryId)?.label ?? '';

  return (
    <>
      <Modal
        title={isCreate ? 'Create specialization' : 'Edit specialization'}
        size="large"
        onClose={() => setIsCancelling(true)}
        closeOnOverlayClick={false}
        footer={
          <>
            <Button variant="outline" onClick={() => setIsCancelling(true)}>
              Cancel
            </Button>
            <Button loading={isSaving} disabled={!canSave} onClick={() => void save()}>
              {isCreate ? 'Confirm' : 'Save changes'}
            </Button>
          </>
        }
      >
        {error !== null ? <Alert variant="error">{error}</Alert> : null}

        <Field
          label="Name"
          required
          error={nameTouched && name.trim().length === 0 ? validationMessages.nameRequired : undefined}
        >
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              value={name}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={setName}
              onBlur={() => setNameTouched(true)}
            />
          )}
        </Field>

        <Field label="Status" required>
          {({ id }) => (
            <Select
              id={id}
              value={status}
              options={STATUS_OPTIONS}
              onValueChange={(value) => setStatus(value as ActivityStatus)}
            />
          )}
        </Field>

        <div className={styles.servicesHeader}>
          <h3 className={styles.servicesTitle}>Services</h3>
          <Button variant="outline" onClick={() => setIsAddingService(true)}>
            Add service
          </Button>
        </div>

        <table className={styles.services}>
          <thead>
            <tr>
              <th>Name</th>
              <th>Price</th>
              <th>Category</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {existingServices.map((service) => (
              <tr key={service.id}>
                <td>{service.name}</td>
                <td>{formatPrice(service.price)}</td>
                <td>{service.categoryName}</td>
                <td>{service.status}</td>
              </tr>
            ))}

            {newServices.map((service, index) => (
              <tr key={`new-${index}`} className={styles.newRow}>
                <td>{service.name}</td>
                <td>{formatPrice(service.price)}</td>
                <td>{categoryName(service.categoryId)}</td>
                <td>{service.status}</td>
              </tr>
            ))}

            {existingServices.length === 0 && newServices.length === 0 ? (
              <tr>
                <td colSpan={4} className={styles.emptyRow}>
                  Add at least one service
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </Modal>

      {isAddingService ? (
        <ServiceModal
          categories={categories}
          onAdd={(service) => {
            setNewServices((previous) => [...previous, service]);
            setIsAddingService(false);
          }}
          onClose={() => setIsAddingService(false)}
        />
      ) : null}

      {isCancelling ? (
        <ConfirmDialog
          message={isCreate ? confirmMessages.cancelEntered : confirmMessages.cancelChanges}
          onConfirm={onClose}
          onCancel={() => setIsCancelling(false)}
        />
      ) : null}
    </>
  );
}
