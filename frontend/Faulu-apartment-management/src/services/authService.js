import axios from 'axios';

const AUTH_SESSION_KEY = 'fauluAuthSession';
const LEGACY_AUTH_TOKEN_KEY = 'authToken';
const LEGACY_USER_KEY = 'user';

// Same base URL logic as services/api.js. Duplicated intentionally rather
// than imported — authService can't import the shared `api` instance
// without creating a circular import (api.js already imports authService
// to read the token for its request interceptor).
const API_BASE_URL = import.meta.env.PROD
  ? import.meta.env.VITE_API_URL || 'https://localhost:5001/api'
  : '/api';

function getStoredSession() {
  try {
    const raw = localStorage.getItem(AUTH_SESSION_KEY);
    if (!raw) {
      return null;
    }
    const session = JSON.parse(raw);
    if (!session?.token || !session?.user) {
      localStorage.removeItem(AUTH_SESSION_KEY);
      localStorage.removeItem(LEGACY_AUTH_TOKEN_KEY);
      localStorage.removeItem(LEGACY_USER_KEY);
      return null;
    }
    // Note: we no longer track a client-side expiresAt. The real JWT already
    // carries its own `exp` claim that the backend validates — if it's
    // expired, the next API call gets a 401 and api.js's response
    // interceptor already handles logging out at that point.
    return session;
  } catch (error) {
    console.error('Unable to read auth session', error);
    localStorage.removeItem(AUTH_SESSION_KEY);
    return null;
  }
}

function persistSession(session) {
  localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
  localStorage.setItem(LEGACY_AUTH_TOKEN_KEY, session.token);
  localStorage.setItem(LEGACY_USER_KEY, JSON.stringify(session.user));
}

function clearSession() {
  localStorage.removeItem(AUTH_SESSION_KEY);
  localStorage.removeItem(LEGACY_AUTH_TOKEN_KEY);
  localStorage.removeItem(LEGACY_USER_KEY);
}

// Shapes AuthController's LoginResponse (userId/email/firstName/lastName/roles)
// into the `user` object the rest of the app already expects (name/role).
function buildUser(data) {
  return {
    id: data.userId,
    email: data.email,
    firstName: data.firstName,
    lastName: data.lastName,
    name: [data.firstName, data.lastName].filter(Boolean).join(' '),
    roles: data.roles || [],
    role: (data.roles && data.roles[0]) || 'landlord',
  };
}

const authService = {
  // Calls the REAL backend now — previously this checked a hardcoded demo
  // password / localStorage-seeded users and manufactured a fake token
  // locally, which is why every real API call after login got a 401 and
  // immediately logged you back out.
  login: async (email, password) => {
    try {
      const response = await axios.post(`${API_BASE_URL}/auth/login`, { email, password });
      const body = response.data;

      if (!body?.success || !body?.data?.token) {
        return { success: false, error: body?.message || 'Invalid email or password.' };
      }

      const user = buildUser(body.data);
      const session = {
        token: body.data.token,
        refreshToken: body.data.refreshToken,
        user,
      };
      persistSession(session);
      return { success: true, data: user, token: session.token };
    } catch (error) {
      const message = error.response?.data?.message || 'Unable to sign in right now.';
      return { success: false, error: message };
    }
  },

  // Matches Signup.jsx's actual call: (email, firstName, lastName, password, companyName).
  // Signup.jsx already validates password === confirmPassword client-side before
  // calling this, so we can safely send password as confirmPassword to satisfy
  // the backend's required field without asking the caller to pass it twice.
  //
  // IMPORTANT: AuthController's RegisterRequest has no CompanyName field at all —
  // it isn't sent to the backend and is silently dropped here. If you want the
  // company/building name entered at signup to actually persist, that needs a
  // CompanyName column added to the User entity + RegisterRequest DTO on the
  // backend first — flagging this rather than pretending it's handled.
  register: async (email, firstName, lastName, password, companyName) => {
    try {
      const response = await axios.post(`${API_BASE_URL}/auth/register`, {
        email,
        firstName,
        lastName,
        password,
        confirmPassword: password,
      });
      const body = response.data;

      if (!body?.success) {
        return { success: false, error: body?.message || 'Registration failed.' };
      }

      // Register doesn't return a token itself — log in right after so the
      // caller gets the same { success, data, token } shape as login().
      return await authService.login(email, password);
    } catch (error) {
      const message = error.response?.data?.message || 'Unable to create your account right now.';
      return { success: false, error: message };
    }
  },

  logout: () => {
    clearSession();
  },

  getCurrentUser: () => {
    const session = getStoredSession();
    return session?.user || null;
  },

  getToken: () => {
    const session = getStoredSession();
    return session?.token || null;
  },

  isAuthenticated: () => {
    return !!getStoredSession();
  },
};

export default authService;