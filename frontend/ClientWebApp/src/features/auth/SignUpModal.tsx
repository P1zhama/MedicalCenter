import { Button, Modal, SignUpForm } from '@mmc/shared';

export interface SignUpModalProps {
  onClose: () => void;
  onSignInClick: () => void;
}

export function SignUpModal({ onClose, onSignInClick }: SignUpModalProps) {
  return (
    <Modal title="Sign up" onClose={onClose}>
      <SignUpForm
        footer={
          <>
            <span>Already have an account? </span>
            <Button variant="ghost" onClick={onSignInClick}>
              Sign in
            </Button>
          </>
        }
      />
    </Modal>
  );
}
