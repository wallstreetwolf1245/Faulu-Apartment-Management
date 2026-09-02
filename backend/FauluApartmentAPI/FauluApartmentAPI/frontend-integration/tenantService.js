/**
 * Tenant Service
 * Handles all tenant-related API calls
 */

import { apiCall } from './api';

const tenantService = {
  /**
   * Get all tenants
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getAllTenants: async () => {
    const result = await apiCall('GET', '/tenants');
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get tenant by ID
   * @param {number} id - Tenant ID
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  getTenantById: async (id) => {
    const result = await apiCall('GET', `/tenants/${id}`);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Search tenants by name
   * @param {string} name - Tenant name to search
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  searchTenants: async (name) => {
    const result = await apiCall('GET', `/tenants/search/${name}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Create new tenant
   * @param {object} tenantData - Tenant data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  createTenant: async (tenantData) => {
    const result = await apiCall('POST', '/tenants', tenantData);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Update tenant
   * @param {number} id - Tenant ID
   * @param {object} tenantData - Updated tenant data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  updateTenant: async (id, tenantData) => {
    const result = await apiCall('PUT', `/tenants/${id}`, {
      id,
      ...tenantData,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Delete tenant
   * @param {number} id - Tenant ID
   * @returns {Promise<{success: boolean, error?: string}>}
   */
  deleteTenant: async (id) => {
    const result = await apiCall('DELETE', `/tenants/${id}`);
    return {
      success: result.success,
      error: result.message,
    };
  },
};

export default tenantService;
