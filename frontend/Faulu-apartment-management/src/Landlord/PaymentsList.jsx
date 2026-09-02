import React, { useState, useEffect, useRef } from 'react'
import { animate } from 'animejs';
import './PaymentsList.css'
import { formatKES } from '../features/payments/api';

// ── Helpers ────────────────────────────────────────────────────────────────
const formatDate = (d) => {
    if (!d) return 'N/A';
    const dt = new Date(d);
    return isNaN(dt) ? 'N/A' : dt.toLocaleDateString();
};

const formatDueDate = (d) => {
    if (!d) return 'N/A';
    const dt = new Date(d);
    if (isNaN(dt)) return 'N/A';
    return `Due ${String(dt.getDate()).padStart(2, '0')}/${String(dt.getMonth() + 1).padStart(2, '0')}`;
};

// Maps the richer real-world status set down to the 4 visual buckets the
// filter pills use, while still displaying the real status label + dot.
const STATUS_META = {
    paid:      { bucket: 'paid',     label: 'Paid' },
    completed: { bucket: 'paid',     label: 'Completed' },
    pending:   { bucket: 'pending',  label: 'Pending' },
    overdue:   { bucket: 'overdue',  label: 'Overdue' },
    partial:   { bucket: 'partial',  label: 'Partial' },
    unmatched: { bucket: 'unmatched',label: 'Unmatched' },
    cancelled: { bucket: 'cancelled',label: 'Cancelled' },
};

const getStatusMeta = (status) => {
    const key = String(status ?? '').toLowerCase();
    return STATUS_META[key] ?? { bucket: 'pending', label: status || 'Unknown' };
};

