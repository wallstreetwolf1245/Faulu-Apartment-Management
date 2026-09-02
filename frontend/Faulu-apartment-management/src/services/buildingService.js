import api from './api';

const buildingService = {
  getAllBuildings: async () => {
    try {
      const response = await api.get('/buildings');
      return { success: response.data.success, data: response.data.data };
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getBuildingById: async (id) => {
    try {
      const response = await api.get(`/buildings/${id}`);
      return { success: response.data.success, data: response.data.data };
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  createBuilding: async (buildingData) => {
    try {
      const response = await api.post('/buildings', buildingData);
      return { success: response.data.success, data: response.data.data };
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  updateBuilding: async (id, buildingData) => {
    try {
      const response = await api.put(`/buildings/${id}`, { id, ...buildingData });
      return { success: response.data.success, data: response.data.data };
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  deleteBuilding: async (id) => {
    try {
      const response = await api.delete(`/buildings/${id}`);
      return { success: response.data.success };
    } catch (error) {
      return { success: false, error: error.message };
    }
  },
};

export default buildingService;
