import { useMemo, useState } from 'react'
import GlassSuccessModal from '../components/GlassSuccessModal'
import './AddUnits.css'

const MAX_BULK_UNITS = 100;

function AddUnits({ property, onUnitsAdded, onBulkUnitsAdded, onNavigate }) {
    // "single" adds one unit, "bulk" adds many similar units at once
    const [addMode, setAddMode] = useState("single");

    // Single mode
    const [unitNumber, setUnitNumber] = useState("");

    // Bulk mode: how unit numbers are produced
    const [numberingMode, setNumberingMode] = useState("pattern"); // "pattern" | "list"
    const [prefix, setPrefix] = useState("");
    const [startNumber, setStartNumber] = useState("1");
    const [count, setCount] = useState("");
    const [unitList, setUnitList] = useState("");

    // Shared by both modes (in bulk mode these are the template for every unit)
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
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [showSuccessModal, setShowSuccessModal] = useState(false);
    const [successDetails, setSuccessDetails] = useState({ title: "", message: "", details: [] });

    const isBulk = addMode === "bulk";

    // Bulk preview: exactly the unit numbers that will be created
    const generatedNumbers = useMemo(() => {
        if (numberingMode === "pattern") {
            const start = parseInt(startNumber);
            const total = parseInt(count);
            if (isNaN(start) || isNaN(total) || total < 1) return [];
            const safeTotal = Math.min(total, MAX_BULK_UNITS + 1); // +1 so validation can flag "too many"
            return Array.from({ length: safeTotal }, (_, i) => `${prefix.trim()}${start + i}`);
        }
        return unitList
            .split(/[,\n]/)
            .map((s) => s.trim())
            .filter(Boolean);
    }, [numberingMode, prefix, startNumber, count, unitList]);

    const duplicateNumbers = useMemo(() => {
        const seen = new Set();
        const dupes = new Set();
        generatedNumbers.forEach((n) => {
            const key = n.toLowerCase();
            if (seen.has(key)) dupes.add(n);
            seen.add(key);
        });
        return [...dupes];
    }, [generatedNumbers]);

    const validateForm = () => {
        const newErrors = {};

        if (isBulk) {
            if (numberingMode === "pattern") {
                if (isNaN(parseInt(startNumber))) newErrors.startNumber = "Starting number is required";
                if (!count.toString().trim() || isNaN(parseInt(count)) || parseInt(count) < 1) {
                    newErrors.count = "How many units? Enter a number of 1 or more";
                }
            } else if (generatedNumbers.length === 0) {
                newErrors.unitList = "Enter at least one unit number";
            }

            if (generatedNumbers.length > MAX_BULK_UNITS) {
                newErrors.count = `You can add at most ${MAX_BULK_UNITS} units at a time`;
                newErrors.unitList = newErrors.count;
            }

            if (duplicateNumbers.length > 0) {
                newErrors.unitList = `Duplicate unit numbers: ${duplicateNumbers.join(", ")}`;
            }
        } else if (!unitNumber.trim()) {
            newErrors.unitNumber = "Unit number is required";
        }

        if (!unitType) newErrors.unitType = "Unit type is required";

        if (unitType === "other") {
            if (!bedrooms.toString().trim()) newErrors.bedrooms = "Number of bedrooms is required";
            else if (isNaN(bedrooms) || parseInt(bedrooms) < 0) newErrors.bedrooms = "Bedrooms must be a valid number";
        }

        if (unitType === "other" || unitType === "bedsitter") {
            if (!bathrooms.toString().trim()) newErrors.bathrooms = "Number of bathrooms is required";
            else if (isNaN(bathrooms) || parseInt(bathrooms) < 0) newErrors.bathrooms = "Bathrooms must be a whole number";
        }

        if (!squareFootage.toString().trim()) newErrors.squareFootage = "Square footage is required";
        else if (isNaN(squareFootage) || parseFloat(squareFootage) <= 0) newErrors.squareFootage = "Square footage must be a valid positive number";

        if (!monthlyRent.toString().trim()) newErrors.monthlyRent = "Monthly rent is required";
        else if (isNaN(monthlyRent) || parseFloat(monthlyRent) <= 0) newErrors.monthlyRent = "Monthly rent must be a valid positive number";

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    // Fields that are identical for one unit or many
    const buildTemplate = () => ({
        unitType,
        floorNumber: parseInt(floorNumber) || 0,
        bedroomCount: unitType === "bedsitter" ? 0 : unitType === "other" ? parseInt(bedrooms) : 1,
        bathroomCount: (unitType === "single_room" || unitType === "double_room")
            ? (hasToilet === "ensuite" ? 1 : 0)
            : parseInt(bathrooms),
        squareFootage: parseFloat(squareFootage),
        monthlyRent: parseFloat(monthlyRent),
        isFurnished,
        amenities: amenities ? amenities.trim() : null,
        notes: null,
    });

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (isSubmitting) return;
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            const template = buildTemplate();

            if (isBulk) {
                const payload = {
                    buildingId: property?.id,
                    unitNumbers: generatedNumbers,
                    ...template,
                };

                await onBulkUnitsAdded(payload);

                setSuccessDetails({
                    title: "Units Added",
                    message: "The new units have been added successfully.",
                    details: [
                        `Units created: ${payload.unitNumbers.length}`,
                        `Range: ${payload.unitNumbers[0]} to ${payload.unitNumbers[payload.unitNumbers.length - 1]}`,
                        `Type: ${payload.unitType}`,
                        `Rent each: KES ${payload.monthlyRent.toLocaleString()}`,
                    ],
                });
            } else {
                const newUnit = {
                    unitNumber,
                    buildingId: property?.id,
                    ...template,
                };

                await onUnitsAdded(newUnit);

                setSuccessDetails({
                    title: "Unit Added",
                    message: "The new unit has been added successfully.",
                    details: [
                        `Unit: ${newUnit.unitNumber}`,
                        `Type: ${newUnit.unitType}`,
                        `Rent: KES ${newUnit.monthlyRent.toLocaleString()}`,
                    ],
                });
            }

            setShowSuccessModal(true);
            resetForm();
        } catch (error) {
            console.error("Error adding unit(s):", error);
            // For bulk, the backend says exactly which unit numbers clash, so show its message when we have it
            setErrors({
                submit: error?.message || (isBulk
                    ? "Failed to add units. Please try again."
                    : "Failed to add unit. Please try again."),
            });
        } finally {
            setIsSubmitting(false);
        }
    };

    const resetForm = () => {
        setUnitNumber("");
        setPrefix("");
        setStartNumber("1");
        setCount("");
        setUnitList("");
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

    const switchMode = (mode) => {
        setAddMode(mode);
        setErrors({});
    };

    const previewLimit = 12;
    const shownNumbers = generatedNumbers.slice(0, previewLimit);
    const hiddenCount = generatedNumbers.length - shownNumbers.length;

    const submitLabel = isSubmitting
        ? "Adding..."
        : isBulk
            ? (generatedNumbers.length > 1 ? `Add ${generatedNumbers.length} Units` : "Add Unit")
            : "Add Unit";

    return (
        <div className="add-units-container">
            <div className="add-units-header">
                <h2>{isBulk ? "Add Multiple Units to" : "Add Unit to"} {property?.name}</h2>
                <p className="property-location">{property?.address}, {property?.city}</p>
            </div>

            <form className="add-units-form" onSubmit={handleSubmit}>
                {errors.submit && <div className="error-message">{errors.submit}</div>}

                {/* Single vs multiple */}
                <div className="form-buttons">
                    <button
                        type="button"
                        className={`btn ${!isBulk ? 'btn-primary' : 'btn-secondary'}`}
                        onClick={() => switchMode("single")}
                    >
                        Single Unit
                    </button>
                    <button
                        type="button"
                        className={`btn ${isBulk ? 'btn-primary' : 'btn-secondary'}`}
                        onClick={() => switchMode("bulk")}
                    >
                        Multiple Units
                    </button>
                </div>

                {!isBulk ? (
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
                ) : (
                    <>
                        <div className="form-group">
                            <label htmlFor="numberingMode">How do you want to number the units?</label>
                            <select
                                id="numberingMode"
                                value={numberingMode}
                                onChange={(e) => setNumberingMode(e.target.value)}
                            >
                                <option value="pattern">Pattern (e.g., A1, A2, A3...)</option>
                                <option value="list">Type my own list</option>
                            </select>
                        </div>

                        {numberingMode === "pattern" ? (
                            <div className="form-row">
                                <div className="form-group">
                                    <label htmlFor="prefix">Prefix (optional)</label>
                                    <input
                                        id="prefix"
                                        type="text"
                                        value={prefix}
                                        onChange={(e) => setPrefix(e.target.value)}
                                        placeholder="e.g., A or Room "
                                    />
                                </div>

                                <div className="form-group">
                                    <label htmlFor="startNumber">Starting Number</label>
                                    <input
                                        id="startNumber"
                                        type="number"
                                        value={startNumber}
                                        onChange={(e) => setStartNumber(e.target.value)}
                                        placeholder="1"
                                        className={errors.startNumber ? 'input-error' : ''}
                                    />
                                    {errors.startNumber && <span className="error-text">{errors.startNumber}</span>}
                                </div>

                                <div className="form-group">
                                    <label htmlFor="count">How Many Units</label>
                                    <input
                                        id="count"
                                        type="number"
                                        value={count}
                                        onChange={(e) => setCount(e.target.value)}
                                        placeholder="e.g., 6"
                                        min="1"
                                        max={MAX_BULK_UNITS}
                                        className={errors.count ? 'input-error' : ''}
                                    />
                                    {errors.count && <span className="error-text">{errors.count}</span>}
                                </div>
                            </div>
                        ) : (
                            <div className="form-group">
                                <label htmlFor="unitList">Unit Numbers (separate with commas or new lines)</label>
                                <textarea
                                    id="unitList"
                                    value={unitList}
                                    onChange={(e) => setUnitList(e.target.value)}
                                    placeholder="e.g., A1, A2, B1, B2"
                                    rows="3"
                                    className={errors.unitList ? 'input-error' : ''}
                                />
                                {errors.unitList && <span className="error-text">{errors.unitList}</span>}
                            </div>
                        )}

                        {/* Live preview of what will be created */}
                        <div className="form-group">
                            <label>Preview</label>
                            {generatedNumbers.length === 0 ? (
                                <small className="form-help">Your unit numbers will show up here.</small>
                            ) : (
                                <>
                                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                                        {shownNumbers.map((n) => (
                                            <span
                                                key={n}
                                                style={{
                                                    padding: '4px 10px',
                                                    borderRadius: '999px',
                                                    border: '1px solid currentColor',
                                                    fontSize: '0.85rem',
                                                    opacity: 0.85,
                                                }}
                                            >
                                                {n}
                                            </span>
                                        ))}
                                        {hiddenCount > 0 && (
                                            <span style={{ padding: '4px 10px', fontSize: '0.85rem', opacity: 0.7 }}>
                                                +{hiddenCount} more
                                            </span>
                                        )}
                                    </div>
                                    <small className="form-help">
                                        {generatedNumbers.length} unit{generatedNumbers.length === 1 ? '' : 's'} will be created, all vacant.
                                    </small>
                                </>
                            )}
                        </div>

                        <h3>Details shared by every unit</h3>
                    </>
                )}

                <div className="form-row">
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
                </div>

                <div className="form-row">
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

                    {unitType === "other" && (
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
                    )}
                </div>

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

                <div className="form-row">
                    {(unitType === "bedsitter" || unitType === "other") && (
                        <div className="form-group">
                            <label htmlFor="bathrooms">Bathrooms</label>
                            <input
                                id="bathrooms"
                                type="number"
                                value={bathrooms}
                                onChange={(e) => setBathrooms(e.target.value)}
                                placeholder="0"
                                min="0"
                                className={errors.bathrooms ? 'input-error' : ''}
                            />
                            {errors.bathrooms && <span className="error-text">{errors.bathrooms}</span>}
                        </div>
                    )}

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

                <div className="form-row">
                    <div className="form-group">
                        <label htmlFor="monthlyRent">{isBulk ? "Monthly Rent per Unit (KES)" : "Monthly Rent (KES)"}</label>
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
                    <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                        {submitLabel}
                    </button>
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
                title={successDetails.title}
                message={successDetails.message}
                details={successDetails.details}
                onClose={() => setShowSuccessModal(false)}
                actionLabel="Confirmed"
            />
        </div>
    );
}

export default AddUnits;