// ── Main component ─────────────────────────────────────────────────────────
// PaymentsList is purely presentational, same as TenantsList: it does NOT
// fetch its own data. PaymentsPage.jsx fetches payments/tenants/units and
// passes them down as props; this component just displays, searches, sorts,
// and forwards actions (update/delete/match/reverse) back up via callbacks.
function PaymentsList({
    payments      = [],
    tenants       = [],
    units         = [],
    onUpdatePayment,
    onDeletePayment,
    onMatchPayment,
    onRequestReverse,
}) {
    // ── State ────────────────────────────────────────────────────────────
    const [searchTerm,      setSearchTerm]      = useState("");
    const [filterStatus,    setFilterStatus]    = useState("all");
    const [activeTab,       setActiveTab]       = useState("all");   // "all" | "unmatched"
    const [editingId,       setEditingId]       = useState(null);
    const [selectedPayment, setSelectedPayment] = useState(null);
    const [matchingId,      setMatchingId]      = useState(null);

    const useDebounced = (value, delay = 200) => {
        const [v, setV] = useState(value);
        useEffect(() => {
            const id = setTimeout(() => setV(value), delay);
            return () => clearTimeout(id);
        }, [value, delay]);
        return v;
    };
    const debouncedSearch = useDebounced(searchTerm, 200);

    // ── Name resolution ────────────────────────────────────────────────────
    const getTenant = (tenantId) => tenants.find(t => String(t.id) === String(tenantId));
    const getTenantName = (tenantId) => {
        const t = getTenant(tenantId);
        if (!t) return tenantId ? `Tenant #${tenantId}` : 'Unknown';
        return `${t.firstName ?? ''} ${t.lastName ?? ''}`.trim() || `Tenant #${tenantId}`;
    };
    const getTenantPhone = (tenantId) => getTenant(tenantId)?.phone ?? getTenant(tenantId)?.phoneNumber ?? null;

    const getUnit = (tenantId, unitId) => {
        const t = getTenant(tenantId);
        return unitId
            ? units.find(u => String(u.id) === String(unitId))
            : t ? units.find(u => String(u.id) === String(t.unitId)) : null;
    };

    // ── Event handlers ─────────────────────────────────────────────────────
    const handleDelete = (id) => {
        if (window.confirm("Are you sure you want to delete this payment record?"))
            onDeletePayment(id);
    };

    // Modal animation refs. We intentionally do NOT track the clicked
    // button's position anymore — the overlay's flex centering already
    // places the modal correctly, and animating a translate offset from
    // the source element was fragile: if a second modal opened before the
    // first finished animating (or a re-render interrupted mid-flight),
    // the inline transform could get stuck at a non-zero offset, visually
    // shoving the modal away from center and clipping it. Scale + fade
    // from the modal's own (always-centered) position has no such failure
    // mode, since there's never a translate value to get stuck at.
    const [isAnimating, setIsAnimating] = useState(false);
    const modalOverlayRef = useRef(null);
    const modalContentRef = useRef(null);

    const handleEdit = (item) => {
        setEditingId(item.id);
        setSelectedPayment(item);
    };

    const handleView = (item) => {
        setSelectedPayment(item);
    };

    const handleCloseModal = () => {
        const content = modalContentRef.current;
        setIsAnimating(true);
        if (content) {
            animate(content, {
                scale: 0.92,
                opacity: 0,
                duration: 220,
                easing: 'cubicBezier(.2,.9,.2,1)',
                onComplete: () => {
                    setSelectedPayment(null);
                    setEditingId(null);
                    setIsAnimating(false);
                    // clear inline styles left by anime
                    content.style.transform = '';
                    content.style.opacity = '';
                }
            });
        } else {
            setSelectedPayment(null);
            setEditingId(null);
            setIsAnimating(false);
        }
    };

    const handleSaveEdit = (updated) => { onUpdatePayment(updated); setEditingId(null); setSelectedPayment(null); };

    const handleMatch = async (paymentId, unitId) => {
        setMatchingId(paymentId);
        try {
            await onMatchPayment(paymentId, unitId);
        } finally {
            setMatchingId(null);
        }
    };

    // Reversal is no longer handled here — PaymentsPage already owns a full
    // reversal modal + submit flow. We just hand the payment back up to it.
    const handleReverse = (item) => {
        onRequestReverse?.(item);
    };

    // ── Derived data ───────────────────────────────────────────────────────
    const unmatchedPayments = (payments || []).filter(p => getStatusMeta(p.status).bucket === 'unmatched');

    const bucketCounts = {
        all:     payments.length,
        paid:    payments.filter(p => getStatusMeta(p.status).bucket === 'paid').length,
        pending: payments.filter(p => getStatusMeta(p.status).bucket === 'pending').length,
        overdue: payments.filter(p => getStatusMeta(p.status).bucket === 'overdue').length,
        partial: payments.filter(p => getStatusMeta(p.status).bucket === 'partial').length,
    };

    const filteredPayments = (payments || []).filter(item => {
        const term    = (debouncedSearch || '').toLowerCase();
        const unit    = getUnit(item.tenantId, item.unitId);
        const name    = getTenantName(item.tenantId).toLowerCase();
        const unitNum = String(unit?.unitNumber ?? '').toLowerCase();
        const matchesSearch = !term || name.includes(term) || unitNum.includes(term);
        const matchesStatus = filterStatus === 'all' || getStatusMeta(item.status).bucket === filterStatus;
        return matchesSearch && matchesStatus;
    });

    const sortedPayments = [...filteredPayments].sort(
        (a, b) => new Date(b.paidDate || b.dueDate || 0) - new Date(a.paidDate || a.dueDate || 0)
    );

    // ── Animated modal open effect ─────────────────────────────────────────
    // Scale + fade in from the modal's own natural (centered) position.
    // No getBoundingClientRect() math, no translate offsets — so there is
    // no state that can get "stuck" mid-animation and leave the modal
    // visually displaced.
    useEffect(() => {
        const content = modalContentRef.current;
        if (selectedPayment && content) {
            content.style.transform = 'scale(0.92)';
            content.style.opacity = '0';

            animate(content, {
                scale: 1,
                opacity: 1,
                duration: 260,
                easing: 'cubicBezier(.2,.9,.2,1)'
            });
        }
    }, [selectedPayment]);

    // ── Render ─────────────────────────────────────────────────────────────
    return (
        <div className="payments-panel">

            {/* Filter row */}
            <div className="filters-row">
                <input
                    type="text"
                    placeholder="Search by name or unit number…"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="search-input"
                />
                <div className="pills-row">
                    <button
                        className={`pill pill-all ${filterStatus === 'all' ? 'active' : ''}`}
                        onClick={() => setFilterStatus('all')}
                    >All ({bucketCounts.all})</button>
                    <button
                        className={`pill pill-paid ${filterStatus === 'paid' ? 'active' : ''}`}
                        onClick={() => setFilterStatus('paid')}
                    >Paid ({bucketCounts.paid})</button>
                    <button
                        className={`pill pill-pending ${filterStatus === 'pending' ? 'active' : ''}`}
                        onClick={() => setFilterStatus('pending')}
                    >Pending ({bucketCounts.pending})</button>
                    <button
                        className={`pill pill-overdue ${filterStatus === 'overdue' ? 'active' : ''}`}
                        onClick={() => setFilterStatus('overdue')}
                    >Overdue ({bucketCounts.overdue})</button>
                    <button
                        className={`pill pill-partial ${filterStatus === 'partial' ? 'active' : ''}`}
                        onClick={() => setFilterStatus('partial')}
                    >Partial ({bucketCounts.partial})</button>
                </div>
            </div>

            {unmatchedPayments.length > 0 && (
                <div className="unmatched-banner">
                    ⚠️ {unmatchedPayments.length} M-Pesa payment{unmatchedPayments.length > 1 ? 's' : ''} couldn't
                    be matched automatically.{' '}
                    <button className="banner-link" onClick={() => setActiveTab('unmatched')}>
                        Reconcile now →
                    </button>
                </div>
            )}

            {activeTab === 'unmatched' ? (
                <div className="unmatched-section">
                    <button className="back-link" onClick={() => setActiveTab('all')}>← Back to payments</button>
                    {unmatchedPayments.length === 0 ? (
                        <div className="empty-state"><p>No unmatched payments — you're all caught up.</p></div>
                    ) : (
                        <div className="unmatched-cards">
                            {unmatchedPayments.map(payment => (
                                <UnmatchedPaymentCard
                                    key={payment.id}
                                    payment={payment}
                                    units={units}
                                    tenants={tenants}
                                    isMatching={matchingId === payment.id}
                                    onMatch={handleMatch}
                                />
                            ))}
                        </div>
                    )}
                </div>
            ) : sortedPayments.length === 0 ? (
                <div className="empty-state"><p>No tenants match this filter.</p></div>
            ) : (
                <div className="table-responsive">
                    <table className="payments-table">
                        <thead>
                            <tr>
                                <th>Tenant</th>
                                <th>Unit</th>
                                <th className="num">Rent Due</th>
                                <th className="num">Paid</th>
                                <th>Method</th>
                                <th>Date</th>
                                <th>Status</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedPayments.map(item => {
                                const unit = getUnit(item.tenantId, item.unitId);
                                const meta = getStatusMeta(item.status);
                                const canReverse = meta.bucket === 'paid' || meta.bucket === 'partial';
                                const paidAmount = Number(item.paidAmount ?? (meta.bucket === 'paid' ? item.amount : 0)) || 0;
                                const isOverdue = meta.bucket === 'overdue';

                                return (
                                    <tr key={item.id} className="payment-row">
                                        <td>
                                            <div className="tenant-name">{item.payerName || getTenantName(item.tenantId)}</div>
                                            {getTenantPhone(item.tenantId) && (
                                                <div className="tenant-phone">{getTenantPhone(item.tenantId)}</div>
                                            )}
                                        </td>
                                        <td><span className="unit-pill">{unit?.unitNumber ?? 'N/A'}</span></td>
                                        <td className="num mono">{formatKES(item.amount)}</td>
                                        <td className="num mono">
                                            {paidAmount > 0 ? <span className="paid-amount">{formatKES(paidAmount)}</span> : <span className="dash">—</span>}
                                        </td>
                                        <td>
                                            {item.paymentMethod
                                                ? <span className="method-other">{item.paymentMethod.replace(/_/g, ' ')}</span>
                                                : <span className="dash">—</span>}
                                        </td>
                                        <td className="mono">
                                            {isOverdue
                                                ? <span className="due-overdue">{formatDueDate(item.dueDate)}</span>
                                                : formatDate(item.paidDate)}
                                        </td>
                                        <td>
                                            <span className={`status-badge status-${meta.bucket}`}>
                                                <span className="status-dot" /> {meta.label}
                                            </span>
                                        </td>
                                        <td className="actions-cell">
                                            <button className="btn-link" onClick={() => handleView(item)}>View</button>
                                            {meta.bucket !== 'unmatched' && (
                                            <button className="btn-link" onClick={() => handleEdit(item)}>Edit</button>
                                            )}
                                            <button className="btn-delete" onClick={() => handleDelete(item.id)}>Delete</button>
                                            {canReverse && (
                                                <button className="btn-reverse" onClick={() => handleReverse(item)}>
                                                    Reverse
                                                </button>
                                            )}
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            )}

            {/* View / Edit Modal */}
            {selectedPayment && (
                <div className="modal-overlay" ref={modalOverlayRef} onClick={handleCloseModal}>
                    <div className="modal-content" ref={modalContentRef} onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2>{editingId ? "Edit Payment" : "Payment Details"}</h2>
                            <button className="close-btn" onClick={handleCloseModal}>×</button>
                        </div>
                        <div className="modal-body">
                            {editingId ? (
                                <EditPaymentForm payment={selectedPayment} onSave={handleSaveEdit} onCancel={handleCloseModal} />
                            ) : (
                                <PaymentDetails
                                    payment={selectedPayment}
                                    tenantName={getTenantName(selectedPayment.tenantId)}
                                    unit={getUnit(selectedPayment.tenantId, selectedPayment.unitId)}
                                />
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

// ── Unmatched Payment Card ─────────────────────────────────────────────────
function UnmatchedPaymentCard({ payment, units, tenants, isMatching, onMatch }) {
    const [selectedUnitId, setSelectedUnitId] = useState('');
    const [error, setError] = useState('');

    const handleConfirm = async () => {
        if (!selectedUnitId) { setError('Please select a unit first.'); return; }
        setError('');
        await onMatch(payment.id, Number(selectedUnitId));
    };

    const unitOptions = units.map(u => {
        const tenant = tenants.find(t => String(t.unitId) === String(u.id));
        const label = tenant ? `${u.unitNumber} — ${tenant.firstName} ${tenant.lastName}` : `${u.unitNumber} — Vacant`;
        return { value: u.id, label };
    });

    return (
        <div className="unmatched-card">
            <div className="unmatched-card-header">
                <div>
                    <span className="receipt-code">{payment.mpesaReceiptNumber ?? '—'}</span>
                    <p className="unmatched-amount">{formatKES(payment.amount)}</p>
                </div>
                <span className="status-badge status-unmatched"><span className="status-dot" /> Unmatched</span>
            </div>
            <div className="unmatched-card-meta">
                <div>
                    <p className="meta-label">Phone</p>
                    <p className="meta-value">{payment.payerPhone ?? '—'}</p>
                </div>
                <div>
                    <p className="meta-label">Account ref entered</p>
                    <p className="meta-value ref-highlight">"{payment.billRefNumber ?? '—'}"</p>
                </div>
                <div>
                    <p className="meta-label">Received</p>
                    <p className="meta-value">
                        {payment.paidDate ? new Date(payment.paidDate).toLocaleString() : new Date(payment.createdAt).toLocaleString()}
                    </p>
                </div>
            </div>
            <div className="unmatched-card-actions">
                <select value={selectedUnitId} onChange={(e) => { setSelectedUnitId(e.target.value); setError(''); }}
                        className="unit-select" disabled={isMatching}>
                    <option value="">Assign to unit…</option>
                    {unitOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                </select>
                <button className="btn-confirm-match" onClick={handleConfirm} disabled={isMatching || !selectedUnitId}>
                    {isMatching ? 'Matching…' : 'Confirm match'}
                </button>
            </div>
            {error && <p className="error-text">{error}</p>}
        </div>
    );
}

// ── Payment Details modal ──────────────────────────────────────────────────
function PaymentDetails({ payment, tenantName, unit }) {
    return (
        <div className="payment-details">
            <div className="detail-row"><span className="detail-label">Tenant:</span><span className="detail-value">{payment.payerName || tenantName}</span></div>
            <div className="detail-row"><span className="detail-label">Unit:</span><span className="detail-value">{unit?.unitNumber ?? 'N/A'}</span></div>
            <div className="detail-row"><span className="detail-label">Payment Type:</span><span className="detail-value">{payment.paymentType ?? 'Rent'}</span></div>
            <div className="detail-row"><span className="detail-label">Amount:</span><span className="detail-value">{formatKES(payment.amount)}</span></div>
            {payment.lateFees > 0 && (
                <div className="detail-row"><span className="detail-label">Late Fees:</span><span className="detail-value">{formatKES(payment.lateFees)}</span></div>
            )}
            <div className="detail-row"><span className="detail-label">Due Date:</span><span className="detail-value">{payment.dueDate ? new Date(payment.dueDate).toLocaleDateString() : 'N/A'}</span></div>
            <div className="detail-row"><span className="detail-label">Paid Date:</span><span className="detail-value">{payment.paidDate ? new Date(payment.paidDate).toLocaleDateString() : 'N/A'}</span></div>
            <div className="detail-row"><span className="detail-label">Payment Method:</span><span className="detail-value">{payment.paymentMethod?.replace(/_/g, ' ') ?? 'N/A'}</span></div>
            {payment.mpesaReceiptNumber && (
                <div className="detail-row"><span className="detail-label">M-Pesa Receipt:</span><span className="detail-value receipt-code">{payment.mpesaReceiptNumber}</span></div>
            )}
            {payment.billRefNumber && (
                <div className="detail-row"><span className="detail-label">Account Ref Used:</span><span className="detail-value">{payment.billRefNumber}</span></div>
            )}
            {payment.payerPhone && (
                <div className="detail-row"><span className="detail-label">Payer Phone:</span><span className="detail-value">{payment.payerPhone}</span></div>
            )}
            {payment.transactionReference && (
                <div className="detail-row"><span className="detail-label">Reference:</span><span className="detail-value">{payment.transactionReference}</span></div>
            )}
            <div className="detail-row">
                <span className="detail-label">Status:</span>
                <span className={`detail-value status-${String(payment.status ?? '').toLowerCase()}`}>{payment.status}</span>
            </div>
            {payment.notes && (
                <div className="detail-row"><span className="detail-label">Notes:</span><span className="detail-value">{payment.notes}</span></div>
            )}
        </div>
    );
}

// ── Edit Payment Form ──────────────────────────────────────────────────────
function EditPaymentForm({ payment, onSave, onCancel }) {
    const [formData, setFormData] = useState({
        ...payment,
        dueDate:  payment.dueDate  ? payment.dueDate.substring(0, 10)  : '',
        paidDate: payment.paidDate ? payment.paidDate.substring(0, 10) : '',
    });
    const [errors, setErrors] = useState({});

    const paymentMethods = ["bank_transfer", "cash", "check", "credit_card", "debit_card", "mobile_payment"];

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: name === 'amount' || name === 'lateFees' ? parseFloat(value) || 0 : value }));
    };

    const validateForm = () => {
        const newErrors = {};
        if (!formData.amount || formData.amount <= 0) newErrors.amount = "Amount must be a positive number";
        if (!formData.dueDate) newErrors.dueDate = "Due date is required";
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validateForm()) onSave({ ...formData, paidDate: formData.paidDate || null });
    };

    return (
        <form className="edit-form" onSubmit={handleSubmit}>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Amount (KES):</label>
                    <input type="number" name="amount" value={formData.amount} onChange={handleChange}
                           className={errors.amount ? 'input-error' : ''} step="0.01" min="0" />
                    {errors.amount && <span className="error-text">{errors.amount}</span>}
                </div>
                <div className="form-group">
                    <label>Due Date:</label>
                    <input type="date" name="dueDate" value={formData.dueDate} onChange={handleChange}
                           className={errors.dueDate ? 'input-error' : ''} />
                    {errors.dueDate && <span className="error-text">{errors.dueDate}</span>}
                </div>
            </div>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Paid Date:</label>
                    <input type="date" name="paidDate" value={formData.paidDate} onChange={handleChange} />
                </div>
                <div className="form-group">
                    <label>Payment Method:</label>
                    <select name="paymentMethod" value={formData.paymentMethod ?? ''} onChange={handleChange}>
                        <option value="">— select —</option>
                        {paymentMethods.map(m => (
                            <option key={m} value={m}>{m.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase())}</option>
                        ))}
                    </select>
                </div>
            </div>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Status:</label>
                    <select name="status" value={formData.status ?? 'Pending'} onChange={handleChange}>
                        <option value="Pending">Pending</option>
                        <option value="Paid">Paid</option>
                        <option value="Completed">Completed</option>
                        <option value="Overdue">Overdue</option>
                        <option value="Partial">Partial</option>
                        <option value="Cancelled">Cancelled</option>
                    </select>
                </div>
                <div className="form-group">
                    <label>Late Fees (KES):</label>
                    <input type="number" name="lateFees" value={formData.lateFees ?? 0} onChange={handleChange} step="0.01" min="0" />
                </div>
            </div>
            <div className="form-group">
                <label>Notes:</label>
                <textarea name="notes" value={formData.notes ?? ''} onChange={handleChange} />
            </div>
            <div className="form-buttons">
                <button type="submit" className="save-btn">Save Changes</button>
                <button type="button" className="cancel-btn" onClick={onCancel}>Cancel</button>
            </div>
        </form>
    );
}

export default PaymentsList; 