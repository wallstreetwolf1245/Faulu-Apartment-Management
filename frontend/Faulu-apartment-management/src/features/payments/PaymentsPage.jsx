import { useState, useEffect, useRef } from 'react';
import PaymentsList from '../../Landlord/PaymentsList';
import paymentsApi, { listReversals, submitReversal, listMpesaTransactions, formatKES } from './api';
import buildingService from '../../services/buildingService';
import authService from '../../services/authService';
import api from '../../services/api';
import './payments.css';

function SummaryCard({ label, value, sub }) {
  return (
    <div className="summary-card">
      <div className="summary-label">{label}</div>
      <div className="summary-value">{value}</div>
      {sub && <div className="summary-sub">{sub}</div>}
    </div>
  );
}

function filterToBuilding(items, selectedBuilding, units, tenants = []) {
  if (!selectedBuilding || !Array.isArray(items) || items.length === 0) return items;
  const allowedUnitIds = new Set((units || []).map(u => String(u.id)));
  const allowedTenantIds = new Set((tenants || []).map(t => String(t.id)));
  let hadDeterminable = false;

  const filtered = items.filter(item => {
    const itemBuildingId = item?.buildingId ?? item?.propertyId;
    if (itemBuildingId !== undefined && itemBuildingId !== null) {
      hadDeterminable = true;
      return String(itemBuildingId) === String(selectedBuilding);
    }

    if (item?.unitId !== undefined && item?.unitId !== null) {
      hadDeterminable = true;
      return allowedUnitIds.has(String(item.unitId));
    }

    if (item?.tenantId !== undefined && item?.tenantId !== null) {
      hadDeterminable = true;
      return allowedTenantIds.has(String(item.tenantId));
    }

    return false;
  });

  return hadDeterminable ? filtered : items;
}

function getBuildingOwnerId(building) {
  return building?.ownerId ?? building?.userId ?? building?.createdById ?? building?.createdBy ?? building?.landlordId ?? building?.landlordUserId ?? null;
}

function ReversalModal({ open, onClose, tenant, unit, amount, onSubmit }) {
  const [reason, setReason] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => { if (!open) setReason(''); setError(null); }, [open]);

  if (!open) return null;
  return (
    <div className="modal-overlay">
      <div className="modal-panel">
        <div className="modal-header">Reverse payment — {tenant} • {unit}</div>
        <div className="modal-body">
          <div className="reverse-amount">{formatKES(amount)}</div>
          <textarea value={reason} onChange={(e)=>setReason(e.target.value)} maxLength={500} placeholder="Reason for reversal (required)" />
          {error && <div className="error-text">{error}</div>}
        </div>
        <div className="modal-actions">
          <button onClick={onClose} className="btn">Cancel</button>
          <button className="btn btn-danger" disabled={!reason.trim() || submitting} onClick={async ()=>{
            setSubmitting(true); setError(null);
            try {
              await onSubmit({ reason: reason.trim() });
              onClose();
            } catch (err) {
              setError(err.response?.data?.message || err.message || 'Failed');
            } finally { setSubmitting(false); }
          }}>{submitting ? 'Submitting…' : 'Submit'}</button>
        </div>
      </div>
    </div>
  );
}

// Small shared toast helper — same pattern already used by
// handleSubmitReversal, now reused for update/delete feedback too.
function showToast(message, isError = false) {
  const el = document.createElement('div');
  el.className = isError ? 'fp-toast fp-toast-error' : 'fp-toast';
  el.innerText = message;
  document.body.appendChild(el);
  setTimeout(() => el.remove(), isError ? 5000 : 3000);
}

