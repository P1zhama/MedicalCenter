import { Alert, AppFooter, AppHeader, Button, Card, Hero, PageShell, useAuth } from '@mmc/shared';

export function AccessDenied() {
  const { signOut } = useAuth();

  return (
    <PageShell
      header={<AppHeader brand={<h1>Medical Center</h1>} subtitle="Clinic portal" />}
      footer={<AppFooter />}
    >
      <Hero title="Access denied">
        <Card>
          <Alert variant="error">This portal is available for clinic staff only</Alert>

          <Button variant="outline" onClick={() => void signOut()}>
            Sign out
          </Button>
        </Card>
      </Hero>
    </PageShell>
  );
}
