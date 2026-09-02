import { useState, useEffect } from 'react'
import GlassSuccessModal from '../components/GlassSuccessModal'
import CustomSelect from '../components/CustomSelect'
import api from '../services/api'
import './AddTenant.css'

const TENANT_STEPS = ['Profile', 'Property', 'Review'];

function AddTenant({ properties = [], onTenantAdded, onNavigate }) {
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [email, setEmail] = useState("");
    const [phone, setPhone] = useState("");
    const [idNumber, setIdNumber] = useState("");
    const [propertyId, setPropertyId] = useState("");
    const [unitId, setUnitId] = useState("");
    const [moveInDate, setMoveInDate] = useState("");
    const [rentAmount, setRentAmount] = useState("");
    const [errors, setErrors] = useState({});
    const [showSuccessModal, setShowSuccessModal] = useState(false);
    const [submittedTenant, setSubmittedTenant] = useState(null);
    const [availableUnits, setAvailableUnits] = useState([]);
    const [loadingUnits, setLoadingUnits] = useState(false);
    const [currentStep, setCurrentStep] = useState(0);

    const selectedProperty = properties.find(p => p.id === parseInt(propertyId));

    useEffect(() => {
        if (!propertyId) {
            setAvailableUnits([]);
            return;
        }
        const fetchUnits = async () => {
            setLoadingUnits(true);
            try {
                const res = await api.get(`/units/building/${propertyId}`);
                if (res.data?.success) setAvailableUnits(res.data.data);
                else setAvailableUnits([]);
            } catch (err) {
                console.error('Fetch units error:', err);
                setAvailableUnits([]);
            } finally {
                setLoadingUnits(false);
            }
        };
        fetchUnits();
    }, [propertyId]);

    const propertyOptions = properties.map(p => ({
        value: p.id,
        // Building entity only has Name/Address/City — no propertyName/location.
        // Fall back through possible shapes just in case, same pattern as
        // getPropertyDisplay() in TenantsList.jsx.
        label: `${p.name || p.propertyName || 'Unnamed property'} - ${p.city || p.address || p.location || ''}`
    }));

    const unitOptions = loadingUnits
        ? [{ value: '', label: 'Loading units...' }]
        : availableUnits.map(u => ({
            value: u.id,
            label: `${u.unitNumber} (${u.unitType || 'Unit'}) - ${u.status}`
        }));

    const validateStep = (step) => {
        const newErrors = {};

        if (step === 0) {
            if (!firstName.trim()) newErrors.firstName = "First name is required";
            if (!lastName.trim()) newErrors.lastName = "Last name is required";
            if (!email.trim()) {
                newErrors.email = "Email is required";
            } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
                newErrors.email = "Please enter a valid email address";
            }
            if (!phone.trim()) {
                newErrors.phone = "Phone number is required";
            } else if (!/^[0-9\s\-\+\(\)]+$/.test(phone) || phone.replace(/\D/g, '').length < 10) {
                newErrors.phone = "Please enter a valid phone number";
            }
            if (!idNumber.trim()) newErrors.idNumber = "ID number is required";
        }

        if (step === 1) {
            if (!propertyId) newErrors.propertyId = "Property selection is required";
            if (!unitId) newErrors.unitId = "Unit selection is required";
            if (!moveInDate) newErrors.moveInDate = "Move-in date is required";
            if (!rentAmount.trim()) {
                newErrors.rentAmount = "Rent amount is required";
            } else if (isNaN(rentAmount) || parseFloat(rentAmount) <= 0) {
                newErrors.rentAmount = "Rent amount must be a valid positive number";
            }
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const validateForm = () => {
        const stepOneValid = validateStep(0);
        const stepTwoValid = validateStep(1);
        return stepOneValid && stepTwoValid;
    };

    const handleNext = () => {
        if (validateStep(currentStep)) {
            setCurrentStep((step) => Math.min(step + 1, TENANT_STEPS.length - 1));
        }
    };

    const handlePrevious = () => {
        setCurrentStep((step) => Math.max(step - 1, 0));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validateForm()) return;

        const selectedUnit = availableUnits.find(u => u.id === parseInt(unitId));
        const payload = {
            firstName,
            lastName,
            email,
            phoneNumber: phone,
            identificationNumber: idNumber,
            identificationType: "NationalID",
            dateOfBirth: "2000-01-01T00:00:00",
            buildingId: parseInt(propertyId),
            unitId: parseInt(unitId),
            moveInDate: new Date(moveInDate).toISOString(),
            rentAmount: parseFloat(rentAmount)
        };

        try {
            const res = await api.post('/tenants', payload);
            if (res.data?.success) {
                const created = res.data.data;
                const tenantRecord = {
                    ...(created || {}),
                    firstName,
                    lastName,
                    email,
                    phoneNumber: phone,
                    identificationNumber: idNumber,
                    propertyName: selectedProperty?.propertyName || selectedProperty?.name || selectedProperty?.buildingName || '',
                    propertyId: parseInt(propertyId),
                    buildingId: parseInt(propertyId),
                    unitNumber: selectedUnit?.unitNumber || '',
                    unitId: parseInt(unitId),
                    unitType: selectedUnit?.unitType || '',
                    rentAmount: parseFloat(rentAmount),
                    moveInDate: new Date(moveInDate).toISOString(),
                    createdAt: created?.createdAt || new Date().toISOString()
                };

                setSubmittedTenant({
                    firstName,
                    lastName,
                    propertyName: tenantRecord.propertyName,
                    unitNumber: tenantRecord.unitNumber,
                    rentAmount: tenantRecord.rentAmount
                });
                setShowSuccessModal(true);
                resetForm();
                onTenantAdded?.(tenantRecord, {
                    skipNavigate: true,
                    property: selectedProperty,
                    unit: selectedUnit,
                    firstName,
                    lastName,
                    email,
                    phoneNumber: phone,
                    propertyId: parseInt(propertyId),
                    unitId: parseInt(unitId),
                    rentAmount: parseFloat(rentAmount),
                    moveInDate: new Date(moveInDate).toISOString()
                });
            } else {
                setErrors({ submit: res.data?.message || "Failed to add tenant." });
            }
        } catch (error) {
            console.error("Error submitting form:", error);
            const msg = error.response?.data?.message || "Failed to add tenant. Please try again.";
            setErrors({ submit: msg });
        }
    };

    const resetForm = () => {
        setFirstName(""); setLastName(""); setEmail(""); setPhone("");
        setIdNumber(""); setPropertyId(""); setUnitId("");
        setMoveInDate(""); setRentAmount(""); setErrors({});
        setCurrentStep(0);
        setAvailableUnits([]);
    };

    const renderStepContent = () => {
        if (currentStep === 0) {
            return (
                <>
                    <div className="form-row">
                        <div className="form-group">
                            <label>First Name</label>
                            <input type="text" value={firstName} placeholder="John"
                                onChange={(e) => setFirstName(e.target.value)}
                                className={errors.firstName ? 'input-error' : ''} />
                            {errors.firstName && <span className="error-text">{errors.firstName}</span>}
                        </div>
                        <div className="form-group">
                            <label>Last Name</label>
                            <input type="text" value={lastName} placeholder="Doe"
                                onChange={(e) => setLastName(e.target.value)}
                                className={errors.lastName ? 'input-error' : ''} />
                            {errors.lastName && <span className="error-text">{errors.lastName}</span>}
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>Email</label>
                            <input type="email" value={email} placeholder="tenant@example.com"
                                onChange={(e) => setEmail(e.target.value)}
                                className={errors.email ? 'input-error' : ''} />
                            {errors.email && <span className="error-text">{errors.email}</span>}
                        </div>
                        <div className="form-group">
                            <label>Phone Number</label>
                            <input type="tel" value={phone} placeholder="+254 700 000000"
                                onChange={(e) => setPhone(e.target.value)}
                                className={errors.phone ? 'input-error' : ''} />
                            {errors.phone && <span className="error-text">{errors.phone}</span>}
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>ID Number</label>
                            <input type="text" value={idNumber} placeholder="e.g. 12345678"
                                onChange={(e) => setIdNumber(e.target.value)}
                                className={errors.idNumber ? 'input-error' : ''} />
                            {errors.idNumber && <span className="error-text">{errors.idNumber}</span>}
                        </div>
                    </div>
                </>
            );
        }

        if (currentStep === 1) {
            return (
                <>
                    <div className="form-row">
                        <div className="form-group">
                            <label>Property</label>
                            <CustomSelect
                                options={propertyOptions}
                                value={propertyId}
                                onChange={(val) => { setPropertyId(val); setUnitId(""); }}
                                placeholder="-- Select a Property --"
                            />
                            {errors.propertyId && <span className="error-text">{errors.propertyId}</span>}
                        </div>
                        <div className="form-group">
                            <label>Unit</label>
                            <CustomSelect
                                options={unitOptions}
                                value={unitId}
                                onChange={(val) => { if (!loadingUnits) setUnitId(val); }}
                                placeholder={loadingUnits ? 'Loading units...' : '-- Select a Unit --'}
                                disabled={!propertyId || loadingUnits}
                            />
                            {errors.unitId && <span className="error-text">{errors.unitId}</span>}
                            {propertyId && !loadingUnits && availableUnits.length === 0 && (
                                <span className="info-text">No units found for this property</span>
                            )}
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-group">
                            <label>Move-In Date</label>
                            <input type="date" value={moveInDate}
                                onChange={(e) => setMoveInDate(e.target.value)}
                                className={errors.moveInDate ? 'input-error' : ''} />
                            {errors.moveInDate && <span className="error-text">{errors.moveInDate}</span>}
                        </div>
                        <div className="form-group">
                            <label>Rent Amount</label>
                            <input type="number" value={rentAmount} placeholder="Enter monthly rent"
                                onChange={(e) => setRentAmount(e.target.value)}
                                className={errors.rentAmount ? 'input-error' : ''} />
                            {errors.rentAmount && <span className="error-text">{errors.rentAmount}</span>}
                        </div>
                    </div>
                </>
            );
        }

        return (
            <div className="review-summary">
                <p><strong>Tenant:</strong> {firstName} {lastName}</p>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Phone:</strong> {phone}</p>
                <p><strong>ID:</strong> {idNumber}</p>
                <p><strong>Property:</strong> {properties.find(p => String(p.id) === String(propertyId))?.name || 'Not selected'}</p>
                <p><strong>Unit:</strong> {availableUnits.find(u => String(u.id) === String(unitId))?.unitNumber || unitId || 'Not selected'}</p>
                <p><strong>Move-In:</strong> {moveInDate || 'Not set'}</p>
                <p><strong>Rent:</strong> KSh {rentAmount ? Number(rentAmount).toLocaleString() : '0'}</p>
            </div>
        );
    };

    return (
        <div className="page-wrapper">
            <GlassSuccessModal
                open={showSuccessModal}
                title="Tenant Added"
                message="The tenant has been created successfully."
                details={submittedTenant ? [
                    `${submittedTenant.firstName} ${submittedTenant.lastName}`,
                    `Unit ${submittedTenant.unitNumber}`,
                    `Rent: KSh ${submittedTenant.rentAmount.toLocaleString()}/mo`
                ] : []}
                onClose={() => { setShowSuccessModal(false); onNavigate('tenantsList'); }}
                actionLabel="Confirmed"
            />

            <h1 className="page-header">Add Tenant</h1>
            {errors.submit && <div className="error-message">{errors.submit}</div>}

            <div className="step-progress">
                {TENANT_STEPS.map((label, idx) => (
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
                {renderStepContent()}

                <div className="form-buttons">
                    {currentStep > 0 && (
                        <button type="button" className="reset-btn" onClick={handlePrevious}>Back</button>
                    )}
                    {currentStep < TENANT_STEPS.length - 1 ? (
                        <button type="button" className="submit-btn" onClick={handleNext}>Next</button>
                    ) : (
                        <button type="submit" className="submit-btn">Add Tenant</button>
                    )}
                    <button type="button" className="reset-btn" onClick={resetForm}>Reset</button>
                </div>
            </form>
        </div>
    );
}

export default AddTenant;