export default function PaymentsPage({ buildingId, onBack }) {
  const [payments, setPayments] = useState([]);
  const [tenants, setTenants] = useState([]);
  const [units, setUnits] = useState([]);
  const [properties, setProperties] = useState([]);
  const [selectedBuilding, setSelectedBuilding] = useState(buildingId ?? null);
  const [activeTab, setActiveTab] = useState('payments');
  const [reversals, setReversals] = useState([]);
  const [mpesaTransactions, setMpesaTransactions] = useState([]);
  const [mpesaLoading, setMpesaLoading] = useState(false);
  const [mpesaError, setMpesaError] = useState(null);
  const [modalState, setModalState] = useState({ open:false, payment:null });

  // Only sync FROM the parent's buildingId prop when that prop itself
  // actually changes (e.g. App.jsx navigates here for a specific building).
  // Comparing against `selectedBuilding` on every render (the old version)
  // meant picking a building from OUR OWN dropdown below would immediately
  // get reverted back to the stale prop — this ref-based check avoids that.
  const prevBuildingIdRef = useRef(buildingId);
  useEffect(() => {
    if (buildingId !== prevBuildingIdRef.current) {
      prevBuildingIdRef.current = buildingId;
      if (buildingId !== undefined && buildingId !== null) {
        setSelectedBuilding(buildingId);
      }
    }
  }, [buildingId]);

  const refreshAll = async (bId = selectedBuilding) => {
    if (bId === null || bId === undefined) {
      setPayments([]);
      setReversals([]);
      setMpesaTransactions([]);
      return;
    }

    try {
      const p = await paymentsApi.listPayments(bId);
      setPayments(p?.data ?? p ?? []);
    } catch (err) { console.error('payments fetch', err); }
    try {
      const r = await listReversals(bId);
      setReversals(r?.data ?? r ?? []);
    } catch (err) { console.error('reversals fetch', err); }
  };

  const refreshMpesaTransactions = async (bId = selectedBuilding) => {
    if (bId === null || bId === undefined) {
      setMpesaTransactions([]);
      setMpesaError(null);
      setMpesaLoading(false);
      return;
    }

    setMpesaLoading(true);
    setMpesaError(null);
    try {
      const result = await listMpesaTransactions(bId);
      setMpesaTransactions(result?.data ?? result ?? []);
    } catch (err) {
      console.error('mpesa transactions fetch', err);
      setMpesaError(err?.response?.data?.message || err.message || 'Failed to load M-Pesa transactions');
      setMpesaTransactions([]);
    } finally {
      setMpesaLoading(false);
    }
  };

  useEffect(() => { refreshAll(); }, [selectedBuilding]);

  useEffect(() => {
    if (activeTab === 'mpesa') {
      refreshMpesaTransactions();
    }
  }, [selectedBuilding, activeTab]);

  useEffect(() => {
    if (buildingId !== undefined && buildingId !== null) {
      refreshAll(buildingId);
    }
  }, [buildingId]);

  // fetch buildings for selector
  useEffect(() => {
  let mounted = true;
  (async () => {
    try {
      const res = await buildingService.getAllBuildings();
      let list = res.success ? (res.data || []) : [];
      const currentUser = authService.getCurrentUser();
      const hasOwnerIds = list.some(b => getBuildingOwnerId(b) !== null);
      if (currentUser?.id && hasOwnerIds) {
        list = list.filter(b => String(getBuildingOwnerId(b)) === String(currentUser.id));
      }
      if (!mounted) return;
      setProperties(list);
      // if no building selected, default to first available
      if ((selectedBuilding === null || selectedBuilding === undefined) && list.length > 0) {
        setSelectedBuilding(list[0].id);
      }
    } catch (err) { console.error('fetch buildings', err); }
  })();
  return () => { mounted = false };
  }, []);

  const handleRequestReverse = (payment) => {
    setModalState({ open:true, payment });
  };

  const handleSubmitReversal = async ({ reason }) => {
    if (!modalState.payment) throw new Error('No payment');
    const payload = { amount: modalState.payment.paidAmount ?? modalState.payment.amount };
    try {
      const res = await submitReversal(modalState.payment.id, { amount: payload.amount, reason });
      await refreshAll();
      setActiveTab('reversals');
      showToast('Reversal submitted');
      return res;
    } catch (err) {
      console.error('submit reversal', err);
      const message = err?.response?.data?.message || err.message || 'Failed to submit reversal';
      showToast(message, true);
      throw err;
    }
  };

  // Update a payment (rent due, dates, method, status, late fees, notes) and
  // refresh the list so the table reflects the saved change.
  const handleUpdatePayment = async (updatedPayment) => {
    try {
      await paymentsApi.updatePayment(updatedPayment.id, updatedPayment);
      await refreshAll();
      showToast('Payment updated');
    } catch (err) {
      console.error('update payment', err);
      const message = err?.response?.data?.message || err.message || 'Failed to update payment';
      showToast(message, true);
    }
  };

  // Delete a payment record and refresh so it disappears from the list.
  const handleDeletePayment = async (id) => {
    try {
      await paymentsApi.deletePayment(id);
      await refreshAll();
      showToast('Payment deleted');
    } catch (err) {
      console.error('delete payment', err);
      const message = err?.response?.data?.message || err.message || 'Failed to delete payment';
      showToast(message, true);
    }
  };

  useEffect(() => {
    if (!selectedBuilding) {
      setTenants([]);
      setUnits([]);
      return;
    }

    let mounted = true;

    (async () => {
      try {
        const unitsRes = await api.get(`/units/building/${selectedBuilding}`);
        if (mounted) setUnits(unitsRes.data?.data ?? unitsRes.data ?? []);
      } catch (err) {
        console.error('fetch units for building', err);
        if (mounted) setUnits([]);
      }

      try {
        const tenantsRes = await api.get(`/tenants?buildingId=${selectedBuilding}`);
        if (mounted) setTenants(tenantsRes.data?.data ?? tenantsRes.data ?? []);
      } catch (err) {
        console.error('fetch tenants for building failed, falling back to all tenants', err);
        try {
          const all = await api.get('/tenants');
          const list = all.data?.data ?? all.data ?? [];
          if (mounted) setTenants(list);
        } catch (err2) {
          console.error('fetch all tenants failed', err2);
          if (mounted) setTenants([]);
        }
      }
    })();

    return () => { mounted = false; };
  }, [selectedBuilding]);

  const visibleTenants = filterToBuilding(tenants, selectedBuilding, units);
  const visiblePayments = filterToBuilding(payments, selectedBuilding, units, visibleTenants);
  const visibleReversals = filterToBuilding(reversals, selectedBuilding, units, visibleTenants);
  const visibleMpesaTransactions = filterToBuilding(mpesaTransactions, selectedBuilding, units, visibleTenants);

  return (
    <div className="payments-page">
      <header className="payments-header-top">
        <div className="payments-header-left">
          <div className="building-selector-row">
            <select className="building-select" value={selectedBuilding ?? ''} onChange={e=> setSelectedBuilding(e.target.value ? Number(e.target.value) : null)}>
              <option value=''>Select building…</option>
              {properties.map(p => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
            <div className="building-heading">
              <h2 className="building-title">{(properties.find(p=>p.id===selectedBuilding)?.name) ?? 'Select a building'}</h2>
                <div className="building-meta">
                {(() => {
                  const b = properties.find(p => p.id === selectedBuilding);
                  if (!b) return 'No address on file';
                  return [b.address, b.city].filter(Boolean).join(', ') || 'No address on file';
                })()} · {units.length} units · {visibleTenants.length} tenants
              </div>            </div>
          </div>
        </div>
        <div className="period-avatar">
          <div className="period-label">CURRENT PERIOD</div>
          <div className="period-value">{new Date().toLocaleString('en-GB', { month:'long', year:'numeric' })}</div>
        </div>
      </header>

      <section className="summary-grid">
        <SummaryCard label="Total Expected" value={formatKES(visiblePayments.reduce((s,p)=>s+Number(p.amount||0),0))} sub={`${visibleTenants.length} active tenants`} />
        <SummaryCard label="Collected" value={formatKES(visiblePayments.reduce((s,p)=>s + (Number(p.paidAmount)||0),0))} sub={`${Math.round((visiblePayments.reduce((s,p)=>s + (Number(p.paidAmount)||0),0) / Math.max(1,visiblePayments.reduce((s,p)=>s + (Number(p.amount)||0),0)))*100)}% collection rate`} />
        <SummaryCard label="Outstanding" value={formatKES(visiblePayments.reduce((s,p)=>s + (Number(p.amount)||0) - (Number(p.paidAmount)||0),0))} sub={`${visiblePayments.filter(p=>{ const due = Number(p.amount||0)-(Number(p.paidAmount)||0); return due>0 && new Date(p.dueDate) < new Date() }).length} overdue units`} />
        <SummaryCard label="MRI Tax Due" value={formatKES(visiblePayments.reduce((s,p)=>s + (Number(p.paidAmount)||0),0) * 0.10)} sub="10% gross · File by 20th" />
      </section>

      <div className="tabs-panel">
        <div className="tabs-bar">
          <button className={activeTab==='payments'? 'tab active' : 'tab'} onClick={()=>setActiveTab('payments')}>💳 Payments</button>
          <button className={activeTab==='history'? 'tab active' : 'tab'} onClick={()=>setActiveTab('history')}>📊 History</button>
          <button className={activeTab==='mpesa'? 'tab active' : 'tab'} onClick={()=>setActiveTab('mpesa')}>🧾 Paid via M-Pesa</button>
          <button className={activeTab==='tax'? 'tab active' : 'tab'} onClick={()=>setActiveTab('tax')}>🇰🇪 Tax Remittance</button>
          <button className={activeTab==='reversals'? 'tab active' : 'tab'} onClick={()=>setActiveTab('reversals')}>↩ Reversals {reversals.length>0 && <span className="badge">{reversals.length}</span>}</button>
        </div>

        <div className="tab-content">
          {activeTab === 'payments' && (
            <PaymentsList
              payments={visiblePayments}
              tenants={visibleTenants}
              units={units}
              properties={properties}
              onUpdatePayment={handleUpdatePayment}
              onDeletePayment={handleDeletePayment}
              onMatchPayment={async (paymentId, unitId)=>{
                await paymentsApi.matchPayment(paymentId, unitId);
                await refreshAll();
              }}
              onNavigate={()=>{}}
              onRequestReverse={(payment)=>handleRequestReverse(payment)}
            />
          )}

          {activeTab === 'history' && (
            <HistoryTab payments={payments} />
          )}

          {activeTab === 'mpesa' && (
            <MpesaTab
              transactions={visibleMpesaTransactions}
              loading={mpesaLoading}
              error={mpesaError}
            />
          )}

          {activeTab === 'tax' && (
            <TaxTab payments={payments} />
          )}

          {activeTab === 'reversals' && (
            <div className="reversals-list">
              {visibleReversals.length === 0 ? (
                <div className="empty-state">No reversals</div>
              ) : (
                visibleReversals.map(r => (
                  <div key={r.id} className="reversal-card">
                    <div className="reversal-left">
                      <div className="tenant-name">{r.tenantName ?? r.payerName}</div>
                      <div className="reversal-reason">{r.reason}</div>
                    </div>
                    <div className="reversal-right">
                      <div className="reversal-amount">{formatKES(r.amount)}</div>
                      <div className={`reversal-status status-${(r.status||'pending').toLowerCase()}`}>{r.status}</div>
                    </div>
                  </div>
                ))
              )}
            </div>
          )}
        </div>
      </div>

      <ReversalModal
        open={modalState.open}
        onClose={() => setModalState({ open:false, payment:null })}
        tenant={modalState.payment?.payerName || modalState.payment?.tenantName}
        unit={modalState.payment?.unitNumber}
        amount={modalState.payment?.paidAmount ?? modalState.payment?.amount}
        onSubmit={handleSubmitReversal}
      />
    </div>
  );
}

function MpesaTab({ transactions = [], loading, error }) {
  const rows = transactions || [];
  const totalCollected = rows.reduce((sum, tx) => sum + Number(tx.amount ?? tx.paidAmount ?? tx.paid ?? 0), 0);
  const formatDate = (tx) => {
    const value = tx.transactionTime || tx.date || tx.createdAt || tx.paidDate || tx.timestamp;
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? 'Unknown' : date.toLocaleString('en-GB', { dateStyle:'medium', timeStyle:'short' });
  };

  if (loading) {
    return <div className="empty-state">Loading M-Pesa transactions…</div>;
  }

  if (error) {
    return <div className="empty-state">{error}</div>;
  }

  if (rows.length === 0) {
    return <div className="empty-state">No M-Pesa C2B transactions found for this building.</div>;
  }

  return (
    <div>
      <div className="history-row" style={{ marginBottom: '1.5rem' }}>
        <div className="history-bucket">
          <div className="history-period">Total collected</div>
          <div className="history-collected">{formatKES(totalCollected)}</div>
        </div>
        <div className="history-bucket">
          <div className="history-period">Transactions</div>
          <div className="history-expected">{rows.length}</div>
        </div>
      </div>
      <div className="table-responsive">
        <table className="payments-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Amount</th>
              <th>Payer</th>
              <th>Phone</th>
              <th>Receipt/Ref</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((tx) => (
              <tr key={tx.id ?? tx.transactionId ?? `${tx.msisdn}-${tx.transactionTime || tx.timestamp || tx.date}`}> 
                <td>{formatDate(tx)}</td>
                <td>{formatKES(tx.amount ?? tx.paidAmount ?? tx.paid ?? 0)}</td>
                <td>{tx.payerName ?? tx.customerName ?? tx.name ?? 'Unknown'}</td>
                <td>{tx.msisdn ?? tx.phoneNumber ?? tx.phone ?? '—'}</td>
                <td>{tx.transactionId ?? tx.reference ?? tx.receiptNumber ?? tx.mpesaReceiptNumber ?? '—'}</td>
                <td>{tx.status ?? tx.transactionStatus ?? 'Unknown'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function HistoryTab({ payments = [] }) {
  // Simple client-side aggregation by month
  const byMonth = {};
  (payments || []).forEach(p => {
    const d = new Date(p.paidDate || p.dueDate || p.createdAt || Date.now());
    const key = `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}`;
    if (!byMonth[key]) byMonth[key] = { expected:0, collected:0, tenantsPaid:0, tenantsTotal:0 };
    byMonth[key].expected += Number(p.amount) || 0;
    const paid = Number(p.paidAmount ?? p.amount) || 0;
    if (paid > 0) { byMonth[key].collected += paid; byMonth[key].tenantsPaid += 1; }
    byMonth[key].tenantsTotal += 1;
  });
  const rows = Object.keys(byMonth).sort().map(k => ({ period:k, ...byMonth[k] }));
  return (
    <div>
      <div className="history-row">{rows.map(r => (
        <div key={r.period} className="history-bucket">
          <div className="history-period">{r.period}</div>
          <div className="history-expected">{formatKES(r.expected)}</div>
          <div className="history-collected">{formatKES(r.collected)}</div>
        </div>
      ))}</div>
    </div>
  );
}

function TaxTab({ payments = [] }) {
  const monthlyGross = (payments || []).reduce((s,p)=> s + (Number(p.paidAmount) || Number(p.amount) || 0), 0);
  const mri = monthlyGross * 0.10;
  return (
    <div>
      <div className="tax-info">
        <strong>Monthly Rental Income (MRI) Tax — Kenya Revenue Authority</strong>
        <p>10% of gross collected. File by 20th via iTax.</p>
      </div>
      <div className="tax-grid">
        <div className="tax-card">Gross Collected<br/>{formatKES(monthlyGross)}</div>
        <div className="tax-highlight">MRI Tax Due<br/>{formatKES(mri)}</div>
        <div className="tax-card">Annual Gross Estimate<br/>{formatKES(monthlyGross*12)}</div>
        <div className="tax-card">Annual MRI Estimate<br/>{formatKES(mri*12)}</div>
      </div>
    </div>
  );
}