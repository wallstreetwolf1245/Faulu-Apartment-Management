/**
 * Building Service
 * Handles all building-related API calls
 */

import { apiCall } from './api';

const buildingService = {
  /**
   * Get all buildings
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getAllBuildings: async () => {
    const result = await apiCall('GET', '/buildings');
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get building by ID
   * @param {number} id - Building ID
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  getBuildingById: async (id) => {
    const result = await apiCall('GET', `/buildings/${id}`);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Create new building
   * @param {object} buildingData - Building data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  createBuilding: async (buildingData) => {
    const result = await apiCall('POST', '/buildings', buildingData);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Update building
   * @param {number} id - Building ID
   * @param {object} buildingData - Updated building data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  updateBuilding: async (id, buildingData) => {
    const result = await apiCall('PUT', `/buildings/${id}`, {
      id,
      ...buildingData,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Delete building
   * @param {number} id - Building ID
   * @returns {Promise<{success: boolean, error?: string}>}
   */
  deleteBuilding: async (id) => {
    const result = await apiCall('DELETE', `/buildings/${id}`);
    return {
      success: result.success,
      error: result.message,
    };
  },
};

export default buildingService;
