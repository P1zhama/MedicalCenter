import { Button, EmptyState } from '@mmc/shared';
import { useAppUi } from '../../app/useAppUi';

export function SignInRequired() {
  const { openSignIn } = useAppUi();

  return (
    <EmptyState message="Please sign in to see this page">
      <Button onClick={openSignIn}>Sign in</Button>
    </EmptyState>
  );
}
