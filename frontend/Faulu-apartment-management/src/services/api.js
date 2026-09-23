import axios from 'axios';
import authService from './authService';

const API_BASE_URL = import.meta.env.PROD
  ? (() => {
      const configuredBaseUrl = import.meta.env.VITE_API_URL;
      if (!configuredBaseUrl) {
        throw new Error('VITE_API_URL is not set. Configure the Render backend URL in the production environment.');
      }
      return configuredBaseUrl.replace(/\/$/, '') + '/api';
    })()
  : '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Add JWT token to requests
api.interceptors.request.use(
  (config) => {
    const token = authService.getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Handle response errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      authService.logout();
      window.location.assign('/');
    }
    return Promise.reject(error);
  }
);

export default api;
