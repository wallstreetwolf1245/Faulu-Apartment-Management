import { useState } from 'react'
import GlassSuccessModal from '../components/GlassSuccessModal'
import './AddMaintenance.css'

const MAINTENANCE_STEPS = ['Tenant', 'Issue Details', 'Review']

function AddMaintenance({ properties = [], tenants = [], units = [], onMaintenanceAdded, onNavigate }){
    const [currentStep, setCurrentStep] = useState(0)
    const [propertyId, setPropertyId] = useState("")
    const [tenantId, setTenantId] = useState("")
    const [title, setTitle] = useState("")
    const [issueType, setIssueType] = useState("")
    const [description, setDescription] = useState("")
    const [priority, setPriority] = useState("Normal")
    const [estimatedCost, setEstimatedCost] = useState("")
    const [assignedVendor, setAssignedVendor] = useState("")
    const [errors, setErrors] = useState({})
    const [showSuccessModal, setShowSuccessModal] = useState(false)
    const [submittedMaintenance, setSubmittedMaintenance] = useState(null)

    const issueTypes = [
        "Plumbing",
        "Electrical",
        "Heating/Cooling",
        "Appliance",
        "Structural",
        "Pest Control",
        "Cleaning",
        "Painting",
        "Carpet/Flooring",
        "Other"
    ]

    const selectedTenant = tenants.find(t => String(t.id) === String(tenantId))
    const selectedProperty = properties.find(p => String(p.id) === String(selectedTenant?.buildingId ?? selectedTenant?.propertyId))

    const validateStep = (step) => {
        const newErrors = {}

        if (step === 0 && !tenantId.trim()) {
            newErrors.tenantId = 'Tenant is required'
        }

        if (step === 1) {
            if (!title.trim()) newErrors.title = 'Title is required'
            if (!issueType) newErrors.issueType = 'Issue type is required'
            if (!description.trim()) newErrors.description = 'Description is required'
            if (estimatedCost.trim() && (isNaN(estimatedCost) || parseFloat(estimatedCost) < 0)) {
                newErrors.estimatedCost = 'Estimated cost must be a valid number'
            }
        }

        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const validateForm = () => {
        const step0Valid = !!tenantId.trim()
        const step1Valid = !!title.trim() && !!issueType && !!description.trim() &&
            (!estimatedCost.trim() || (!isNaN(estimatedCost) && parseFloat(estimatedCost) >= 0))
        return step0Valid && step1Valid
    }

    const resetForm = () => {
        setCurrentStep(0)
        setPropertyId("")
        setTenantId("")
        setTitle("")
        setIssueType("")
        setDescription("")
        setPriority("Normal")
        setEstimatedCost("")
        setAssignedVendor("")
        setErrors({})
    }

    const goNext = () => {
        if (validateStep(currentStep)) {
            setCurrentStep((step) => Math.min(step + 1, MAINTENANCE_STEPS.length - 1))
        }
    }

    const goBack = () => {
        setErrors({})
        setCurrentStep((step) => Math.max(step - 1, 0))
    }

    const handleSubmit = async (e) => {
        e.preventDefault()

        const isLastStep = currentStep === MAINTENANCE_STEPS.length - 1
        if (!isLastStep) {
            goNext()
            return
        }

        if (!validateForm()) return

        if (!propertyId) {
            setErrors({ submit: 'Could not determine the property for this tenant. Please re-select the tenant.' })
            return
        }

        const newMaintenance = {
            buildingId: Number(propertyId),
            unitId: selectedTenant?.unitId ? Number(selectedTenant.unitId) : null,
            tenantId: Number(tenantId),
            title,
            description,
            priority,
            category: issueType,
            estimatedCost: estimatedCost.trim() ? parseFloat(estimatedCost) : null,
            assignedVendor: assignedVendor.trim() || null
        }

        try {
            await onMaintenanceAdded(newMaintenance, { skipNavigate: true })
            setSubmittedMaintenance(newMaintenance)
            setShowSuccessModal(true)
            resetForm()
        } catch (error) {
            console.error('Error submitting form:', error)
            setErrors({ submit: 'Failed to add maintenance request. Please try again.' })
        }
    }

    const isLastStep = currentStep === MAINTENANCE_STEPS.length - 1

    return (
        <>
            <h1>Add Maintenance Request</h1>
            {errors.submit && <div className="error-message">{errors.submit}</div>}

            <div className="step-progress">
                {MAINTENANCE_STEPS.map((label, idx) => (
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
                    <>
                        <div className="form-group">
                            <label>Tenant:</label>
                            <select
                                value={tenantId}
                                onChange={(e) => {
                                    const val = e.target.value
                                    setTenantId(val)
                                    const sel = tenants.find(t => String(t.id) === String(val))
                                    if (sel) {
                                        if (sel.buildingId) setPropertyId(String(sel.buildingId))
                                        else if (sel.propertyId) setPropertyId(String(sel.propertyId))
                                        else if (sel.propertyName) {
                                            const p = properties.find(pp => String(pp.id) === String(sel.propertyId) || pp.propertyName === sel.propertyName || pp.name === sel.propertyName)
                                            if (p) setPropertyId(String(p.id))
                                        }
                                    } else {
                                        setPropertyId("")
                                    }
                                }}
                                className={errors.tenantId ? 'input-error' : ''}
                            >
                                <option value="">Select tenant</option>
                                {tenants.map(t => (
                                    <option key={t.id} value={t.id}>{(t.firstName || '') + ' ' + (t.lastName || '') + (t.unitNumber ? ` - ${t.unitNumber}` : '')}</option>
                                ))}
                            </select>
                            {errors.tenantId && <span className="error-text">{errors.tenantId}</span>}
                        </div>

                        <div className="form-group">
                            <label>Property & Unit:</label>
                            {selectedTenant ? (
                                <>
                                    <div className="info-text">{selectedProperty ? (selectedProperty.name || selectedProperty.title || selectedProperty.propertyName || selectedProperty.address || selectedProperty.id) : (selectedTenant.propertyName || 'Unknown property')}</div>
                                    <div className="info-text">Unit: {selectedTenant.unitNumber || selectedTenant.unitId || 'N/A'}</div>
                                </>
                            ) : (
                                <div className="info-text">Select a tenant to view property and unit</div>
                            )}
                        </div>
                    </>
                )}

                {currentStep === 1 && (
                    <>
                        <div className="form-group">
                            <label>Title:</label>
                            <input
                                type="text"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                className={errors.title ? 'input-error' : ''}
                                placeholder="e.g. Leaking kitchen sink"
                            />
                            {errors.title && <span className="error-text">{errors.title}</span>}
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label>Issue Type:</label>
                                <select
                                    value={issueType}
                                    onChange={(e) => setIssueType(e.target.value)}
                                    className={errors.issueType ? 'input-error' : ''}
                                >
                                    <option value="">Select issue type</option>
                                    {issueTypes.map(type => (
                                        <option key={type} value={type}>{type}</option>
                                    ))}
                                </select>
                                {errors.issueType && <span className="error-text">{errors.issueType}</span>}
                            </div>
                            <div className="form-group">
                                <label>Priority:</label>
                                <select value={priority} onChange={(e) => setPriority(e.target.value)}>
                                    <option value="Low">Low</option>
                                    <option value="Normal">Normal</option>
                                    <option value="High">High</option>
                                    <option value="Urgent">Urgent</option>
                                </select>
                            </div>
                        </div>

                        <div className="form-group">
                            <label>Description:</label>
                            <textarea
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                className={errors.description ? 'input-error' : ''}
                                placeholder="Describe the maintenance issue..."
                            />
                            {errors.description && <span className="error-text">{errors.description}</span>}
                        </div>

                        <div className="form-row">
                            <div className="form-group">
                                <label>Estimated Cost (KES):</label>
                                <input
                                    type="number"
                                    value={estimatedCost}
                                    onChange={(e) => setEstimatedCost(e.target.value)}
                                    className={errors.estimatedCost ? 'input-error' : ''}
                                    placeholder="0.00"
                                    step="0.01"
                                />
                                {errors.estimatedCost && <span className="error-text">{errors.estimatedCost}</span>}
                            </div>
                            <div className="form-group">
                                <label>Assigned Vendor (optional):</label>
                                <input
                                    type="text"
                                    value={assignedVendor}
                                    onChange={(e) => setAssignedVendor(e.target.value)}
                                    placeholder="e.g. Acme Plumbing"
                                />
                            </div>
                        </div>
                    </>
                )}

                {currentStep === 2 && (
                    <div className="review-summary">
                        <p><strong>Tenant:</strong> {selectedTenant ? `${selectedTenant.firstName || ''} ${selectedTenant.lastName || ''}`.trim() : 'Not selected'}</p>
                        <p><strong>Property:</strong> {selectedProperty ? (selectedProperty.name || selectedProperty.title || selectedProperty.propertyName || selectedProperty.address || `Property #${selectedProperty.id}`) : 'Unknown property'}</p>
                        <p><strong>Unit:</strong> {selectedTenant?.unitNumber || selectedTenant?.unitId || 'N/A'}</p>
                        <p><strong>Title:</strong> {title}</p>
                        <p><strong>Issue Type:</strong> {issueType}</p>
                        <p><strong>Priority:</strong> {priority}</p>
                        <p><strong>Estimated Cost:</strong> KES {Number(estimatedCost || 0).toLocaleString()}</p>
                        <p><strong>Description:</strong> {description}</p>
                        {assignedVendor && <p><strong>Assigned Vendor:</strong> {assignedVendor}</p>}
                    </div>
                )}

                <div className="form-buttons">
                    {currentStep > 0 && (
                        <button type="button" className="reset-btn" onClick={goBack}>Back</button>
                    )}
                    <button type="submit" className="submit-btn">
                        {isLastStep ? 'Add Maintenance Request' : 'Next'}
                    </button>
                    {isLastStep && (
                        <button type="button" className="reset-btn" onClick={resetForm}>Reset</button>
                    )}
                </div>
            </form>

            <GlassSuccessModal
                open={showSuccessModal}
                title="Maintenance Request Created"
                message="The maintenance issue has been recorded successfully."
                details={submittedMaintenance ? [
                    `Title: ${submittedMaintenance.title}`,
                    `Category: ${submittedMaintenance.category}`,
                    `Priority: ${submittedMaintenance.priority}`,
                    submittedMaintenance.estimatedCost != null ? `Cost: KES ${Number(submittedMaintenance.estimatedCost).toLocaleString()}` : null
                ].filter(Boolean) : []}
                onClose={() => {
                    setShowSuccessModal(false)
                    onNavigate('maintenanceList')
                }}
                actionLabel="Confirmed"
            />
        </>
    )
}

export default AddMaintenance
