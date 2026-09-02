import { useState } from 'react'
import GlassSuccessModal from '../components/GlassSuccessModal'
import './AddPayment.css'

const PAYMENT_STEPS = ['Tenant', 'Payment Details', 'Review']

function AddPayment({ properties = [], tenants = [], units = [], onPaymentAdded, onNavigate }) {
    const [currentStep, setCurrentStep] = useState(0)
    const [tenantId, setTenantId] = useState("")
    const [leaseId, setLeaseId] = useState("")
    const [propertyId, setPropertyId] = useState("")
    const [paymentAmount, setPaymentAmount] = useState("")
    const [paymentDate, setPaymentDate] = useState("")
    const [paymentMethod, setPaymentMethod] = useState("bank_transfer")
    const [paymentType, setPaymentType] = useState("Rent")
    const [rentalPeriod, setRentalPeriod] = useState("")
    const [payerPhone, setPayerPhone] = useState("")
    const [notes, setNotes] = useState("")
    const [errors, setErrors] = useState({})
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [showSuccessModal, setShowSuccessModal] = useState(false)
    const [showFailureModal, setShowFailureModal] = useState(false)
    const [submittedPayment, setSubmittedPayment] = useState(null)
    const [successTitle, setSuccessTitle] = useState('Payment Recorded')
    const [successMessage, setSuccessMessage] = useState('The payment has been added successfully.')
    const [failureTitle, setFailureTitle] = useState('Payment Failed')
    const [failureMessage, setFailureMessage] = useState('Failed to submit the payment. Please try again.')
    const [failureDetails, setFailureDetails] = useState([])

    const paymentMethods = [
        'bank_transfer',
        'cash',
        'check',
        'credit_card',
        'debit_card',
        'mobile_payment',
    ]

    const paymentTypes = ['Rent', 'Deposit', 'LateFee', 'Maintenance', 'Other']

    const selectedTenant = tenants.find(t => String(t.id) === String(tenantId))
    const selectedUnit = units.find(u => String(u.id) === String(selectedTenant?.unitId))
    const selectedBuilding = selectedUnit?.buildingId ?? selectedUnit?.propertyId
    const selectedProperty = properties.find(p => String(p.id) === String(selectedBuilding))

    const propertyDisplay = selectedProperty
        ? (selectedProperty.name ?? selectedProperty.buildingName ?? selectedProperty.propertyName ?? selectedProperty.address ?? `Property #${selectedProperty.id}`)
        : (selectedTenant?.propertyName ?? 'Unknown property')

    const unitDisplay = selectedUnit?.unitNumber ?? selectedTenant?.unitNumber ?? 'N/A'

    const validateStep = (step) => {
        const newErrors = {}

        if (step === 0 && !tenantId) {
            newErrors.tenantId = 'Tenant is required'
        }

        if (step === 1) {
            if (!paymentAmount || isNaN(paymentAmount) || parseFloat(paymentAmount) <= 0) {
                newErrors.paymentAmount = 'Payment amount must be a valid positive number'
            }
            if (!paymentDate) newErrors.paymentDate = 'Payment date is required'
            if (!rentalPeriod.trim()) newErrors.rentalPeriod = 'Rental period is required'
        }

        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const validateForm = () => {
        const newErrors = {}

        if (!tenantId) newErrors.tenantId = 'Tenant is required'
        if (!paymentAmount || isNaN(paymentAmount) || parseFloat(paymentAmount) <= 0) newErrors.paymentAmount = 'Payment amount must be a valid positive number'
        if (!paymentDate) newErrors.paymentDate = 'Payment date is required'
        if (!rentalPeriod.trim()) newErrors.rentalPeriod = 'Rental period is required'

        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const resetForm = () => {
        setCurrentStep(0)
        setTenantId("")
        setLeaseId("")
        setPropertyId("")
        setPaymentAmount("")
        setPaymentDate("")
        setPaymentMethod("bank_transfer")
        setPaymentType("Rent")
        setPayerPhone("")
        setRentalPeriod("")
        setNotes("")
        setErrors({})
    }

    const goNext = () => {
        if (validateStep(currentStep)) {
            setCurrentStep((step) => Math.min(step + 1, PAYMENT_STEPS.length - 1))
        }
    }

    const goBack = () => {
        setErrors({})
        setCurrentStep((step) => Math.max(step - 1, 0))
    }

    const handleSubmit = async (e) => {
        e.preventDefault()

        const isLastStep = currentStep === PAYMENT_STEPS.length - 1
        if (!isLastStep) {
            goNext()
            return
        }

        if (!validateForm()) return

        try {
            setIsSubmitting(true)

            const newPayment = {
                tenantId: parseInt(tenantId),
                leaseId: leaseId ? parseInt(leaseId) : null,
                amount: parseFloat(paymentAmount),
                dueDate: paymentDate,
                paymentMethod,
                paymentType,
                payerPhone: payerPhone || null,
                notes: notes || null,
                buildingId: selectedBuilding ? Number(selectedBuilding) : undefined,
                propertyId: selectedProperty?.id ? Number(selectedProperty.id) : undefined,
                ...(rentalPeriod && { notes: [notes, `Period: ${rentalPeriod}`].filter(Boolean).join(' | ') }),
            }

            const created = await onPaymentAdded(newPayment, {
                skipNavigate: true,
                propertyId: selectedBuilding ? Number(selectedBuilding) : undefined,
                buildingId: selectedBuilding ? Number(selectedBuilding) : undefined,
            })
            const result = created ? created : newPayment
            setSubmittedPayment({
                ...result,
                tenantName: selectedTenant ? `${selectedTenant.firstName ?? ''} ${selectedTenant.lastName ?? ''}`.trim() : String(tenantId)
            })

            setSuccessTitle('Payment Recorded')
            setSuccessMessage('The payment has been added successfully.')
            setShowSuccessModal(true)
            resetForm()
        } catch (error) {
            console.error('Error submitting payment:', error)

            const errorMessage = error?.response?.data?.message || error?.message || 'Failed to add payment. Please try again.'

            setFailureTitle('Payment Failed')
            setFailureMessage(`Could not save the payment. ${errorMessage}`)
            setFailureDetails([
                ...(error?.response?.data?.errors ? Object.entries(error.response.data.errors).map(([key, value]) => `${key}: ${value}`) : []),
            ])
            setShowFailureModal(true)
            setErrors({ submit: errorMessage })
        } finally {
            setIsSubmitting(false)
        }
    }

    const isLastStep = currentStep === PAYMENT_STEPS.length - 1

    return (
        <>
            <h1>Add Manual Payment</h1>

            {errors.submit && <div className="error-message">{errors.submit}</div>}

            <div className="step-progress">
                {PAYMENT_STEPS.map((label, idx) => (
                    <div
                        key={label}
                        className={`step-segment ${idx <= currentStep ? 'filled' : ''} ${idx === currentStep ? 'active' : ''}`}
                    >
                        <div className="segment-bar" />
                        <span className="segment-label">{label}</span>
                    </div>
                ))}
            </div>

            <form className="form-container" onSubmit={handleSubmit}>
                {currentStep === 0 && (
                    <div className="form-row">
                        /* <div className="form-group">
                            <label>Tenant:</label>
                            <select
                                value={tenantId}
                                onChange={(e) => {
                                    const val = e.target.value
                                    setTenantId(val)
                                    const sel = tenants.find(t => String(t.id) === String(val))
                                    if (sel) {
                                        setLeaseId(sel.activeLeaseId ? String(sel.activeLeaseId) : '')
                                        const unit = units.find(u => String(u.id) === String(sel.unitId))
                                        if (unit) {
                                            setPropertyId(String(unit.buildingId ?? unit.propertyId ?? ''))
                                        } else {
                                            setPropertyId('')
                                        }
                                    } else {
                                        setLeaseId('')
                                        setPropertyId('')
                                    }
                                }}
                                className={errors.tenantId ? 'input-error' : ''}
                            >
                                <option value="">Select tenant</option>
                                {tenants.map(t => (
                                    <option key={t.id} value={t.id}>
                                        {`${t.firstName ?? ''} ${t.lastName ?? ''}`.trim()}
                                        {t.unitNumber ? ` — Unit ${t.unitNumber}` : ''}
                                    </option>
                                ))}
                            </select>
                            {errors.tenantId && <span className="error-text">{errors.tenantId}</span>}
                        </div>

                        <div className="form-group">
                            <label>Property &amp; Unit:</label>
                            {selectedTenant ? (
                                <>
                                    <div className="info-text">{propertyDisplay}</div>
                                    <div className="info-text">Unit: {unitDisplay}</div>
                                </>
                            ) : (
                                <div className="info-text">Select a tenant to view property and unit</div>
                            )}
                        </div>
                    </div>
                )}

                {currentStep === 1 && (
                    <>
                        <div className="form-row">
                            <div className="form-group">
                                <label>Payment Amount (KES):</label>
                                <input
                                    type="number"
                                    value={paymentAmount}
                                    onChange={(e) => setPaymentAmount(e.target.value)}
                                    className={errors.paymentAmount ? 'input-error' : ''}
                                    placeholder="0.00"
                                    step="0.01"
                                    min="0"
                                />
                                {errors.paymentAmount && <span className="error-text">{errors.paymentAmount}</span>}
                            </div>
                            <div className="form-group">
                                <label>Payment Date:</label>
                                <input
                                    type="date"
                                    value={paymentDate}
                                    onChange={(e) => setPaymentDate(e.target.value)}
                                    className={errors.paymentDate ? 'input-error' : ''}
                                />
                                {errors.paymentDate && <span className="error-text">{errors.paymentDate}</span>}
                            </div>
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label>Payment Method:</label>
                                <select value={paymentMethod} onChange={(e) => setPaymentMethod(e.target.value)}>
                                    {paymentMethods.map(method => (
                                        <option key={method} value={method}>
                                            {method.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase())}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <div className="form-group">
                                <label>Payment Type:</label>
                                <select value={paymentType} onChange={(e) => setPaymentType(e.target.value)}>
                                    {paymentTypes.map(type => (
                                        <option key={type} value={type}>{type}</option>
                                    ))}
                                </select>
                            </div>
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label>Rental Period:</label>
                                <input
                                    type="text"
                                    value={rentalPeriod}
                                    onChange={(e) => setRentalPeriod(e.target.value)}
                                    className={errors.rentalPeriod ? 'input-error' : ''}
                                    placeholder="e.g., June 2026"
                                />
                                {errors.rentalPeriod && <span className="error-text">{errors.rentalPeriod}</span>}
                            </div>
                            <div className="form-group">
                                <label>Payer Phone:</label>
                                <input
                                    type="tel"
                                    value={payerPhone}
                                    onChange={(e) => setPayerPhone(e.target.value)}
                                    placeholder="e.g., 0712 345 678"
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label>Notes:</label>
                            <textarea value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Add any additional notes..." />
                        </div>
                    </>
                )}

                {currentStep === 2 && (
                    <div className="review-summary">
                        <p><strong>Tenant:</strong> {selectedTenant ? `${selectedTenant.firstName || ''} ${selectedTenant.lastName || ''}`.trim() : 'Not selected'}</p>
                        <p><strong>Property:</strong> {propertyDisplay}</p>
                        <p><strong>Unit:</strong> {unitDisplay}</p>
                        <p><strong>Amount:</strong> KES {Number(paymentAmount || 0).toLocaleString()}</p>
                        <p><strong>Date:</strong> {paymentDate}</p>
                        <p><strong>Method:</strong> {paymentMethod.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase())}</p>
                        <p><strong>Type:</strong> {paymentType}</p>
                        <p><strong>Period:</strong> {rentalPeriod || 'N/A'}</p>
                        <p><strong>Notes:</strong> {notes || 'None'}</p>
                    </div>
                )}

                <div className="form-buttons">
                    {currentStep > 0 && (
                        <button type="button" className="reset-btn" onClick={goBack}>Back</button>
                    )}
                    <button type="submit" className="submit-btn" disabled={isSubmitting}>
                        {isLastStep ? (isSubmitting ? 'Submitting…' : 'Add Payment') : 'Next'}
                    </button>
                    {isLastStep && (
                        <button type="button" className="reset-btn" onClick={resetForm} disabled={isSubmitting}>Reset</button>
                    )}
                </div>
            </form>

            <GlassSuccessModal
                open={showSuccessModal}
                title={successTitle}
                message={successMessage}
                details={submittedPayment ? [
                    `Tenant: ${submittedPayment.tenantName}`,
                    `Amount: KES ${Number(submittedPayment.amount).toLocaleString()}`,
                    `Type: ${submittedPayment.paymentType}`,
                    ...(submittedPayment.payerPhone ? [`Payer Phone: ${submittedPayment.payerPhone}`] : []),
                ] : []}
                onClose={() => {
                    setShowSuccessModal(false)
                    onNavigate('paymentsList')
                }}
                actionLabel="Confirmed"
            />

            <GlassSuccessModal
                open={showFailureModal}
                title={failureTitle}
                message={failureMessage}
                details={failureDetails}
                onClose={() => setShowFailureModal(false)}
                actionLabel="Close"
                variant="error"
            />
        </>
    )
}

export default AddPayment
