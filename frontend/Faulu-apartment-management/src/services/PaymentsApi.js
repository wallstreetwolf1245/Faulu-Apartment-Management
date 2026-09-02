import api from './api';

// ── Payments ───────────────────────────────────────────────────────────────

export const getPayments = async () => {
    const response = await api.get('/payments');
    return response.data;
};

export const createPayment = async (data) => {
    const response = await api.post('/payments', data);
    return response.data;
};


export const updatePayment = async (id, data) => {
    const response = await api.put(`/payments/${id}`, data);
    return response.data;
};

export const deletePayment = async (id) => {
    const response = await api.delete(`/payments/${id}`);
    return response.data;
};

// ── M-Pesa C2B ─────────────────────────────────────────────────────────────

// Manually assign an Unmatched M-Pesa payment to a unit
export const matchPayment = async (paymentId, unitId) => {
    const response = await api.put(`/mpesa/payments/${paymentId}/match`, { unitId });
    return response.data;
};