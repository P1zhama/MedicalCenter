import { Button, Modal, SignInForm } from '@mmc/shared';

export interface SignInModalProps {
  onClose: () => void;
  onSignUpClick: () => void;
}

export function SignInModal({ onClose, onSignUpClick }: SignInModalProps) {
  return (
    <Modal title="Sign in" onClose={onClose}>
      <SignInForm
        onSuccess={onClose}
        footer={
          <>
            <span>Don&apos;t have an account? </span>
            <Button variant="ghost" onClick={onSignUpClick}>
              Sign up
            </Button>
          </>
        }
      />
    </Modal>
  );
}
