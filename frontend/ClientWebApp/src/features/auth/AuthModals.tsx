import { SignInModal } from './SignInModal';
import { SignUpModal } from './SignUpModal';

export type AuthModalMode = 'signIn' | 'signUp';

export interface AuthModalsProps {
  mode: AuthModalMode | null;
  onClose: () => void;
  onModeChange: (mode: AuthModalMode) => void;
}

export function AuthModals({ mode, onClose, onModeChange }: AuthModalsProps) {
  if (mode === 'signIn') {
    return <SignInModal onClose={onClose} onSignUpClick={() => onModeChange('signUp')} />;
  }

  if (mode === 'signUp') {
    return <SignUpModal onClose={onClose} onSignInClick={() => onModeChange('signIn')} />;
  }

  return null;
}
