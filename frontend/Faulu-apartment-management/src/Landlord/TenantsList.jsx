import { useState, useEffect } from 'react'
import CustomSelect from '../components/CustomSelect'
import api from '../services/api'
import './TenantsList.css'

// These three functions exist because tenant data can come from the API
// in several different "shapes" depending on how it was fetched or nested.
// As long as App.jsx's enrichTenantsWithLookup() / normalizeTenant() have
// run, tenants arriving here should already have propertyName/unitNumber
// attached — these fallbacks are defensive, not the primary source of truth.

const getPropertyDisplay = (tenant) => {
    if (!tenant) return 'N/A';
    const propertyName = tenant.propertyName
        || tenant.property?.name
        || tenant.property
        || tenant.buildingName
        || tenant.building?.name
        || tenant.building?.propertyName
        || tenant.buildingId
        || tenant.propertyId;
    return propertyName ? String(propertyName) : 'N/A';
};

const getUnitDisplay = (tenant) => {
    if (!tenant) return 'N/A';
    const unit = tenant.unitNumber
        || tenant.unit?.unitNumber
        || tenant.unit?.name
        || tenant.unitId
        || tenant.unit?.id;
    return unit ? String(unit) : 'N/A';
};

const getRentDisplay = (tenant) => {
    const amt = tenant?.rentAmount ?? tenant?.rent ?? null;
    return (amt != null && !isNaN(Number(amt))) ? `KSh ${Number(amt).toLocaleString()}` : 'N/A';
};

