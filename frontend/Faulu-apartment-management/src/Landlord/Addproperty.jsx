import { useState } from 'react'
import GlassSuccessModal from '../components/GlassSuccessModal'
import authService from '../services/authService'
import './Addproperty.css'

const STEPS = ['Basic Info', 'Property Details', 'Description']

function AddProperty({ onPropertyAdded, onNavigate }) {

    const [currentStep, setCurrentStep] = useState(0)

    const [name, setName] = useState("")
    const [address, setAddress] = useState("")
    const [city, setCity] = useState("")
    const [postalCode, setPostalCode] = useState("")
    const [totalUnits, setTotalUnits] = useState("")
    const [propertyValue, setPropertyValue] = useState("")
    const [yearBuilt, setYearBuilt] = useState("")
    const [description, setDescription] = useState("")
    const [errors, setErrors] = useState({})
    const [showSuccessModal, setShowSuccessModal] = useState(false)
    const [submittedProperty, setSubmittedProperty] = useState(null)

    // ── Per-step validation ──────────────────────────────────────────────
    const validateStep = (step) => {
        const newErrors = {}

        if (step === 0) {
            if (!name.trim()) newErrors.name = "Property name is required"
            if (!address.trim()) newErrors.address = "Address is required"
            if (!city.trim()) newErrors.city = "City is required"
        }

        if (step === 1) {
            if (!totalUnits || isNaN(totalUnits) || parseInt(totalUnits) <= 0)
                newErrors.totalUnits = "Total units must be a valid positive number"
        }

        setErrors(newErrors)
        return Object.keys(newErrors).length === 0
    }

    const validateForm = () => {
        // Full validation across all steps, used as a final guard before submit
        const step0Valid = (() => {
            const e = {}
            if (!name.trim()) e.name = "Property name is required"
            if (!address.trim()) e.address = "Address is required"
            if (!city.trim()) e.city = "City is required"
            return Object.keys(e).length === 0
        })()
        const step1Valid = totalUnits && !isNaN(totalUnits) && parseInt(totalUnits) > 0
        return step0Valid && step1Valid
    }

    const resetForm = () => {
        setName("")
        setAddress("")
        setCity("")
        setPostalCode("")
        setTotalUnits("")
        setPropertyValue("")
        setYearBuilt("")
        setDescription("")
        setErrors({})
        setCurrentStep(0)
    }

    const goNext = () => {
        if (validateStep(currentStep)) {
            setCurrentStep((s) => Math.min(s + 1, STEPS.length - 1))
        }
    }

    const goBack = () => {
        setErrors({})
        setCurrentStep((s) => Math.max(s - 1, 0))
    }

    const handleSubmit = async (e) => {
        e.preventDefault()

        const isLastStep = currentStep === STEPS.length - 1
        if (!isLastStep) {
            goNext()
            return
        }

        if (!validateForm()) return

        const currentUser = authService.getCurrentUser()

        const newProperty = {
            name,
            address,
            city,
            postalCode,
            totalUnits: parseInt(totalUnits),
            propertyValue: propertyValue ? parseFloat(propertyValue) : null,
            yearBuilt: yearBuilt ? parseInt(yearBuilt) : null,
            description,
            ownerId: currentUser?.id,
            userId: currentUser?.id,
        }

        try {
            onPropertyAdded(newProperty, { skipNavigate: true })
            setSubmittedProperty(newProperty)
            setShowSuccessModal(true)
            resetForm()
        } catch (error) {
            console.error("Error submitting form:", error)
            setErrors({ submit: "Failed to add property. Please try again." })
        }
    }

    const isLastStep = currentStep === STEPS.length - 1

    return (
        <>
            <h1 className="page_header">Add Property</h1>
            {errors.submit && <div className="error-message">{errors.submit}</div>}

            {/* Segmented progress bar */}
            <div className="step-progress">
                {STEPS.map((label, idx) => (
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
                        <div >
                            <label>Property Name:</label>
                            <input
                                type="text"
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                                className={errors.name ? 'input-error' : ''}
                            />
                            {errors.name && <span className="error-text">{errors.name}</span>}
                        </div>
                        <div className="form-group">
                            <label>Address:</label>
                            <input
                                type="text"
                                value={address}
                                onChange={(e) => setAddress(e.target.value)}
                                className={errors.address ? 'input-error' : ''}
                            />
                            {errors.address && <span className="error-text">{errors.address}</span>}
                        </div>
                        <div className="form-group">
                            <label>City:</label>
                            <input
                                type="text"
                                value={city}
                                onChange={(e) => setCity(e.target.value)}
                                className={errors.city ? 'input-error' : ''}
                            />
                            {errors.city && <span className="error-text">{errors.city}</span>}
                        </div>
                        <div className="form-group">
                            <label>Postal Code:</label>
                            <input
                                type="text"
                                value={postalCode}
                                onChange={(e) => setPostalCode(e.target.value)}
                            />
                        </div>
                    </>
                )}

                {currentStep === 1 && (
                    <>
                        <div className="form-group">
                            <label>Total Units:</label>
                            <input
                                type="number"
                                value={totalUnits}
                                onChange={(e) => setTotalUnits(e.target.value)}
                                className={errors.totalUnits ? 'input-error' : ''}
                                min="1"
                            />
                            {errors.totalUnits && <span className="error-text">{errors.totalUnits}</span>}
                        </div>
                        <div className="form-group">
                            <label>Property Value (KES):</label>
                            <input
                                type="number"
                                value={propertyValue}
                                onChange={(e) => setPropertyValue(e.target.value)}
                                min="0"
                            />
                        </div>
                        <div className="form-group">
                            <label>Year Built:</label>
                            <input
                                type="number"
                                value={yearBuilt}
                                onChange={(e) => setYearBuilt(e.target.value)}
                                min="1900"
                                max={new Date().getFullYear()}
                            />
                        </div>
                    </>
                )}

                {currentStep === 2 && (
                    <>
                        <div className="form-group">
                            <label>Description:</label>
                            <textarea
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                            />
                        </div>

                        <div className="review-summary">
                            <p><strong>{name}</strong></p>
                            <p>{address}, {city} {postalCode}</p>
                            <p>{totalUnits} unit{totalUnits === '1' ? '' : 's'}
                                {propertyValue ? ` · KES ${Number(propertyValue).toLocaleString()}` : ''}
                                {yearBuilt ? ` · Built ${yearBuilt}` : ''}
                            </p>
                        </div>
                    </>
                )}

                <div className="form-buttons">
                    {currentStep > 0 && (
                        <button type="button" className="reset-btn" onClick={goBack}>
                            Back
                        </button>
                    )}
                    <button type="submit" className="submit-btn">
                        {isLastStep ? 'Add Property' : 'Next'}
                    </button>
                    {isLastStep && (
                        <button type="button" className="reset-btn" onClick={resetForm}>Reset</button>
                    )}
                </div>
            </form>
            <GlassSuccessModal
                open={showSuccessModal}
                title="Property Added"
                message="Your new property has been created successfully."
                details={submittedProperty ? [
                    `Name: ${submittedProperty.name}`,
                    `Address: ${submittedProperty.address}`,
                    `City: ${submittedProperty.city}`,
                    `Total Units: ${submittedProperty.totalUnits}`
                ] : []}
                onClose={() => {
                    setShowSuccessModal(false)
                    onNavigate('propertiesList')
                }}
                actionLabel="Great"
            />
        </>
    )
}

export default AddProperty