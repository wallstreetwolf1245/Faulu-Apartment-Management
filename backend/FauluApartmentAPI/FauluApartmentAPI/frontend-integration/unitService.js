/**
 * Unit Service
 * Handles all unit/apartment-related API calls
 */

import { apiCall } from './api';

const unitService = {
  /**
   * Get units by building
   * @param {number} buildingId - Building ID
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getUnitsByBuilding: async (buildingId) => {
    const result = await apiCall('GET', `/units/building/${buildingId}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get unit by ID
   * @param {number} id - Unit ID
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  getUnitById: async (id) => {
    const result = await apiCall('GET', `/units/${id}`);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Get vacant units in building
   * @param {number} buildingId - Building ID
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getVacantUnits: async (buildingId) => {
    const result = await apiCall('GET', `/units/building/${buildingId}/vacant`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Create new unit
   * @param {object} unitData - Unit data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  createUnit: async (unitData) => {
    const result = await apiCall('POST', '/units', unitData);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Update unit
   * @param {number} id - Unit ID
   * @param {object} unitData - Updated unit data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  updateUnit: async (id, unitData) => {
    const result = await apiCall('PUT', `/units/${id}`, {
      id,
      ...unitData,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Delete unit
   * @param {number} id - Unit ID
   * @returns {Promise<{success: boolean, error?: string}>}
   */
  deleteUnit: async (id) => {
    const result = await apiCall('DELETE', `/units/${id}`);
    return {
      success: result.success,
      error: result.message,
    };
  },
};

export default unitService;
