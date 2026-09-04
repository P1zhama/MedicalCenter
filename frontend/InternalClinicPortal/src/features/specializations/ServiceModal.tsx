import { useState } from 'react';
import {
  Button,
  ConfirmDialog,
  Field,
  Modal,
  Select,
  TextInput,
  confirmMessages,
  required,
  useForm,
  validatePrice,
  validationMessages,
  type ActivityStatus,
  type NewServiceItem,
  type SelectOption,
} from '@mmc/shared';

const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export interface ServiceModalProps {
  categories: readonly SelectOption[];
  onAdd: (service: NewServiceItem) => void;
  onClose: () => void;
}

export function ServiceModal({ categories, onAdd, onClose }: ServiceModalProps) {
  const [status, setStatus] = useState<ActivityStatus>('Active');
  const [isCancelling, setIsCancelling] = useState(false);

  const { values, errors, isValid, setValue, handleBlur, validateAll } = useForm({
    initialValues: { name: '', price: '', categoryId: '' },
    validators: {
      name: required(validationMessages.nameRequired),
      price: validatePrice,
      categoryId: required(validationMessages.serviceCategoryRequired),
    },
  });

  const submit = () => {
    if (!validateAll()) {
      return;
    }

    onAdd({
      name: values.name,
      price: Number(values.price),
      categoryId: values.categoryId,
      status,
    });
  };

  return (
    <>
      <Modal
        title="Add service"
        onClose={() => setIsCancelling(true)}
        closeOnOverlayClick={false}
        elevated
        footer={
          <>
            <Button variant="outline" onClick={() => setIsCancelling(true)}>
              Cancel
            </Button>
            <Button disabled={!isValid} onClick={submit}>
              Confirm
            </Button>
          </>
        }
      >
        <Field label="Service name" required error={errors.name}>
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              value={values.name}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={(value) => setValue('name', value)}
              onBlur={() => handleBlur('name')}
            />
          )}
        </Field>

        <Field label="Price" required error={errors.price}>
          {({ id, hasError, describedBy }) => (
            <TextInput
              id={id}
              type="number"
              min="0"
              step="0.01"
              value={values.price}
              invalid={hasError}
              aria-describedby={describedBy}
              onValueChange={(value) => setValue('price', value)}
              onBlur={() => handleBlur('price')}
            />
          )}
        </Field>

        <Field label="Service category" required error={errors.categoryId}>
          {({ id, hasError }) => (
            <Select
              id={id}
              value={values.categoryId}
              options={categories}
              placeholder="Choose the service category"
              invalid={hasError}
              onValueChange={(value) => setValue('categoryId', value)}
              onBlur={() => handleBlur('categoryId')}
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
      </Modal>

      {isCancelling ? (
        <ConfirmDialog
          message={confirmMessages.cancelEntered}
          onConfirm={onClose}
          onCancel={() => setIsCancelling(false)}
        />
      ) : null}
    </>
  );
}
