import { Hero, useAuth } from '@mmc/shared';

export function HomePage() {
  const { session } = useAuth();

  return (
    <Hero title="Welcome to the Medical Center">
      {session === null ? (
        <p>Please sign in to make an appointment with a doctor and check your personal page.</p>
      ) : (
        <p>You are signed in. Doctors, services and appointments are coming in the next steps.</p>
      )}
    </Hero>
  );
}
