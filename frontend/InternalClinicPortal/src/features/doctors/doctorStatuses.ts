import type { DoctorStatus } from '@mmc/shared';

export const DOCTOR_STATUSES: { value: DoctorStatus; label: string }[] = [
  { value: 'At work', label: 'At work' },
  { value: 'On vacation', label: 'On vacation' },
  { value: 'Sick Day', label: 'Sick Day' },
  { value: 'Sick Leave', label: 'Sick Leave' },
  { value: 'Self-isolation', label: 'Self-isolation' },
  { value: 'Leave without pay', label: 'Leave without pay' },
  { value: 'Inactive', label: 'Inactive' },
];
