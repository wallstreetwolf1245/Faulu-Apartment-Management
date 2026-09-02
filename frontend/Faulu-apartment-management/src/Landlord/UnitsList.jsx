import { useState } from 'react'
import './UnitsList.css'

function UnitsList({ property, units, onDeleteUnit, onUpdateUnit, onNavigate }) {
    const [editingUnit, setEditingUnit] = useState(null);
    const [searchTerm, setSearchTerm] = useState("");
    const [filterStatus, setFilterStatus] = useState("all");

    // Normalize a unit from backend DTO field names to frontend-friendly names
    const normalizeUnit = (unit) => ({
        ...unit,
        bedrooms: unit.bedrooms ?? unit.bedroomCount ?? 0,
        bathrooms: unit.bathrooms ?? unit.bathroomCount ?? 0,
        squareFeet: unit.squareFeet ?? unit.squareFootage ?? null,
        unitRent: unit.unitRent ?? unit.monthlyRent ?? 0,
        status: (unit.status ?? 'vacant').toLowerCase(),
        amenities: Array.isArray(unit.amenities)
            ? unit.amenities
            : typeof unit.amenities === 'string' && unit.amenities.trim()
                ? unit.amenities.split(',').map(a => a.trim()).filter(Boolean)
                : [],
    });

    const filteredUnits = units.map(normalizeUnit).filter(unit => {
        const matchesSearch = unit.unitNumber.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesStatus = filterStatus === "all" || unit.status === filterStatus;
        return matchesSearch && matchesStatus;
    });

    const handleEdit = (unit) => {
        setEditingUnit({ ...unit });
    };

    const handleSaveEdit = (updatedUnit) => {
        // Map back to the shape the backend/parent expects
        onUpdateUnit({
            ...updatedUnit,
            bedroomCount: updatedUnit.bedrooms,
            bathroomCount: updatedUnit.bathrooms,
            squareFootage: updatedUnit.squareFeet,
            monthlyRent: updatedUnit.unitRent,
            amenities: Array.isArray(updatedUnit.amenities)
                ? updatedUnit.amenities.join(', ')
                : updatedUnit.amenities,
        });
        setEditingUnit(null);
    };

    const handleDeleteUnit = (unitId) => {
        if (window.confirm("Are you sure you want to delete this unit?")) {
            onDeleteUnit(unitId);
        }
    };

    const getStatusBadge = (status) => {
        const statusColors = {
            vacant: { bg: '#d4edda', color: '#155724' },
            occupied: { bg: '#cfe2ff', color: '#084298' },
            maintenance: { bg: '#fff3cd', color: '#664d03' },
            reserved: { bg: '#f8d7da', color: '#842029' }
        };
        return statusColors[status] || statusColors.vacant;
    };

    const totalUnits = units.length;
    const occupiedUnits = units.filter(u => (u.status ?? '').toLowerCase() === 'occupied').length;
    const vacantUnits = units.filter(u => (u.status ?? '').toLowerCase() === 'vacant').length;

    return (
        <div className="units-list-container">
            <div className="units-header">
                <div>
                    <h2>{property?.propertyName}</h2>
                    <p className="property-info">{property?.location} • {totalUnits} units</p>
                </div>
                <button
                    className="btn btn-primary"
                    onClick={() => onNavigate('addUnits')}
                >
                    ➕ Add Unit
                </button>
            </div>

            <div className="units-stats">
                <div className="stat-card">
                    <span className="stat-value">{totalUnits}</span>
                    <span className="stat-label">Total Units</span>
                </div>
                <div className="stat-card">
                    <span className="stat-value">{occupiedUnits}</span>
                    <span className="stat-label">Occupied</span>
                </div>
                <div className="stat-card">
                    <span className="stat-value">{vacantUnits}</span>
                    <span className="stat-label">Vacant</span>
                </div>
                <div className="stat-card">
                    <span className="stat-value">{totalUnits > 0 ? Math.round((occupiedUnits / totalUnits) * 100) : 0}%</span>
                    <span className="stat-label">Occupancy Rate</span>
                </div>
            </div>

            <div className="units-filters">
                <input
                    type="text"
                    placeholder="Search by unit number..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="search-input"
                />
                <select
                    value={filterStatus}
                    onChange={(e) => setFilterStatus(e.target.value)}
                    className="filter-select"
                >
                    <option value="all">All Status</option>
                    <option value="vacant">Vacant</option>
                    <option value="occupied">Occupied</option>
                    <option value="maintenance">Under Maintenance</option>
                    <option value="reserved">Reserved</option>
                </select>
            </div>

            {filteredUnits.length === 0 ? (
                <div className="empty-state">
                    <p>🏠 No units found. Start by adding your first unit!</p>
                </div>
            ) : (
                <div className="units-table-wrapper">
                    <table className="units-table">
                        <thead>
                            <tr>
                                <th>Unit Number</th>
                                <th>Beds/Baths</th>
                                <th>Sq. Feet</th>
                                <th>Monthly Rent</th>
                                <th>Status</th>
                                <th>Amenities</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredUnits.map(unit => (
                                <tr key={unit.id}>
                                    <td className="unit-number">{unit.unitNumber}</td>
                                    <td>{unit.bedrooms} / {unit.bathrooms}</td>
                                    <td>{unit.squareFeet != null ? Number(unit.squareFeet).toLocaleString() : '—'}</td>
                                    <td className="rent">${parseFloat(unit.unitRent).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                                    <td>
                                        <span
                                            className="status-badge"
                                            style={{
                                                backgroundColor: getStatusBadge(unit.status).bg,
                                                color: getStatusBadge(unit.status).color
                                            }}
                                        >
                                            {unit.status.charAt(0).toUpperCase() + unit.status.slice(1)}
                                        </span>
                                    </td>
                                    <td className="amenities">
                                        {unit.amenities.length > 0
                                            ? unit.amenities.slice(0, 2).join(', ') + (unit.amenities.length > 2 ? '...' : '')
                                            : '—'
                                        }
                                    </td>
                                    <td className="actions">
                                        <button
                                            className="btn-action btn-view"
                                            onClick={() => handleEdit(unit)}
                                            title="View/Edit"
                                        >
                                            👁️
                                        </button>
                                        <button
                                            className="btn-action btn-delete"
                                            onClick={() => handleDeleteUnit(unit.id)}
                                            title="Delete"
                                        >
                                            🗑️
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {editingUnit && (
                <div className="modal-overlay" onClick={() => setEditingUnit(null)}>
                    <div className="modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h3>Edit Unit {editingUnit.unitNumber}</h3>
                            <button
                                className="close-btn"
                                onClick={() => setEditingUnit(null)}
                            >
                                ✕
                            </button>
                        </div>

                        <div className="modal-content">
                            <div className="detail-row">
                                <div className="detail-item">
                                    <label>Bedrooms</label>
                                    <input
                                        type="number"
                                        value={editingUnit.bedrooms}
                                        onChange={(e) => setEditingUnit({ ...editingUnit, bedrooms: parseInt(e.target.value) })}
                                        min="0"
                                    />
                                </div>
                                <div className="detail-item">
                                    <label>Bathrooms</label>
                                    <input
                                        type="number"
                                        value={editingUnit.bathrooms}
                                        onChange={(e) => setEditingUnit({ ...editingUnit, bathrooms: parseFloat(e.target.value) })}
                                        step="0.5"
                                        min="0"
                                    />
                                </div>
                            </div>

                            <div className="detail-row">
                                <div className="detail-item">
                                    <label>Square Feet</label>
                                    <input
                                        type="number"
                                        value={editingUnit.squareFeet ?? ''}
                                        onChange={(e) => setEditingUnit({ ...editingUnit, squareFeet: parseFloat(e.target.value) })}
                                        min="0"
                                    />
                                </div>
                                <div className="detail-item">
                                    <label>Monthly Rent</label>
                                    <input
                                        type="number"
                                        value={editingUnit.unitRent}
                                        onChange={(e) => setEditingUnit({ ...editingUnit, unitRent: parseFloat(e.target.value) })}
                                        step="0.01"
                                        min="0"
                                    />
                                </div>
                            </div>

                            <div className="detail-item">
                                <label>Status</label>
                                <select
                                    value={editingUnit.status}
                                    onChange={(e) => setEditingUnit({ ...editingUnit, status: e.target.value })}
                                >
                                    <option value="vacant">Vacant</option>
                                    <option value="occupied">Occupied</option>
                                    <option value="maintenance">Under Maintenance</option>
                                    <option value="reserved">Reserved</option>
                                </select>
                            </div>

                            <div className="detail-item">
                                <label>Amenities</label>
                                <textarea
                                    value={Array.isArray(editingUnit.amenities) ? editingUnit.amenities.join(', ') : (editingUnit.amenities ?? '')}
                                    onChange={(e) => setEditingUnit({
                                        ...editingUnit,
                                        amenities: e.target.value.split(',').map(a => a.trim()).filter(Boolean)
                                    })}
                                    placeholder="e.g., Air Conditioning, Balcony, In-unit Laundry"
                                    rows="3"
                                />
                            </div>
                        </div>

                        <div className="modal-footer">
                            <button
                                className="btn btn-secondary"
                                onClick={() => setEditingUnit(null)}
                            >
                                Cancel
                            </button>
                            <button
                                className="btn btn-primary"
                                onClick={() => handleSaveEdit(editingUnit)}
                            >
                                Save Changes
                            </button>
                        </div>
                    </div>
                </div>
            )}

            <div className="units-footer">
                <button
                    className="btn btn-secondary"
                    onClick={() => onNavigate('propertiesList')}
                >
                    ← Back to Properties
                </button>
            </div>
        </div>
    );
}

export default UnitsList;