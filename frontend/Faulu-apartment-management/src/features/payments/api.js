import api from '../../services/api';
import * as PaymentsApi from '../../services/PaymentsApi';

// Shared currency formatter
export const formatKES = (value) =>
  `KES ${Number(value || 0).toLocaleString('en-KE')}`;

// Helper to safely read response payloads where backend wraps in { success, data }
const unwrap = (resp) => {
  if (!resp) return null;
  // If axios already returned response.data in existing services, allow both shapes
  if (resp.data !== undefined && resp.data !== null) return resp.data;
  return resp;
};

// Payments list for a building (scoped)
export const listPayments = async (buildingId) => {
  try {
    const res = await api.get(`/payments${buildingId ? `?buildingId=${buildingId}` : ''}`);
    return unwrap(res);
  } catch (err) {
    throw err;
  }
};

export const createPayment = async (payload) => {
  return PaymentsApi.createPayment(payload);
};


export const updatePayment = async (id, payload) => {
  return PaymentsApi.updatePayment(id, payload);
};

export const deletePayment = async (id) => {
  return PaymentsApi.deletePayment(id);
};

// Reversals
export const listReversals = async (buildingId) => {
  try {
    if (!buildingId) throw new Error('buildingId required for reversals');
    const res = await api.get(`/buildings/${buildingId}/reversals`);
    return unwrap(res);
  } catch (err) {
    throw err;
  }
};

export const submitReversal = async (paymentId, { amount, reason }) => {
  if (!paymentId) throw new Error('paymentId required');
  const payload = { amount, reason };
  const res = await api.post(`/payments/${paymentId}/reverse`, payload);
  return unwrap(res);
};

// History / Aggregation
export const getHistory = async (buildingId, { period = 'month', startDate, endDate } = {}) => {
  try {
    const q = new URLSearchParams();
    if (buildingId) q.set('buildingId', buildingId);
    q.set('period', period);
    if (startDate) q.set('startDate', startDate);
    if (endDate) q.set('endDate', endDate);
    const res = await api.get(`/payments/history?${q.toString()}`);
    return unwrap(res);
  } catch (err) {
    throw err;
  }
};

export const listMpesaTransactions = async (buildingId) => {
  try {
    const query = buildingId ? `?buildingId=${buildingId}` : '';
    const res = await api.get(`/mpesa/pull-transactions${query}`);
    return unwrap(res);
  } catch (err) {
    throw err;
  }
};

// Expose small helpers used by components for status computation
export const computeStatus = (payment) => {
  // Expecting fields: amountPaid, rentDue, dueDate, status
  if (!payment) return { key: 'unknown', label: 'Unknown' };
  const amtPaid = Number(payment.amountPaid ?? payment.paidAmount ?? 0);
  const due = Number(payment.rentDue ?? payment.amountDue ?? 0);
  const dueDate = payment.dueDate ? new Date(payment.dueDate) : null;
  if (payment.status) {
    return { key: payment.status.toLowerCase(), label: payment.status };
  }
  if (amtPaid === 0 && due > 0) {
    if (dueDate && dueDate < new Date()) return { key: 'overdue', label: 'Overdue' };
    return { key: 'pending', label: 'Pending' };
  }
  if (amtPaid > 0 && amtPaid < due) return { key: 'partial', label: 'Partial' };
  if (amtPaid >= due) return { key: 'paid', label: 'Paid' };
  return { key: 'unknown', label: 'Unknown' };
};

export default {
  formatKES,
  listPayments,
  createPayment,
  updatePayment,
  deletePayment,
  listReversals,
  submitReversal,
  listMpesaTransactions,
  getHistory,
  computeStatus,
};
