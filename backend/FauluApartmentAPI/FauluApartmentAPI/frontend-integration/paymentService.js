/**
 * Payment Service
 * Handles all payment-related API calls
 */

import { apiCall } from './api';

const paymentService = {
  /**
   * Get payment by ID
   * @param {number} id - Payment ID
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  getPaymentById: async (id) => {
    const result = await apiCall('GET', `/payments/${id}`);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Get payments by tenant
   * @param {number} tenantId - Tenant ID
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getPaymentsByTenant: async (tenantId) => {
    const result = await apiCall('GET', `/payments/tenant/${tenantId}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get payments by lease
   * @param {number} leaseId - Lease ID
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getPaymentsByLease: async (leaseId) => {
    const result = await apiCall('GET', `/payments/lease/${leaseId}`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get overdue payments
   * @returns {Promise<{success: boolean, data?: array, error?: string}>}
   */
  getOverduePayments: async () => {
    const result = await apiCall('GET', '/payments/status/overdue');
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },

  /**
   * Get total outstanding amount for tenant
   * @param {number} tenantId - Tenant ID
   * @returns {Promise<{success: boolean, data?: number, error?: string}>}
   */
  getTotalOutstanding: async (tenantId) => {
    const result = await apiCall('GET', `/payments/tenant/${tenantId}/outstanding`);
    return {
      success: result.success,
      data: result.data || 0,
      error: result.message,
    };
  },

  /**
   * Create new payment
   * @param {object} paymentData - Payment data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  createPayment: async (paymentData) => {
    const result = await apiCall('POST', '/payments', paymentData);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Record payment (mark as paid)
   * @param {number} paymentId - Payment ID
   * @param {number} amount - Amount paid
   * @param {string} paymentMethod - Payment method
   * @param {string} transactionReference - Transaction reference
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  recordPayment: async (paymentId, amount, paymentMethod, transactionReference) => {
    const result = await apiCall('POST', `/payments/${paymentId}/record`, {
      paymentId,
      amount,
      paymentMethod,
      transactionReference,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Update payment
   * @param {number} id - Payment ID
   * @param {object} paymentData - Updated payment data
   * @returns {Promise<{success: boolean, data?: object, error?: string}>}
   */
  updatePayment: async (id, paymentData) => {
    const result = await apiCall('PUT', `/payments/${id}`, {
      id,
      ...paymentData,
    });
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Delete payment
   * @param {number} id - Payment ID
   * @returns {Promise<{success: boolean, error?: string}>}
   */
  deletePayment: async (id) => {
    const result = await apiCall('DELETE', `/payments/${id}`);
    return {
      success: result.success,
      error: result.message,
    };
  },

  /**
   * Initiate STK Push via backend
   * @param {{tenantId:number, leaseId?:number, amount:number, phoneNumber:string, accountReference?:string, notes?:string, rentalPeriod?:string}} payload
   */
  initiateStkPush: async (payload) => {
    const body = {
      TenantId: payload.tenantId,
      LeaseId: payload.leaseId ?? null,
      Amount: payload.amount,
      PhoneNumber: payload.phoneNumber,
      AccountReference: payload.accountReference,
      Notes: payload.notes,
      RentalPeriod: payload.rentalPeriod,
    };

    const result = await apiCall('POST', '/webhooks/stkpush/initiate', body);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Request TransactionStatus query (admin)
   * @param {string} transactionId
   */
  queryTransactionStatus: async (transactionId) => {
    if (!transactionId) return { success: false, error: 'transactionId is required' };
    const result = await apiCall('POST', `/webhooks/transactionstatus/query?transactionId=${encodeURIComponent(transactionId)}`);
    return {
      success: result.success,
      data: result.data,
      error: result.message,
    };
  },

  /**
   * Get transaction status logs for a payment
   * @param {number} paymentId
   */
  getTransactionStatuses: async (paymentId) => {
    const result = await apiCall('GET', `/payments/${paymentId}/transactionstatus`);
    return {
      success: result.success,
      data: result.data || [],
      error: result.message,
    };
  },
};

export default paymentService;
