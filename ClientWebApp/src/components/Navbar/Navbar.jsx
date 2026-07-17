import { useState } from 'react';
import Button from '../Button/Button';
import SignUpModal from '../SignUpModal/SignUpModal';
import SignInModal from '../SignInModal/SignInModal';
import './Navbar.css';

const Navbar = () => {
  const [isSignUpOpen, setIsSignUpOpen] = useState(false);
  const [isSignInOpen, setIsSignInOpen] = useState(false);

  const handleOpenSignUp = () => {
    setIsSignInOpen(false);
    setIsSignUpOpen(true);
  };

  const handleOpenSignIn = () => {
    setIsSignUpOpen(false);
    setIsSignInOpen(true);
  };

  return (
    <>
      <nav className="navbar">
        <div className="navbar-brand">
          <h1>Medical Center</h1>
        </div>

        <div className="navbar-actions">
          <Button variant="outline-white" onClick={handleOpenSignIn}>
            Sign in
          </Button>

          <Button variant="white" onClick={handleOpenSignUp}>
            Sign up
          </Button>
        </div>
      </nav>

      {isSignUpOpen && (
        <SignUpModal
          onClose={() => setIsSignUpOpen(false)}
          onSignInClick={handleOpenSignIn}
        />
      )}

      {isSignInOpen && (
        <SignInModal
          onClose={() => setIsSignInOpen(false)}
          onSignUpClick={handleOpenSignUp}
        />
      )}
    </>
  );
};

export default Navbar;