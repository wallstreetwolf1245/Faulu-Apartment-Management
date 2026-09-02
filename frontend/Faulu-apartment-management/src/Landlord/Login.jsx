import { useState } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import './Auth.css'
import authService from '../services/authService'

function Login({ onLoginSuccess, onSwitchToSignup }) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errors, setErrors] = useState({});
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const validateForm = () => {
        const newErrors = {};

        if (!email.trim()) {
            newErrors.email = 'Email is required';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            newErrors.email = 'Please enter a valid email address';
        }

        if (!password) {
            newErrors.password = 'Password is required';
        } else if (password.length < 6) {
            newErrors.password = 'Password must be at least 6 characters';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleEmailChange = (e) => {
        setEmail(e.target.value);
        if (errors.email) setErrors(prev => ({ ...prev, email: undefined }));
    };

    const handlePasswordChange = (e) => {
        setPassword(e.target.value);
        if (errors.password) setErrors(prev => ({ ...prev, password: undefined }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        setLoading(true);

        try {
            const result = await authService.login(email.trim().toLowerCase(), password);

            if (result.success) {
                onLoginSuccess(result.data);
            } else {
                setErrors({ submit: result.error || 'Invalid email or password' });
            }
        } catch (error) {
            console.error('Login error:', error);
            setErrors({ submit: 'Login failed. Please try again.' });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <div className="auth-card">
                <div className="auth-header">
                    <h1>Faulu</h1>
                    <p>Apartment Management System</p>
                </div>

                <form className="auth-form" onSubmit={handleSubmit} noValidate>
                    <h2>Sign In</h2>

                    {errors.submit && (
                        <div className="error-banner" role="alert" aria-live="assertive">
                            {errors.submit}
                        </div>
                    )}

                    <div className="form-group">
                        <label htmlFor="email">Email Address</label>
                        <input
                            id="email"
                            name="email"
                            type="email"
                            autoComplete="email"
                            autoFocus
                            value={email}
                            onChange={handleEmailChange}
                            placeholder="you@example.com"
                            className={errors.email ? 'input-error' : ''}
                            disabled={loading}
                            aria-invalid={!!errors.email}
                            aria-describedby={errors.email ? 'email-error' : undefined}
                        />
                        {errors.email && (
                            <span id="email-error" className="error-text" role="alert">
                                {errors.email}
                            </span>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="password">Password</label>
                        <div className="password-input-wrapper">
                            <input
                                id="password"
                                name="password"
                                type={showPassword ? 'text' : 'password'}
                                autoComplete="current-password"
                                value={password}
                                onChange={handlePasswordChange}
                                placeholder="••••••••"
                                className={errors.password ? 'input-error' : ''}
                                disabled={loading}
                                aria-invalid={!!errors.password}
                                aria-describedby={errors.password ? 'password-error' : undefined}
                            />
                            <button
                                type="button"
                                className="password-toggle"
                                onClick={() => setShowPassword(!showPassword)}
                                disabled={loading}
                                aria-label={showPassword ? 'Hide password' : 'Show password'}
                                aria-pressed={showPassword}
                            >
                                {showPassword ? (
                                    <EyeOff size={18} aria-hidden="true" />
                                ) : (
                                    <Eye size={18} aria-hidden="true" />
                                )}
                            </button>
                        </div>
                        {errors.password && (
                            <span id="password-error" className="error-text" role="alert">
                                {errors.password}
                            </span>
                        )}
                    </div>

                    <button
                        type="submit"
                        className="submit-btn"
                        disabled={loading}
                        aria-busy={loading}
                    >
                        {loading ? 'Signing In...' : 'Sign In'}
                    </button>

                    <div className="auth-divider">or</div>

                    <button
                        type="button"
                        className="switch-btn"
                        onClick={onSwitchToSignup}
                        disabled={loading}
                    >
                        Create a new account
                    </button>

                    {import.meta.env.DEV && (
                        <div className="demo-hint">
                            <p>Demo admin: <strong>admin@faulu.local</strong> / <strong>admin123</strong></p>
                        </div>
                    )}
                </form>
            </div>
        </div>
    );
}

export default Login;
