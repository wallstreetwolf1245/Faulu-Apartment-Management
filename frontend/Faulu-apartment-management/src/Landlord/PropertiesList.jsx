import { useState } from 'react'
import './PropertiesList.css'

function PropertiesList({ properties = [], onUpdateProperty, onDeleteProperty, onNavigate, onViewUnits }) {
    const [searchTerm, setSearchTerm] = useState("");
    const [sortBy, setSortBy] = useState("dateAdded");
    const [editingId, setEditingId] = useState(null);
    const [selectedProperty, setSelectedProperty] = useState(null);

    const handleDelete = (id) => {
        if (window.confirm("Are you sure you want to delete this property?")) {
            onDeleteProperty(id);
        }
    };

    const handleEdit = (property) => {
        setEditingId(property.id);
        setSelectedProperty(property);
    };

    const handleView = (property) => {
        setSelectedProperty(property);
    };

    const handleCloseModal = () => {
        setSelectedProperty(null);
        setEditingId(null);
    };

    const handleSaveEdit = (updatedProperty) => {
        onUpdateProperty(updatedProperty);
        setEditingId(null);
        setSelectedProperty(null);
    };

    const filteredProperties = properties.filter(property =>
        (property.name || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
        (property.address || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
        (property.city || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
        (property.description || '').toLowerCase().includes(searchTerm.toLowerCase())
    );

    const sortedProperties = [...filteredProperties].sort((a, b) => {
        switch (sortBy) {
            case "name":
                return (a.name || '').localeCompare(b.name || '');
            case "city":
                return (a.city || '').localeCompare(b.city || '');
            case "propertyValue":
                return (a.propertyValue || 0) - (b.propertyValue || 0);
            case "dateAdded":
            default:
                return new Date(b.createdAt) - new Date(a.createdAt);
        }
    });

    return (
        <div className="properties-list-container">
            <div className="properties-header">
                <h1>Properties </h1>
                <p className="property-count">Total Properties: {properties.length}</p>
            </div>

            <div className="filters-section">
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search by name, address, or city..."
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
                        <option value="propertyValue">Sort by: Property Value</option>
                        <option value="city">Sort by: City</option>
                    </select>
                </div>
            </div>

            {sortedProperties.length === 0 ? (
                <div className="no-properties">
                    <p>No properties found. Try a different search term.</p>
                </div>
            ) : (
                <div className="table-responsive">
                    <table className="properties-table">
                        <thead>
                            <tr>
                                <th>Property Name</th>
                                <th>Address</th>
                                <th>City</th>
                                <th>Total Units</th>
                                <th>Date Added</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedProperties.map(property => (
                                <tr key={property.id}>
                                    <td className="property-name">{property.name}</td>
                                    <td>{property.address}</td>
                                    <td>{property.city}</td>
                                    <td>{property.totalUnits}</td>
                                    <td>{new Date(property.createdAt).toLocaleDateString()}</td>
                                    <td className="actions">
                                        <button
                                            className="btn btn-view"
                                            onClick={() => handleView(property)}
                                            title="View details"
                                        >
                                            View
                                        </button>
                                        <button
                                            className="btn btn-units"
                                            onClick={() => onViewUnits(property.id)}
                                            title="Manage units"
                                        >
                                            Units
                                        </button>
                                        <button
                                            className="btn btn-edit"
                                            onClick={() => handleEdit(property)}
                                            title="Edit property"
                                        >
                                            Edit
                                        </button>
                                        <button
                                            className="btn btn-delete"
                                            onClick={() => handleDelete(property.id)}
                                            title="Delete property"
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {selectedProperty && (
                <div className="modal-overlay" onClick={handleCloseModal}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2>{editingId ? "Edit Property" : "Property Details"}</h2>
                            <button className="close-btn" onClick={handleCloseModal}>×</button>
                        </div>
                        <div className="modal-body">
                            {editingId ? (
                                <EditPropertyForm
                                    property={selectedProperty}
                                    onSave={handleSaveEdit}
                                    onCancel={handleCloseModal}
                                />
                            ) : (
                                <PropertyDetails property={selectedProperty} />
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

function PropertyDetails({ property }) {
    return (
        <div className="property-details">
            <div className="detail-row">
                <span className="detail-label">Property Name:</span>
                <span className="detail-value">{property.name}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Address:</span>
                <span className="detail-value">{property.address}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">City:</span>
                <span className="detail-value">{property.city}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Postal Code:</span>
                <span className="detail-value">{property.postalCode}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Total Units:</span>
                <span className="detail-value">{property.totalUnits}</span>
            </div>
            {property.propertyValue && (
                <div className="detail-row">
                    <span className="detail-label">Property Value:</span>
                    <span className="detail-value">KES {property.propertyValue?.toLocaleString()}</span>
                </div>
            )}
            {property.description && (
                <div className="detail-row">
                    <span className="detail-label">Description:</span>
                    <span className="detail-value">{property.description}</span>
                </div>
            )}
            <div className="detail-row">
                <span className="detail-label">Date Added:</span>
                <span className="detail-value">{new Date(property.createdAt).toLocaleDateString()}</span>
            </div>
        </div>
    );
}

function EditPropertyForm({ property, onSave, onCancel }) {
    const [formData, setFormData] = useState(property);
    const [errors, setErrors] = useState({});

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'totalUnits' || name === 'propertyValue' ? parseFloat(value) : value
        }));
    };

    const validateForm = () => {
        const newErrors = {};
        if (!formData.name?.trim()) newErrors.name = "Property name is required";
        if (!formData.address?.trim()) newErrors.address = "Address is required";
        if (!formData.city?.trim()) newErrors.city = "City is required";
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validateForm()) {
            onSave(formData);
        }
    };

    return (
        <form className="edit-form" onSubmit={handleSubmit}>
            <div className="form-group">
                <label>Property Name:</label>
                <input
                    type="text"
                    name="name"
                    value={formData.name || ''}
                    onChange={handleChange}
                    className={errors.name ? 'input-error' : ''}
                />
                {errors.name && <span className="error-text">{errors.name}</span>}
            </div>
            <div className="form-group">
                <label>Address:</label>
                <input
                    type="text"
                    name="address"
                    value={formData.address || ''}
                    onChange={handleChange}
                    className={errors.address ? 'input-error' : ''}
                />
                {errors.address && <span className="error-text">{errors.address}</span>}
            </div>
            <div className="form-group">
                <label>City:</label>
                <input
                    type="text"
                    name="city"
                    value={formData.city || ''}
                    onChange={handleChange}
                    className={errors.city ? 'input-error' : ''}
                />
                {errors.city && <span className="error-text">{errors.city}</span>}
            </div>
            <div className="form-group">
                <label>Postal Code:</label>
                <input
                    type="text"
                    name="postalCode"
                    value={formData.postalCode || ''}
                    onChange={handleChange}
                />
            </div>
            <div className="form-group">
                <label>Total Units:</label>
                <input
                    type="number"
                    name="totalUnits"
                    value={formData.totalUnits || ''}
                    onChange={handleChange}
                    min="1"
                />
            </div>
            <div className="form-group">
                <label>Property Value (KES):</label>
                <input
                    type="number"
                    name="propertyValue"
                    value={formData.propertyValue || ''}
                    onChange={handleChange}
                    min="0"
                />
            </div>
            <div className="form-group">
                <label>Description:</label>
                <textarea
                    name="description"
                    value={formData.description || ''}
                    onChange={handleChange}
                />
            </div>
            <div className="form-buttons">
                <button type="submit" className="save-btn">Save Changes</button>
                <button type="button" className="cancel-btn" onClick={onCancel}>Cancel</button>
            </div>
        </form>
    );
}

export default PropertiesList;