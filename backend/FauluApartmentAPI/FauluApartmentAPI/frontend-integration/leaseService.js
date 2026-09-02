/**
 * Lease Service
 * Handles all lease-related API calls
 */

import { apiCall } from './api';

const leaseService = {
  /**
   * Get lease by ID
   * @param {number} id - Lease ID
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  getLeaseById: async (id) => {
    const result = await apiCall('GET', `/leases/${id}`);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Get leases by tenant
   * @param {number} tenantId - Tenant ID
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getLeasesByTenant: async (tenantId) => {
    const result = await apiCall('GET', `/leases/tenant/${tenantId}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get leases by unit
   * @param {number} unitId - Unit ID
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getLeasesByUnit: async (unitId) => {
    const result = await apiCall('GET', `/leases/unit/${unitId}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get expiring leases
   * @param {number} daysUntilExpiry - Number of days to check
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getExpiringLeases: async (daysUntilExpiry = 30) => {
    const result = await apiCall('GET', `/leases/expiring/${daysUntilExpiry}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Create new lease
   * @param {object} leaseData - Lease data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  createLease: async (leaseData) => {
    const result = await apiCall('POST', '/leases', leaseData);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Update lease
   * @param {number} id - Lease ID
   * @param {object} leaseData - Updated lease data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  updateLease: async (id, leaseData) => {
    const result = await apiCall('PUT', `/leases/${id}`, {
      id,
      ...leaseData,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Renew lease
   * @param {number} id - Lease ID
   * @param {string} newEndDate - New end date
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  renewLease: async (id, newEndDate) => {
    const result = await apiCall('POST', `/leases/${id}/renew`, {
      newEndDate,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Terminate lease
   * @param {number} id - Lease ID
   * @returns {Promise<{success: boolean, error?: string}>}
   */
  terminateLease: async (id) => {
    const result = await apiCall('POST', `/leases/${id}/terminate`);
    return {
      success: result.success,
      error: result.message,
    };
  },
};

export default leaseService;
