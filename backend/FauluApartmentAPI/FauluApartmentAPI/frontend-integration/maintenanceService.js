/**
 * Maintenance Service
 * Handles all maintenance order-related API calls
 */

import { apiCall } from './api';

const maintenanceService = {
  /**
   * Get maintenance order by ID
   * @param {number} id - Order ID
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  getOrderById: async (id) => {
    const result = await apiCall('GET', `/maintenance/${id}`);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Get orders by building
   * @param {number} buildingId - Building ID
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getOrdersByBuilding: async (buildingId) => {
    const result = await apiCall('GET', `/maintenance/building/${buildingId}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get all open maintenance orders
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getOpenOrders: async () => {
    const result = await apiCall('GET', '/maintenance/status/open');
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get urgent maintenance orders
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getUrgentOrders: async () => {
    const result = await apiCall('GET', '/maintenance/priority/urgent');
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Create new maintenance order
   * @param {object} orderData - Order data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  createOrder: async (orderData) => {
    const result = await apiCall('POST', '/maintenance', orderData);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Update maintenance order
   * @param {number} id - Order ID
   * @param {object} orderData - Updated order data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  updateOrder: async (id, orderData) => {
    const result = await apiCall('PUT', `/maintenance/${id}`, {
      id,
      ...orderData,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Complete maintenance order
   * @param {number} id - Order ID
   * @param {string} completionNotes - Completion notes
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  completeOrder: async (id, completionNotes) => {
    const result = await apiCall('POST', `/maintenance/${id}/complete`, {
      completionNotes,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Delete maintenance order
   * @param {number} id - Order ID
   * @returns {Promise<{success: boolean, error?: string}>}
   */
  deleteOrder: async (id) => {
    const result = await apiCall('DELETE', `/maintenance/${id}`);
    return {
      success: result.success,
      error: result.message,
    };
  },
};

export default maintenanceService;
