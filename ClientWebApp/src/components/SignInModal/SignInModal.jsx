import { useState } from 'react';
import axios from 'axios';
import './SignInModal.css';

const SignInModal = ({ onClose, onSignUpClick }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const [emailError, setEmailError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [globalError, setGlobalError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleEmailBlur = () => validateEmail(email);
  const handlePasswordBlur = () => validatePassword(password);

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

  const isFormValid = 
    email !== '' && 
    password !== '' && 
    emailError === '' && 
    passwordError === '' &&
    password.length >= 6 && 
    password.length <= 15 &&
    email.includes('@');

  const isButtonDisabled = !isFormValid || isSubmitting;

  // --- Вот эту хуету с бэкендом потом не забудь переделать ---
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isButtonDisabled) return;

    setIsSubmitting(true);
    setGlobalError('');
    setSuccessMessage('');

    try {
      const response = await axios.post('/api/auth/sign-in', {
        email: email,
        password: password
      });

      localStorage.setItem('accessToken', response.data.accessToken);
      localStorage.setItem('refreshToken', response.data.refreshToken);

      setSuccessMessage(response.data.message || 'Successfully signed in!');
      
      setTimeout(() => {
        onClose();
      }, 1500);

    } catch (error) {
      if (error.response && (error.response.status === 401 || error.response.status === 400)) {
        
        const errorDetail = error.response.data.error || '';

        if (errorDetail.toLowerCase().includes('not found') || errorDetail.toLowerCase().includes('does not exist')) {
            setEmailError('User with this email doesn’t exist');
        } else {
            setGlobalError('Either an email or a password is incorrect');
        }
      } else {
        setGlobalError('An unexpected error occurred. Please try again.');
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

        <h2 className="modal-title">Sign in</h2>

        {globalError && <div className="global-error">{globalError}</div>}
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

          <button type="submit" className="submit-btn" disabled={isButtonDisabled}>
            {isSubmitting ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <div className="modal-footer">
          <span>Don't have an account? </span>
          <button className="link-btn" onClick={onSignUpClick}>
            Sign up
          </button>
        </div>
      </div>
    </div>
  );
};

export default SignInModal;