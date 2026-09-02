/**
 * Authentication Service
 * Handles user login, registration, and authentication state
 */

import api, { apiCall } from './api';

const authService = {
  /**
   * Login user with email and password
   * @param {string} email - User email
   * @param {string} password - User password
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  login: async (email, password) => {
    const result = await apiCall('POST', '/auth/login', { email, password });

    if (result.success && result.data?.token) {
      localStorage.setItem('authToken', result.data.token);
      localStorage.setItem('user', JSON.stringify(result.data));
      localStorage.setItem('userRoles', JSON.stringify(result.data.roles || []));
      return { success: true, data: result.data };
    }

    return { success: false, error: result.message };
  },

  /**
   * Register new user
   * @param {object} userData - User data (email, firstName, lastName, password)
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  register: async (userData) => {
    const result = await apiCall('POST', '/auth/register', {
      ...userData,
      userType: userData.userType || 'Tenant',
      confirmPassword: userData.password,
    });

    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Logout user - clear local storage
   */
  logout: () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    localStorage.removeItem('userRoles');
  },

  /**
   * Get current logged-in user from localStorage
   * @returns {object|null}
   */
  getCurrentUser: () => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },

  /**
   * Get user roles
   * @returns {array}
   */
  getUserRoles: () => {
    const roles = localStorage.getItem('userRoles');
    return roles ? JSON.parse(roles) : [];
  },

  /**
   * Check if user is authenticated
   * @returns {boolean}
   */
  isAuthenticated: () => {
    return !!localStorage.getItem('authToken');
  },

  /**
   * Check if user has specific role
   * @param {string} role - Role to check
   * @returns {boolean}
   */
  hasRole: (role) => {
    const roles = authService.getUserRoles();
    return roles.includes(role);
  },

  /**
   * Get JWT token
   * @returns {string|null}
   */
  getToken: () => {
    return localStorage.getItem('authToken');
  },
};

export default authService;
