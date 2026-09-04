import { useMemo, useState } from 'react';
import { Card, EmptyState, PageSpinner, Tabs, formatPrice, useDictionaries } from '@mmc/shared';
import styles from './ServicesPage.module.css';

export function ServicesPage() {
  const { catalog, isLoading } = useDictionaries();
  const [activeId, setActiveId] = useState<string>('');

  const tabs = useMemo(
    () => (catalog?.categories ?? []).map((category) => ({ id: category.id, label: category.name })),
    [catalog],
  );

  const defaultId =
    tabs.find((tab) => tab.label.toLowerCase().startsWith('consultation'))?.id ?? tabs[0]?.id ?? '';

  const currentId = activeId !== '' ? activeId : defaultId;
  const current = catalog?.categories.find((category) => category.id === currentId) ?? null;

  if (isLoading) {
    return <PageSpinner label="Loading services" />;
  }

  if (catalog === null || tabs.length === 0) {
    return <EmptyState message="There are no services available at the moment" />;
  }

  return (
    <section className={styles.page}>
      <h2 className={styles.title}>Services</h2>

      <Tabs items={tabs} activeId={currentId} onChange={setActiveId} />

      {current === null || current.specializations.length === 0 ? (
        <EmptyState message="There are no services in this category" />
      ) : (
        <div className={styles.groups}>
          {current.specializations.map((specialization) => (
            <Card key={specialization.id} title={specialization.name}>
              <ul className={styles.services}>
                {specialization.services.map((service) => (
                  <li key={service.id} className={styles.service}>
                    <span>{service.name}</span>
                    <span className={styles.price}>{formatPrice(service.price)}</span>
                  </li>
                ))}
              </ul>
            </Card>
          ))}
        </div>
      )}
    </section>
  );
}
