import { useState } from 'react';
import axios from 'axios';
import './SignUpModal.css';

const SignUpModal = ({ onClose, onSignInClick }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rePassword, setRePassword] = useState('');

  const [emailError, setEmailError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [rePasswordError, setRePasswordError] = useState('');

  const [showPassword, setShowPassword] = useState(false);
  const [showRePassword, setShowRePassword] = useState(false);

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');


  const handleEmailBlur = () => validateEmail(email);
  const handlePasswordBlur = () => {
    validatePassword(password);
    if (rePassword) validateRePassword(rePassword, password);
  };
  const handleRePasswordBlur = () => validateRePassword(rePassword, password);


  const validateEmail = (value) => {
    if (!value) {
      setEmailError('Please, enter the email');
      return false;
    }
    if (!value.includes('@')) {
      setEmailError("You've entered an invalid email");
      return false;
    }
    setEmailError('');
    return true;
  };

  const validatePassword = (value) => {
    if (!value) {
      setPasswordError('Please, enter the password');
      return false;
    }
    if (value.length < 6 || value.length > 15) {
      setPasswordError('Password must be between 6 and 15 symbols');
      return false;
    }
    setPasswordError('');
    return true;
  };

  const validateRePassword = (value, currentPassword) => {
    if (!value) {
      setRePasswordError('Please, reenter the password');
      return false;
    }
    if (value !== currentPassword) {
      setRePasswordError('The passwords you’ve entered don’t coincide');
      return false;
    }
    setRePasswordError('');
    return true;
  };

  const isFormValid = 
    email !== '' && 
    password !== '' && 
    rePassword !== '' && 
    emailError === '' && 
    passwordError === '' && 
    rePasswordError === '' &&
    password.length >= 6 && 
    password.length <= 15 &&
    password === rePassword &&
    email.includes('@');

  const isButtonDisabled = !isFormValid || isSubmitting;

  // --- бэк переделать зарос к Gateway ---
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isButtonDisabled) return;

    setIsSubmitting(true);
    setSuccessMessage('');

    try {
      const response = await axios.post('/api/auth/sign-up', {
        email: email,
        password: password
      });

      setSuccessMessage(response.data.message || 'An email with a link has been sent to confirm signing up.');
      
      setEmail(''); setPassword(''); setRePassword('');
      
    } catch (error) {
      if (error.response && error.response.status === 400) {
        setEmailError('User with this email already exists');
      } else {
        setEmailError('An unexpected error occurred. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-btn" onClick={onClose} aria-label="Close modal">
          &times;
        </button>

        <h2 className="modal-title">Sign up</h2>

        {successMessage && <div className="success-message">{successMessage}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">E-mail *</label>
            <input
              type="text"
              id="email"
              className={`form-control ${emailError ? 'input-error' : ''}`}
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                if (emailError) validateEmail(e.target.value);
              }}
              onBlur={handleEmailBlur}
              placeholder="Enter your email"
            />
            {emailError && <span className="error-message">{emailError}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="password">Password *</label>
            <div className="password-wrapper">
              <input
                type={showPassword ? 'text' : 'password'}
                id="password"
                className={`form-control ${passwordError ? 'input-error' : ''}`}
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value);
                  if (passwordError) validatePassword(e.target.value);
                }}
                onBlur={handlePasswordBlur}
                placeholder="6-15 symbols"
              />
              <button
                type="button"
                className="eye-icon"
                onClick={() => setShowPassword(!showPassword)}
                aria-label="Toggle password visibility"
              >
                {showPassword ? 'Hide' : 'Show'}
              </button>
            </div>
            {passwordError && <span className="error-message">{passwordError}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="rePassword">Repeat entered password *</label>
            <div className="password-wrapper">
              <input
                type={showRePassword ? 'text' : 'password'}
                id="rePassword"
                className={`form-control ${rePasswordError ? 'input-error' : ''}`}
                value={rePassword}
                onChange={(e) => {
                  setRePassword(e.target.value);
                  if (rePasswordError) validateRePassword(e.target.value, password);
                }}
                onBlur={handleRePasswordBlur}
                placeholder="Repeat password"
              />
              <button
                type="button"
                className="eye-icon"
                onClick={() => setShowRePassword(!showRePassword)}
                aria-label="Toggle re-entered password visibility"
              >
                {showRePassword ? 'Hide' : 'Show'}
              </button>
            </div>
            {rePasswordError && <span className="error-message">{rePasswordError}</span>}
          </div>

          <button type="submit" className="submit-btn" disabled={isButtonDisabled}>
            {isSubmitting ? 'Signing up...' : 'Sign up'}
          </button>
        </form>

        <div className="modal-footer">
          <span>Already have an account? </span>
          <button className="link-btn" onClick={onSignInClick}>
            Sign in
          </button>
        </div>
      </div>
    </div>
  );
};

export default SignUpModal;