// TenantsList is a presentational component: it does NOT fetch tenant data
// itself. App.jsx fetches tenants from /tenants, enriches them, and passes
// the result down as the `tenants` prop. `properties` and `units` are also
// passed down now so the edit form can offer property/unit reassignment.
function TenantsList({ tenants = [], properties = [], units = [], onUpdateTenant, onDeleteTenant, onNavigate }) {
    const [searchTerm, setSearchTerm] = useState("");
    const [sortBy, setSortBy] = useState("dateAdded");
    const [editingId, setEditingId] = useState(null);
    const [selectedTenant, setSelectedTenant] = useState(null);

    const handleDelete = (id) => {
        if (window.confirm("Are you sure you want to delete this tenant?")) {
            onDeleteTenant(id);
        }
    };

    const handleEdit = (tenant) => {
        setEditingId(tenant.id);
        setSelectedTenant(tenant);
    };

    const handleView = (tenant) => {
        setSelectedTenant(tenant);
    };

    const handleCloseModal = () => {
        setSelectedTenant(null);
        setEditingId(null);
    };

    const handleSaveEdit = (updatedTenant) => {
        onUpdateTenant(updatedTenant);
        setEditingId(null);
        setSelectedTenant(null);
    };

    const filteredTenants = tenants.filter(tenant =>
        `${tenant.firstName || ''} ${tenant.lastName || ''}`.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (tenant.email && tenant.email.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (tenant.phoneNumber && tenant.phoneNumber.includes(searchTerm)) ||
        (tenant.propertyName && tenant.propertyName.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (tenant.unitNumber && tenant.unitNumber.toLowerCase().includes(searchTerm.toLowerCase()))
    );

    const sortedTenants = [...filteredTenants].sort((a, b) => {
        switch (sortBy) {
            case "name":
                return `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`);
            case "rent":
                return (a.rentAmount || 0) - (b.rentAmount || 0);
            case "moveIn":
                return new Date(b.moveInDate || 0) - new Date(a.moveInDate || 0);
            case "dateAdded":
            default:
                return new Date(b.createdAt || 0) - new Date(a.createdAt || 0);
        }
    });

    return (
        <div className="tenants-list-container">
            <div className="tenants-header">
                <h1>Tenants List</h1>
                <p className="tenants-count">Total Tenants: {tenants.length}</p>
            </div>

            <div className="filters-section">
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search by name, email, phone, property, or unit..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="search-input"
                    />
                </div>
                <div className="sort-box">
                    <select
                        value={sortBy}
                        onChange={(e) => setSortBy(e.target.value)}
                        className="sort-select"
                    >
                        <option value="dateAdded">Sort by: Date Added</option>
                        <option value="name">Sort by: Name</option>
                        <option value="rent">Sort by: Rent Amount</option>
                        <option value="moveIn">Sort by: Move-In Date</option>
                    </select>
                </div>
            </div>

            {sortedTenants.length === 0 ? (
                <div className="no-tenants">
                    <p>No tenants found. Try a different search term or add a new tenant.</p>
                </div>
            ) : (
                <div className="table-responsive">
                    <table className="tenants-table">
                        <thead>
                            <tr>
                                <th>Full Name</th>
                                <th>Email</th>
                                <th>Phone</th>
                                <th>Property</th>
                                <th>Unit</th>
                                <th>Monthly Rent</th>
                                <th>Move-In Date</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedTenants.map(tenant => (
                                <tr key={tenant.id}>
                                    <td className="tenant-name">{tenant.firstName} {tenant.lastName}</td>
                                    <td>{tenant.email}</td>
                                    <td>{tenant.phoneNumber}</td>
                                    <td>{getPropertyDisplay(tenant)}</td>
                                    <td className="unit-info">
                                        {getUnitDisplay(tenant)}
                                        {tenant.unitType && <span className="unit-type">{tenant.unitType}</span>}
                                    </td>
                                    <td className="rent-price">
                                        {getRentDisplay(tenant)}
                                    </td>
                                    <td>
                                        {tenant.moveInDate
                                            ? new Date(tenant.moveInDate).toLocaleDateString()
                                            : 'N/A'}
                                    </td>
                                    <td className="actions">
                                        <button className="btn btn-view" onClick={() => handleView(tenant)} title="View details">View</button>
                                        <button className="btn btn-edit" onClick={() => handleEdit(tenant)} title="Edit tenant">Edit</button>
                                        <button className="btn btn-delete" onClick={() => handleDelete(tenant.id)} title="Delete tenant">Delete</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {selectedTenant && (
                <div className="modal-overlay" onClick={handleCloseModal}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2>{editingId ? "Edit Tenant" : "Tenant Details"}</h2>
                            <button className="close-btn" onClick={handleCloseModal}>×</button>
                        </div>
                        <div className="modal-body">
                            {editingId ? (
                                <EditTenantForm
                                    tenant={selectedTenant}
                                    properties={properties}
                                    units={units}
                                    onSave={handleSaveEdit}
                                    onCancel={handleCloseModal}
                                />
                            ) : (
                                <TenantDetails tenant={selectedTenant} />
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

function TenantDetails({ tenant }) {
    return (
        <div className="tenant-details">
            <div className="detail-row">
                <span className="detail-label">First Name:</span>
                <span className="detail-value">{tenant.firstName}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Last Name:</span>
                <span className="detail-value">{tenant.lastName}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Email:</span>
                <span className="detail-value">{tenant.email}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Phone Number:</span>
                <span className="detail-value">{tenant.phoneNumber}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">ID Number:</span>
                <span className="detail-value">{tenant.identificationNumber}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Property:</span>
                <span className="detail-value">{getPropertyDisplay(tenant)}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Unit:</span>
                <span className="detail-value">
                    {getUnitDisplay(tenant)} {tenant.unitType && `(${tenant.unitType})`}
                </span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Move-In Date:</span>
                <span className="detail-value">
                    {tenant.moveInDate ? new Date(tenant.moveInDate).toLocaleDateString() : 'N/A'}
                </span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Monthly Rent:</span>
                <span className="detail-value">
                    {getRentDisplay(tenant)}
                </span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Status:</span>
                <span className="detail-value">{tenant.status}</span>
            </div>
        </div>
    );
}

function EditTenantForm({ tenant, properties, units, onSave, onCancel }) {
    const [formData, setFormData] = useState(tenant);
    const [errors, setErrors] = useState({});

    // propertyId/unitId drive the dropdowns. Seeded from whatever the tenant
    // currently has (propertyId or buildingId — data has come through both
    // names depending on the source, so we check both).
    const [propertyId, setPropertyId] = useState(
        tenant.propertyId ?? tenant.buildingId ?? ""
    );
    const [unitId, setUnitId] = useState(tenant.unitId ?? "");

    // Units available for whichever property is currently selected.
    // Seeded with the full `units` list filtered to the tenant's current
    // property, then replaced by a fresh fetch if the user picks a
    // DIFFERENT property (same pattern as AddTenant.jsx).
    const [availableUnits, setAvailableUnits] = useState(
        units.filter(u => String(u.buildingId ?? u.propertyId) === String(propertyId))
    );
    const [loadingUnits, setLoadingUnits] = useState(false);

    // Track the property the form was opened with, so we only re-fetch
    // units when the user actually CHANGES the property — not on first render.
    const [initialPropertyId] = useState(propertyId);

    useEffect(() => {
        if (!propertyId || String(propertyId) === String(initialPropertyId)) {
            // Nothing selected yet, or still the tenant's original property —
            // no need to re-fetch, we already seeded availableUnits above.
            return;
        }

        let isMounted = true;
        const fetchUnits = async () => {
            setLoadingUnits(true);
            try {
                const res = await api.get(`/units/building/${propertyId}`);
                if (isMounted) {
                    setAvailableUnits(res.data?.success ? res.data.data : []);
                }
            } catch (err) {
                console.error('Fetch units error:', err);
                if (isMounted) setAvailableUnits([]);
            } finally {
                if (isMounted) setLoadingUnits(false);
            }
        };
        fetchUnits();

        return () => { isMounted = false; };
    }, [propertyId, initialPropertyId]);

    const propertyOptions = properties.map(p => ({
        value: p.id,
        // Building entity only has Name/Address/City — no propertyName/location.
        label: `${p.name || p.propertyName || 'Unnamed property'} - ${p.city || p.address || p.location || ''}`
    }));

    const unitOptions = loadingUnits
        ? [{ value: '', label: 'Loading units...' }]
        : availableUnits.map(u => ({
            value: u.id,
            label: `${u.unitNumber} (${u.unitType || 'Unit'})`
        }));

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'rentAmount' ? parseFloat(value) : value
        }));
    };

    const handlePropertyChange = (val) => {
        setPropertyId(val);
        setUnitId(""); // reset unit whenever property changes — old unit doesn't belong to new property
    };

    const validateForm = () => {
        const newErrors = {};
        if (!formData.firstName?.trim()) newErrors.firstName = "First name is required";
        if (!formData.lastName?.trim()) newErrors.lastName = "Last name is required";
        if (!formData.email?.trim()) newErrors.email = "Email is required";
        if (!formData.phoneNumber?.trim()) newErrors.phoneNumber = "Phone number is required";
        if (!formData.rentAmount || formData.rentAmount <= 0) newErrors.rentAmount = "Rent amount must be a positive number";
        if (!propertyId) newErrors.propertyId = "Property selection is required";
        if (!unitId) newErrors.unitId = "Unit selection is required";
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (!validateForm()) return;

        const selectedProperty = properties.find(p => String(p.id) === String(propertyId));
        const selectedUnit = availableUnits.find(u => String(u.id) === String(unitId));

        // Send both the edited fields AND the (possibly changed) property/unit,
        // plus the ORIGINAL unit id so the backend can free it up if it changed.
        onSave({
            ...formData,
            propertyId: parseInt(propertyId),
            buildingId: parseInt(propertyId),
            propertyName: selectedProperty?.propertyName || selectedProperty?.name || '',
            unitId: parseInt(unitId),
            unitNumber: selectedUnit?.unitNumber || '',
            unitType: selectedUnit?.unitType || '',
            previousUnitId: tenant.unitId ?? null, // lets the backend vacate the old unit if it's different
        });
    };

    return (
        <form className="edit-form" onSubmit={handleSubmit}>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>First Name:</label>
                    <input type="text" name="firstName" value={formData.firstName || ''} onChange={handleChange} className={errors.firstName ? 'input-error' : ''} />
                    {errors.firstName && <span className="error-text">{errors.firstName}</span>}
                </div>
                <div className="form-group">
                    <label>Last Name:</label>
                    <input type="text" name="lastName" value={formData.lastName || ''} onChange={handleChange} className={errors.lastName ? 'input-error' : ''} />
                    {errors.lastName && <span className="error-text">{errors.lastName}</span>}
                </div>
            </div>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Email:</label>
                    <input type="email" name="email" value={formData.email || ''} onChange={handleChange} className={errors.email ? 'input-error' : ''} />
                    {errors.email && <span className="error-text">{errors.email}</span>}
                </div>
                <div className="form-group">
                    <label>Phone Number:</label>
                    <input type="tel" name="phoneNumber" value={formData.phoneNumber || ''} onChange={handleChange} className={errors.phoneNumber ? 'input-error' : ''} />
                    {errors.phoneNumber && <span className="error-text">{errors.phoneNumber}</span>}
                </div>
            </div>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Property:</label>
                    <CustomSelect
                        options={propertyOptions}
                        value={propertyId}
                        onChange={handlePropertyChange}
                        placeholder="-- Select a Property --"
                    />
                    {errors.propertyId && <span className="error-text">{errors.propertyId}</span>}
                </div>
                <div className="form-group">
                    <label>Unit:</label>
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
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Monthly Rent:</label>
                    <input type="number" name="rentAmount" value={formData.rentAmount || ''} onChange={handleChange} className={errors.rentAmount ? 'input-error' : ''} />
                    {errors.rentAmount && <span className="error-text">{errors.rentAmount}</span>}
                </div>
                <div className="form-group">
                    <label>Move-In Date:</label>
                    <input type="date" name="moveInDate" value={formData.moveInDate || ''} onChange={handleChange} />
                </div>
            </div>
            <div className="form-buttons">
                <button type="submit" className="save-btn">Save Changes</button>
                <button type="button" className="cancel-btn" onClick={onCancel}>Cancel</button>
            </div>
        </form>
    );
}

export default TenantsList;