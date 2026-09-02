import { useState } from 'react'
import './MaintenanceList.css'

function getPropertyName(properties, buildingId) {
    const p = properties.find(pp => String(pp.id) === String(buildingId))
    return p ? (p.name || p.title || p.propertyName || p.address || `Property #${buildingId}`) : (buildingId ? `Property #${buildingId}` : 'Unknown property')
}

function getTenantName(tenants, tenantId) {
    if (!tenantId) return 'Unassigned'
    const t = tenants.find(tt => String(tt.id) === String(tenantId))
    return t ? `${t.firstName || ''} ${t.lastName || ''}`.trim() || `Tenant #${tenantId}` : `Tenant #${tenantId}`
}

function MaintenanceList({ maintenance = [], properties = [], tenants = [], onUpdateMaintenance, onDeleteMaintenance, onNavigate }) {
    const [searchTerm, setSearchTerm] = useState("");
    const [filterStatus, setFilterStatus] = useState("all");
    const [sortBy, setSortBy] = useState("reportedDate");
    const [editingId, setEditingId] = useState(null);
    const [selectedMaintenance, setSelectedMaintenance] = useState(null);

    const handleDelete = (id) => {
        if (window.confirm("Are you sure you want to delete this maintenance request?")) {
            onDeleteMaintenance(id);
        }
    };

    const handleEdit = (item) => {
        setEditingId(item.id);
        setSelectedMaintenance(item);
    };

    const handleView = (item) => {
        setSelectedMaintenance(item);
    };

    const handleCloseModal = () => {
        setSelectedMaintenance(null);
        setEditingId(null);
    };

    const handleSaveEdit = (updatedMaintenance) => {
        onUpdateMaintenance(updatedMaintenance);
        setEditingId(null);
        setSelectedMaintenance(null);
    };

    const filteredMaintenance = maintenance.filter(item => {
        const propertyName = getPropertyName(properties, item.buildingId);
        const tenantName = getTenantName(tenants, item.tenantId);
        const term = searchTerm.toLowerCase();
        const matchesSearch = !term ||
            propertyName.toLowerCase().includes(term) ||
            tenantName.toLowerCase().includes(term) ||
            (item.title || '').toLowerCase().includes(term) ||
            (item.category || '').toLowerCase().includes(term);
        const matchesStatus = filterStatus === 'all' || item.status === filterStatus;
        return matchesSearch && matchesStatus;
    });

    const sortedMaintenance = [...filteredMaintenance].sort((a, b) => {
        switch(sortBy) {
            case "priority": {
                const priorityOrder = { Urgent: 0, High: 1, Normal: 2, Low: 3 };
                return (priorityOrder[a.priority] ?? 99) - (priorityOrder[b.priority] ?? 99);
            }
            case "cost":
                return (b.estimatedCost || 0) - (a.estimatedCost || 0);
            case "reportedDate":
            default:
                return new Date(b.reportedDate) - new Date(a.reportedDate);
        }
    });

    const getPriorityClass = (priority) => {
        return `priority-${(priority || '').toLowerCase()}`;
    };

    const getStatusClass = (status) => {
        return `status-${(status || '').toLowerCase()}`;
    };

    return (
        <div className="maintenance-list-container">
            <div className="maintenance-header">
                <h1>Maintenance Requests</h1>
                <p className="maintenance-count">Total Requests: {maintenance.length}</p>
            </div>

            <div className="filters-section">
                <div className="search-box">
                    <input 
                        type="text" 
                        placeholder="Search by property, tenant, or title..." 
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="search-input"
                    />
                </div>
                <div className="filter-box">
                    <select 
                        value={filterStatus}
                        onChange={(e) => setFilterStatus(e.target.value)}
                        className="filter-select"
                    >
                        <option value="all">All Status</option>
                        <option value="Open">Open</option>
                        <option value="InProgress">In Progress</option>
                        <option value="OnHold">On Hold</option>
                        <option value="Completed">Completed</option>
                        <option value="Cancelled">Cancelled</option>
                    </select>
                </div>
                <div className="sort-box">
                    <select 
                        value={sortBy}
                        onChange={(e) => setSortBy(e.target.value)}
                        className="sort-select"
                    >
                        <option value="reportedDate">Sort by: Report Date</option>
                        <option value="priority">Sort by: Priority</option>
                        <option value="cost">Sort by: Cost</option>
                    </select>
                </div>
            </div>

            {sortedMaintenance.length === 0 ? (
                <div className="no-maintenance">
                    <p>No maintenance requests found. Try a different search or add a new request.</p>
                </div>
            ) : (
                <div className="table-responsive">
                    <table className="maintenance-table">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Property</th>
                                <th>Tenant</th>
                                <th>Category</th>
                                <th>Priority</th>
                                <th>Status</th>
                                <th>Estimated Cost</th>
                                <th>Report Date</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedMaintenance.map(item => (
                                <tr key={item.id}>
                                    <td>{item.title}</td>
                                    <td className="property-name">{getPropertyName(properties, item.buildingId)}</td>
                                    <td>{getTenantName(tenants, item.tenantId)}</td>
                                    <td>{item.category}</td>
                                    <td><span className={`priority-badge ${getPriorityClass(item.priority)}`}>{item.priority}</span></td>
                                    <td><span className={`status-badge ${getStatusClass(item.status)}`}>{item.status}</span></td>
                                    <td className="cost">{item.estimatedCost != null ? `KES ${Number(item.estimatedCost).toLocaleString()}` : '—'}</td>
                                    <td>{item.reportedDate ? new Date(item.reportedDate).toLocaleDateString() : '—'}</td>
                                    <td className="actions">
                                        <button 
                                            className="btn btn-view"
                                            onClick={() => handleView(item)}
                                            title="View details"
                                        >
                                            View
                                        </button>
                                        <button 
                                            className="btn btn-edit"
                                            onClick={() => handleEdit(item)}
                                            title="Edit request"
                                        >
                                            Edit
                                        </button>
                                        <button 
                                            className="btn btn-delete"
                                            onClick={() => handleDelete(item.id)}
                                            title="Delete request"
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

            {selectedMaintenance && (
                <div className="modal-overlay" onClick={handleCloseModal}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h2>{editingId ? "Edit Maintenance Request" : "Maintenance Request Details"}</h2>
                            <button className="close-btn" onClick={handleCloseModal}>×</button>
                        </div>
                        <div className="modal-body">
                            {editingId ? (
                                <EditMaintenanceForm 
                                    maintenance={selectedMaintenance}
                                    properties={properties}
                                    tenants={tenants}
                                    onSave={handleSaveEdit}
                                    onCancel={handleCloseModal}
                                />
                            ) : (
                                <MaintenanceDetails maintenance={selectedMaintenance} properties={properties} tenants={tenants} />
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

function MaintenanceDetails({ maintenance, properties, tenants }) {
    return (
        <div className="maintenance-details">
            <div className="detail-row">
                <span className="detail-label">Property:</span>
                <span className="detail-value">{getPropertyName(properties, maintenance.buildingId)}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Tenant:</span>
                <span className="detail-value">{getTenantName(tenants, maintenance.tenantId)}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Title:</span>
                <span className="detail-value">{maintenance.title}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Category:</span>
                <span className="detail-value">{maintenance.category}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Priority:</span>
                <span className={`detail-value priority-${(maintenance.priority || '').toLowerCase()}`}>{maintenance.priority}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Status:</span>
                <span className={`detail-value status-${(maintenance.status || '').toLowerCase()}`}>{maintenance.status}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Description:</span>
                <span className="detail-value">{maintenance.description}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Report Date:</span>
                <span className="detail-value">{maintenance.reportedDate ? new Date(maintenance.reportedDate).toLocaleDateString() : '—'}</span>
            </div>
            <div className="detail-row">
                <span className="detail-label">Estimated Cost:</span>
                <span className="detail-value">{maintenance.estimatedCost != null ? `KES ${Number(maintenance.estimatedCost).toLocaleString()}` : '—'}</span>
            </div>
            {maintenance.assignedVendor && (
                <div className="detail-row">
                    <span className="detail-label">Assigned Vendor:</span>
                    <span className="detail-value">{maintenance.assignedVendor}</span>
                </div>
            )}
        </div>
    );
}

function EditMaintenanceForm({ maintenance, properties, tenants, onSave, onCancel }) {
    const [formData, setFormData] = useState(maintenance);
    const [errors, setErrors] = useState({});

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
    ];

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'estimatedCost' ? (value === '' ? null : parseFloat(value)) : value
        }));
    };

    const validateForm = () => {
        const newErrors = {};
        if (!formData.title || !formData.title.trim()) {
            newErrors.title = "Title is required";
        }
        if (!formData.category) {
            newErrors.category = "Category is required";
        }
        if (!formData.description || !formData.description.trim()) {
            newErrors.description = "Description is required";
        }
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
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Property:</label>
                    <div className="info-text">{getPropertyName(properties, formData.buildingId)}</div>
                </div>
                <div className="form-group">
                    <label>Tenant:</label>
                    <div className="info-text">{getTenantName(tenants, formData.tenantId)}</div>
                </div>
            </div>
            <div className="form-group">
                <label>Title:</label>
                <input
                    type="text"
                    name="title"
                    value={formData.title || ''}
                    onChange={handleChange}
                    className={errors.title ? 'input-error' : ''}
                />
                {errors.title && <span className="error-text">{errors.title}</span>}
            </div>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Category:</label>
                    <select 
                        name="category"
                        value={formData.category || ''}
                        onChange={handleChange}
                        className={errors.category ? 'input-error' : ''}
                    >
                        <option value="">Select category</option>
                        {issueTypes.map(type => (
                            <option key={type} value={type}>{type}</option>
                        ))}
                    </select>
                    {errors.category && <span className="error-text">{errors.category}</span>}
                </div>
                <div className="form-group">
                    <label>Priority:</label>
                    <select 
                        name="priority"
                        value={formData.priority || 'Normal'}
                        onChange={handleChange}
                    >
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
                    name="description"
                    value={formData.description || ''}
                    onChange={handleChange}
                    className={errors.description ? 'input-error' : ''}
                />
                {errors.description && <span className="error-text">{errors.description}</span>}
            </div>
            <div className="form-row-edit">
                <div className="form-group">
                    <label>Status:</label>
                    <select 
                        name="status"
                        value={formData.status || 'Open'}
                        onChange={handleChange}
                    >
                        <option value="Open">Open</option>
                        <option value="InProgress">In Progress</option>
                        <option value="OnHold">On Hold</option>
                        <option value="Completed">Completed</option>
                        <option value="Cancelled">Cancelled</option>
                    </select>
                </div>
                <div className="form-group">
                    <label>Estimated Cost (KES):</label>
                    <input 
                        type="number"
                        name="estimatedCost"
                        value={formData.estimatedCost ?? ''}
                        onChange={handleChange}
                        step="0.01"
                    />
                </div>
            </div>
            <div className="form-group">
                <label>Assigned Vendor:</label>
                <input
                    type="text"
                    name="assignedVendor"
                    value={formData.assignedVendor || ''}
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

export default MaintenanceList;