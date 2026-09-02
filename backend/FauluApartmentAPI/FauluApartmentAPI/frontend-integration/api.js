/**
 * API Configuration and HTTP Client
 * Handles all HTTP requests to the backend API
 */

import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:5001';

/**
 * Create axios instance with default config
 */
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: parseInt(process.env.REACT_APP_API_TIMEOUT || '30000'),
});

/**
 * Request Interceptor - Add JWT token to all requests
 */
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/**
 * Response Interceptor - Handle errors globally
 */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle 401 Unauthorized
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }

    // Handle 403 Forbidden
    if (error.response?.status === 403) {
      console.error('Access denied:', error.response.data.message);
    }

    return Promise.reject(error);
  }
);

/**
 * Wrapper function for API calls with standardized error handling
 */
export const apiCall = async (method, url, data = null, config = {}) => {
  try {
    const response = await api({
      method,
      url,
      data,
      ...config,
    });

    return {
      success: response.data.success !== false,
      data: response.data.data,
      message: response.data.message,
      status: response.status,
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      message: error.response?.data?.message || error.message,
      status: error.response?.status,
      error: error,
    };
  }
};

export default api;
