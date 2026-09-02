import { useState } from 'react'
import GlassSuccessModal from '../components/GlassSuccessModal'
import './AddUnits.css'

function AddUnits({ property, onUnitsAdded, onNavigate }) {
    const [unitNumber, setUnitNumber] = useState("");
    const [unitType, setUnitType] = useState("other");
    const [bedrooms, setBedrooms] = useState("");
    const [bathrooms, setBathrooms] = useState("");
    const [squareFootage, setSquareFootage] = useState("");
    const [monthlyRent, setMonthlyRent] = useState("");
    const [amenities, setAmenities] = useState("");
    const [hasToilet, setHasToilet] = useState("shared");
    const [isFurnished, setIsFurnished] = useState(false);
    const [floorNumber, setFloorNumber] = useState(0);
    const [errors, setErrors] = useState({});
    const [showSuccessModal, setShowSuccessModal] = useState(false);
    const [submittedUnit, setSubmittedUnit] = useState(null);

    const validateForm = () => {
        const newErrors = {};

        if (!unitNumber.trim()) newErrors.unitNumber = "Unit number is required";
        if (!unitType) newErrors.unitType = "Unit type is required";

        if (unitType === "other") {
            if (!bedrooms.toString().trim()) newErrors.bedrooms = "Number of bedrooms is required";
            else if (isNaN(bedrooms) || parseInt(bedrooms) < 0) newErrors.bedrooms = "Bedrooms must be a valid number";
        }

        if (unitType === "other" || unitType === "bedsitter") {
            if (!bathrooms.toString().trim()) newErrors.bathrooms = "Number of bathrooms is required";
            else if (isNaN(bathrooms) || parseFloat(bathrooms) < 0) newErrors.bathrooms = "Bathrooms must be a valid number";
        }

        if (!squareFootage.toString().trim()) newErrors.squareFootage = "Square footage is required";
        else if (isNaN(squareFootage) || parseFloat(squareFootage) <= 0) newErrors.squareFootage = "Square footage must be a valid positive number";

        if (!monthlyRent.toString().trim()) newErrors.monthlyRent = "Monthly rent is required";
        else if (isNaN(monthlyRent) || parseFloat(monthlyRent) <= 0) newErrors.monthlyRent = "Monthly rent must be a valid positive number";

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validateForm()) return;

        try {
            const newUnit = {
                unitNumber,
                unitType,
                buildingId: property?.id,
                floorNumber: parseInt(floorNumber) || 0,
                bedroomCount: unitType === "bedsitter" ? 0 : unitType === "other" ? parseInt(bedrooms) : 1,
                bathroomCount: (unitType === "single_room" || unitType === "double_room")
                    ? (hasToilet === "ensuite" ? 1 : 0)
                    : parseFloat(bathrooms),
                squareFootage: parseFloat(squareFootage),
                monthlyRent: parseFloat(monthlyRent),
                isFurnished,
                amenities: amenities ? amenities.trim() : null,
                notes: null,
            };

            await onUnitsAdded(newUnit);
            setSubmittedUnit(newUnit);
            setShowSuccessModal(true);
            resetForm();
        } catch (error) {
            console.error("Error adding unit:", error);
            setErrors({ submit: "Failed to add unit. Please try again." });
        }
    };

    const resetForm = () => {
        setUnitNumber("");
        setUnitType("other");
        setBedrooms("");
        setBathrooms("");
        setSquareFootage("");
        setMonthlyRent("");
        setHasToilet("shared");
        setIsFurnished(false);
        setFloorNumber(0);
        setAmenities("");
        setErrors({});
    };

    return (
        <div className="add-units-container">
            <div className="add-units-header">
                <h2>Add Unit to {property?.name}</h2>
                <p className="property-location">{property?.address}, {property?.city}</p>
            </div>

            <form className="add-units-form" onSubmit={handleSubmit}>
                {errors.submit && <div className="error-message">{errors.submit}</div>}

                <div className="form-row">
                    <div className="form-group">
                        <label htmlFor="unitNumber">Unit Number/Name</label>
                        <input
                            id="unitNumber"
                            type="text"
                            value={unitNumber}
                            onChange={(e) => setUnitNumber(e.target.value)}
                            placeholder="e.g., 101, A1, Penthouse"
                            className={errors.unitNumber ? 'input-error' : ''}
                        />
                        {errors.unitNumber && <span className="error-text">{errors.unitNumber}</span>}
                    </div>

                    <div className="form-group">
                        <label htmlFor="unitType">Unit Type</label>
                        <select
                            id="unitType"
                            value={unitType}
                            onChange={(e) => setUnitType(e.target.value)}
                            className={errors.unitType ? 'input-error' : ''}
                        >
                            <option value="bedsitter">Bedsitter (Studio)</option>
                            <option value="single_room">Single Room</option>
                            <option value="double_room">Double Room</option>
                            <option value="other">Other (Specify Bedrooms)</option>
                        </select>
                        {errors.unitType && <span className="error-text">{errors.unitType}</span>}
                    </div>
                </div>

                <div className="form-row">
                    <div className="form-group">
                        <label htmlFor="floorNumber">Floor Number</label>
                        <input
                            id="floorNumber"
                            type="number"
                            value={floorNumber}
                            onChange={(e) => setFloorNumber(e.target.value)}
                            placeholder="0"
                            min="0"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="isFurnished">Furnished</label>
                        <select
                            id="isFurnished"
                            value={isFurnished}
                            onChange={(e) => setIsFurnished(e.target.value === "true")}
                        >
                            <option value="false">Unfurnished</option>
                            <option value="true">Furnished</option>
                        </select>
                    </div>
                </div>

                {unitType === "other" && (
                    <div className="form-row">
                        <div className="form-group">
                            <label htmlFor="bedrooms">Bedrooms</label>
                            <input
                                id="bedrooms"
                                type="number"
                                value={bedrooms}
                                onChange={(e) => setBedrooms(e.target.value)}
                                placeholder="0"
                                min="0"
                                className={errors.bedrooms ? 'input-error' : ''}
                            />
                            {errors.bedrooms && <span className="error-text">{errors.bedrooms}</span>}
                        </div>
                    </div>
                )}

                {(unitType === "single_room" || unitType === "double_room") && (
                    <div className="form-row">
                        <div className="form-group">
                            <label htmlFor="hasToilet">Toilet Type</label>
                            <select
                                id="hasToilet"
                                value={hasToilet}
                                onChange={(e) => setHasToilet(e.target.value)}
                            >
                                <option value="shared">Shared Toilet</option>
                                <option value="ensuite">En-Suite (Attached)</option>
                            </select>
                            <small className="form-help">
                                Note: {unitType === "single_room" ? "Single" : "Double"} rooms typically share toilets
                            </small>
                        </div>
                    </div>
                )}

                {(unitType === "bedsitter" || unitType === "other") && (
                    <div className="form-row">
                        <div className="form-group">
                            <label htmlFor="bathrooms">Bathrooms</label>
                            <input
                                id="bathrooms"
                                type="number"
                                value={bathrooms}
                                onChange={(e) => setBathrooms(e.target.value)}
                                placeholder="0"
                                step="0.5"
                                min="0"
                                className={errors.bathrooms ? 'input-error' : ''}
                            />
                            {errors.bathrooms && <span className="error-text">{errors.bathrooms}</span>}
                        </div>

                        <div className="form-group">
                            <label htmlFor="squareFootage">Square Footage</label>
                            <input
                                id="squareFootage"
                                type="number"
                                value={squareFootage}
                                onChange={(e) => setSquareFootage(e.target.value)}
                                placeholder="0"
                                min="0"
                                className={errors.squareFootage ? 'input-error' : ''}
                            />
                            {errors.squareFootage && <span className="error-text">{errors.squareFootage}</span>}
                        </div>
                    </div>
                )}

                {(unitType === "single_room" || unitType === "double_room") && (
                    <div className="form-row">
                        <div className="form-group">
                            <label htmlFor="squareFootage">Square Footage</label>
                            <input
                                id="squareFootage"
                                type="number"
                                value={squareFootage}
                                onChange={(e) => setSquareFootage(e.target.value)}
                                placeholder="0"
                                min="0"
                                className={errors.squareFootage ? 'input-error' : ''}
                            />
                            {errors.squareFootage && <span className="error-text">{errors.squareFootage}</span>}
                        </div>
                    </div>
                )}

                <div className="form-row">
                    <div className="form-group">
                        <label htmlFor="monthlyRent">Monthly Rent (KES)</label>
                        <input
                            id="monthlyRent"
                            type="number"
                            value={monthlyRent}
                            onChange={(e) => setMonthlyRent(e.target.value)}
                            placeholder="0"
                            step="0.01"
                            min="0"
                            className={errors.monthlyRent ? 'input-error' : ''}
                        />
                        {errors.monthlyRent && <span className="error-text">{errors.monthlyRent}</span>}
                    </div>
                </div>

                <div className="form-group">
                    <label htmlFor="amenities">Amenities (comma separated)</label>
                    <textarea
                        id="amenities"
                        value={amenities}
                        onChange={(e) => setAmenities(e.target.value)}
                        placeholder="e.g., Air Conditioning, Balcony, In-unit Laundry, Gym"
                        rows="3"
                    />
                </div>

                <div className="form-buttons">
                    <button type="submit" className="btn btn-primary">Add Unit</button>
                    <button
                        type="button"
                        className="btn btn-secondary"
                        onClick={() => onNavigate('propertiesList')}
                    >
                        Back to Properties
                    </button>
                </div>
            </form>

            <GlassSuccessModal
                open={showSuccessModal}
                title="Unit Added"
                message="The new unit has been added successfully."
                details={submittedUnit ? [
                    `Unit: ${submittedUnit.unitNumber}`,
                    `Type: ${submittedUnit.unitType}`,
                    `Rent: KES ${submittedUnit.monthlyRent.toLocaleString()}`
                ] : []}
                onClose={() => setShowSuccessModal(false)}
                actionLabel="Confirmed"
            />
        </div>
    );
}

export default AddUnits;