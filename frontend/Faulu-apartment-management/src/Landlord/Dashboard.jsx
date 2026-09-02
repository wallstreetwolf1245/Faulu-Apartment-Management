import './Dashboard.css'

function Dashboard({ properties, tenants, maintenance, payments }) {
    const totalProperties = properties.length;
    const totalTenants = tenants.length;
    const totalMaintenance = maintenance.length;
    const totalPayments = payments.length;

    const pendingMaintenance = maintenance.filter(m => m.status === 'Open').length;
    const completedMaintenance = maintenance.filter(m => m.status === 'Completed').length;
    const inProgressMaintenance = maintenance.filter(m => m.status === 'InProgress').length;

    const completedPayments = payments.filter(p => p.status === 'Completed').length;
    const pendingPayments = payments.filter(p => p.status === 'Pending').length;
    const failedPayments = payments.filter(p => p.status === 'Failed').length;

    const totalPropertyValue = properties.reduce((sum, p) => sum + (p.propertyValue || 0), 0);
    const monthlyPaymentsTotal = payments.reduce((sum, p) => sum + (p.amount || 0), 0);
    const maintenanceCostTotal = maintenance.reduce((sum, m) => sum + (m.estimatedCost || 0), 0);

    const urgentMaintenance = maintenance.filter(m => m.priority === 'Urgent').length;

    const recentProperties = [...properties].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 5);
    const recentTenants = [...tenants].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 5);
    const recentMaintenance = [...maintenance].sort((a, b) => new Date(b.reportedDate) - new Date(a.reportedDate)).slice(0, 5);
    const recentPayments = [...payments].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 5);

    return (
        <div className="dashboard-container">
            <div className="dashboard-header">
                <h1>Dashboard</h1>
                <p className="dashboard-subtitle">Welcome back! Here's your property management overview.</p>
            </div>

            <div className="summary-section">
                <div className="summary-card properties">
                    <div className="card-content">
                        <h3>Total Properties</h3>
                        <p className="card-value">{totalProperties}</p>
                        <span className="card-subtext">Total Value: KES {totalPropertyValue.toLocaleString()}</span>
                    </div>
                </div>

                <div className="summary-card tenants">
                    <div className="card-content">
                        <h3>Total Tenants</h3>
                        <p className="card-value">{totalTenants}</p>
                        <span className="card-subtext">Active residents</span>
                    </div>
                </div>

                <div className="summary-card maintenance">
                    <div className="card-content">
                        <h3>Maintenance</h3>
                        <p className="card-value">{totalMaintenance}</p>
                        <span className="card-subtext urgent">{urgentMaintenance} Urgent</span>
                    </div>
                </div>

                <div className="summary-card payments">
                    <div className="card-content">
                        <h3>Total Payments</h3>
                        <p className="card-value">KES {monthlyPaymentsTotal.toLocaleString()}</p>
                        <span className="card-subtext">{completedPayments} Completed</span>
                    </div>
                </div>
            </div>

            <div className="stats-section">
                <div className="stats-card">
                    <h3>Maintenance Status</h3>
                    <div className="stat-item">
                        <span className="stat-label">Open</span>
                        <span className="stat-value">{pendingMaintenance}</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-label">In Progress</span>
                        <span className="stat-value">{inProgressMaintenance}</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-label">Completed</span>
                        <span className="stat-value">{completedMaintenance}</span>
                    </div>
                    <div className="stat-divider"></div>
                    <div className="stat-item total">
                        <span className="stat-label">Est. Cost</span>
                        <span className="stat-value">KES {maintenanceCostTotal.toLocaleString()}</span>
                    </div>
                </div>

                <div className="stats-card">
                    <h3>Payment Status</h3>
                    <div className="stat-item">
                        <span className="stat-label">Completed</span>
                        <span className="stat-value">{completedPayments}</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-label">Pending</span>
                        <span className="stat-value">{pendingPayments}</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-label">Failed</span>
                        <span className="stat-value">{failedPayments}</span>
                    </div>
                    <div className="stat-divider"></div>
                    <div className="stat-item total">
                        <span className="stat-label">Total</span>
                        <span className="stat-value">KES {monthlyPaymentsTotal.toLocaleString()}</span>
                    </div>
                </div>

                <div className="stats-card">
                    <h3>Quick Info</h3>
                    <div className="stat-item">
                        <span className="stat-label">Total Units</span>
                        <span className="stat-value">{properties.reduce((sum, p) => sum + (p.totalUnits || 0), 0)}</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-label">Tenants/Property</span>
                        <span className="stat-value">{totalProperties > 0 ? Math.round(totalTenants / totalProperties) : 0}</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-label">Occupancy Rate</span>
                        <span className="stat-value">{totalProperties > 0 ? Math.round((totalTenants / totalProperties) * 100) : 0}%</span>
                    </div>
                </div>
            </div>

            <div className="activity-section">
                <div className="activity-card">
                    <h3>📋 Recent Properties</h3>
                    {recentProperties.length === 0 ? (
                        <p className="empty-message">No properties yet</p>
                    ) : (
                        <ul className="activity-list">
                            {recentProperties.map(prop => (
                                <li key={prop.id} className="activity-item">
                                    <div className="activity-info">
                                        <span className="activity-title">{prop.name}</span>
                                        <span className="activity-detail">{prop.address}, {prop.city}</span>
                                    </div>
                                    <span className="activity-date">{prop.totalUnits} units</span>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>

                <div className="activity-card">
                    <h3>👥 Recent Tenants</h3>
                    {recentTenants.length === 0 ? (
                        <p className="empty-message">No tenants yet</p>
                    ) : (
                        <ul className="activity-list">
                            {recentTenants.map(tenant => (
                                <li key={tenant.id} className="activity-item">
                                    <div className="activity-info">
                                        <span className="activity-title">{tenant.firstName} {tenant.lastName}</span>
                                        <span className="activity-detail">{tenant.email}</span>
                                    </div>
                                    <span className="activity-date">{tenant.status}</span>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>

                <div className="activity-card">
                    <h3>🔧 Recent Maintenance</h3>
                    {recentMaintenance.length === 0 ? (
                        <p className="empty-message">No maintenance requests yet</p>
                    ) : (
                        <ul className="activity-list">
                            {recentMaintenance.map(item => (
                                <li key={item.id} className="activity-item">
                                    <div className="activity-info">
                                        <span className="activity-title">{item.title}</span>
                                        <span className="activity-detail">{item.category} — {item.priority}</span>
                                    </div>
                                    <span className={`activity-status status-${item.status?.toLowerCase()}`}>{item.status}</span>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>

                <div className="activity-card">
                    <h3>💰 Recent Payments</h3>
                    {recentPayments.length === 0 ? (
                        <p className="empty-message">No payments yet</p>
                    ) : (
                        <ul className="activity-list">
                            {recentPayments.map(payment => (
                                <li key={payment.id} className="activity-item">
                                    <div className="activity-info">
                                        <span className="activity-title">Tenant #{payment.tenantId}</span>
                                        <span className="activity-detail">Due: {new Date(payment.dueDate).toLocaleDateString()}</span>
                                    </div>
                                    <span className={`activity-status status-${payment.status?.toLowerCase()}`}>
                                        KES {(payment.amount || 0).toLocaleString()}
                                    </span>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            </div>
        </div>
    );
}

export default Dashboard; 