import { AppFooter, AppHeader, Hero, Modal, PageShell, SignInForm } from '@mmc/shared';

export function SignInGate() {
  return (
    <>
      <PageShell
        header={<AppHeader brand={<h1>Medical Center</h1>} subtitle="Clinic portal" />}
        footer={<AppFooter />}
      >
        <Hero title="Staff workspace">
          <p>Sign in with the credentials issued by the clinic to manage the daily work.</p>
        </Hero>
      </PageShell>

      <Modal
        title="Sign in"
        onClose={() => undefined}
        showCloseButton={false}
        closeOnOverlayClick={false}
      >
        <SignInForm onSuccess={() => undefined} />
      </Modal>
    </>
  );
}
