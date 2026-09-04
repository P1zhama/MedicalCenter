import { useEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Alert, ApiError, PageSpinner, authApi, notificationMessages } from '@mmc/shared';
import styles from './ConfirmEmailPage.module.css';

type ConfirmState = 'pending' | 'confirmed' | 'failed';

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const hasToken = token !== null && token.length > 0;

  const [state, setState] = useState<ConfirmState>(() => (hasToken ? 'pending' : 'failed'));
  const requested = useRef(false);

  useEffect(() => {
    if (!hasToken || requested.current) {
      return;
    }

    requested.current = true;

    authApi
      .confirmEmail({ token })
      .then(() => setState('confirmed'))
      .catch((error: unknown) => {
        setState(error instanceof ApiError && error.isConflict ? 'confirmed' : 'failed');
      });
  }, [token, hasToken]);

  return (
    <section className={styles.page}>
      <h2>Email confirmation</h2>

      {state === 'pending' ? <PageSpinner label="Confirming your email" /> : null}

      {state === 'confirmed' ? (
        <>
          <Alert variant="success">{notificationMessages.emailConfirmed}</Alert>
          <p className={styles.hint}>
            Creating your patient profile is the next step and will be available shortly.
          </p>
        </>
      ) : null}

      {state === 'failed' ? (
        <Alert variant="error">{notificationMessages.emailConfirmationFailed}</Alert>
      ) : null}
    </section>
  );
}
