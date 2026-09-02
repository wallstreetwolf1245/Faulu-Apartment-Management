import { useState, useEffect } from 'react'
import { LayoutDashboard, Building2, PlusCircle, Users, Wrench, Wallet, LogOut, PanelLeftClose, PanelLeftOpen } from 'lucide-react'
import Dashboard from './Landlord/Dashboard'
import AddProperty from './Landlord/Addproperty'
import PropertiesList from './Landlord/PropertiesList'
import AddTenant from './Landlord/AddTenant'
import TenantsList from './Landlord/TenantsList'
import AddMaintenance from './Landlord/AddMaintenance'
import MaintenanceList from './Landlord/MaintenanceList'
import AddPayment from './Landlord/AddPayment'
import PaymentsList from './Landlord/PaymentsList'
import PaymentsPage from './features/payments/PaymentsPage'
import AddUnits from './Landlord/AddUnits'
import UnitsList from './Landlord/UnitsList'
import Login from './Landlord/Login'
import Signup from './Landlord/Signup'
import authService from './services/authService'
import buildingService from './services/buildingService'
import { createPayment, matchPayment } from './services/PaymentsApi'
import api from './services/api'
import { listPayments } from './features/payments/api'

const getBuildingOwnerId = (building) =>
  building?.ownerId ?? building?.userId ?? building?.createdById ?? building?.createdBy ?? building?.landlordId ?? building?.landlordUserId ?? null;
import './App.css'
import { normalizeTenant, enrichTenantsWithLookup } from './Landlord/tenantUtils'

// Ensure Authorization header is attached to api requests (workaround for broken interceptor in services/api.js)
api.interceptors.request.use(
  (config) => {
    const token = authService.getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(() => authService.isAuthenticated())
  const [user, setUser]                       = useState(() => authService.getCurrentUser())
  const [authMode, setAuthMode]               = useState('login')
  const [currentPage, setCurrentPage]         = useState('dashboard')
  const [selectedPropertyId, setSelectedPropertyId] = useState(null)
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false)

  const [properties, setProperties] = useState([])
  const [tenants,    setTenants]    = useState([])
  const [maintenance, setMaintenance] = useState([])
  const [payments,   setPayments]   = useState([])

  const [units,    setUnits]    = useState([])
  const [allUnits, setAllUnits] = useState([])

  // ── Initial data fetch ───────────────────────────────────────────────────
  useEffect(() => {
    if (!isAuthenticated) return;

    const fetchData = async () => {
      try {
        const propsRes = await buildingService.getAllBuildings();
        if (propsRes.success) {
          const currentUser = authService.getCurrentUser();
          const allProperties = Array.isArray(propsRes.data) ? propsRes.data : [];
          const propertiesWithOwner = allProperties.filter(p => getBuildingOwnerId(p) !== null);
          setProperties((currentUser?.id && propertiesWithOwner.length > 0)
            ? allProperties.filter(p => String(getBuildingOwnerId(p)) === String(currentUser.id))
            : allProperties
          );
        } else {
          console.error('Fetch properties failed:', propsRes.error);
        }
      } catch (err) { console.error('Fetch properties error:', err); }

      try {
        const tenantsRes = await api.get('/tenants');
        if (tenantsRes.data?.success) setTenants(tenantsRes.data.data);
        else console.error('Fetch tenants failed:', tenantsRes.data?.message);
      } catch (err) { console.error('Fetch tenants error:', err); }

      try {
        const maintRes = await api.get('/maintenance');
        if (maintRes.data?.success) setMaintenance(maintRes.data.data);
        else console.error('Fetch maintenance failed:', maintRes.data?.message);
      } catch (err) { console.error('Fetch maintenance error:', err); }

      try {
        if (selectedPropertyId) {
          const paymentsRes = await api.get(`/payments?buildingId=${selectedPropertyId}`);
          if (paymentsRes.data?.success) setPayments(paymentsRes.data.data);
          else console.error('Fetch payments failed:', paymentsRes.data?.message);
        } else {
          // Avoid unscoped payments fetch; payments will be loaded when a building is selected
          setPayments([]);
        }
      } catch (err) { console.error('Fetch payments error:', err); }

      try {
        const allUnitsRes = await api.get('/units');
        if (allUnitsRes.data?.success) setAllUnits(allUnitsRes.data.data);
        else console.error('Fetch all units failed:', allUnitsRes.data?.message);
      } catch (err) { console.error('Fetch all units error:', err); }
    };

    fetchData();
  }, [isAuthenticated]);

  // ── Building-specific units ──────────────────────────────────────────────
  useEffect(() => {
    if (!selectedPropertyId || !isAuthenticated) return;

    const fetchUnits = async () => {
      try {
        const res = await api.get(`/units/building/${selectedPropertyId}`);
        if (res.data?.success) setUnits(res.data.data);
        else { console.error('Fetch units failed:', res.data?.message); setUnits([]); }
      } catch (err) { console.error('Fetch units error:', err); setUnits([]); }
    };

    fetchUnits();
  }, [selectedPropertyId, isAuthenticated]);

  // ── Enrich existing tenants with property and unit names ──────────────────
  useEffect(() => {
    if (tenants.length > 0 && (properties.length > 0 || allUnits.length > 0)) {
      const enrichedTenants = enrichTenantsWithLookup(tenants, properties, allUnits);
      setTenants(enrichedTenants);
    }
  }, [properties, allUnits, tenants.length]);

  // ── Handlers ─────────────────────────────────────────────────────────────

  const handleAddProperty = async (newProperty, options = {}) => {
    try {
      const res = await buildingService.createBuilding(newProperty);
      if (res.success) {
        setProperties(prev => [...prev, res.data]);
        if (!options.skipNavigate) setCurrentPage('propertiesList');
      } else { console.error('Create property failed', res.error); }
    } catch (err) { console.error('Create property error', err); }
  }

  const handleAddTenant = (createdTenant, options = {}) => {
    const normalizedTenant = normalizeTenant(createdTenant, options);
    if (normalizedTenant) setTenants(prev => [...prev, normalizedTenant]);
    if (!options.skipNavigate) setCurrentPage('tenantsList');
  }

  const handleAddMaintenance = async (newMaintenance, options = {}) => {
    try {
      const res = await api.post('/maintenance', newMaintenance);
      if (res.data?.success) {
        setMaintenance(prev => [...prev, res.data.data]);
        if (!options.skipNavigate) setCurrentPage('maintenanceList');
      } else { console.error('Create maintenance failed', res.data?.message || res); }
    } catch (err) { console.error('Create maintenance error', err); }
  }

  const handleAddPayment = async (newPayment, options = {}) => {
    try {
      const payload = {
        tenantId:      newPayment.tenantId  ? Number(newPayment.tenantId)  : undefined,
        leaseId:       newPayment.leaseId   ? Number(newPayment.leaseId)   : null,
        amount:        Number(newPayment.amount),
        dueDate:       newPayment.dueDate
                         ? new Date(newPayment.dueDate).toISOString()
                         : new Date().toISOString(),
        paymentMethod: newPayment.paymentMethod || null,
        paymentType:   newPayment.paymentType   || 'Rent',
        notes:         newPayment.notes         || null,
        payerPhone:    newPayment.payerPhone   || null,
      };

      const selectedPropertyId = options.propertyId ?? options.buildingId ?? (newPayment.propertyId ? Number(newPayment.propertyId) : null);

      if (!isFinite(payload.amount) || payload.amount <= 0) {
        alert('Payment amount must be greater than 0');
        return null;
      }

      const res = await createPayment(payload);
      const created = res?.data ?? res;
      const success = res?.success !== false && Boolean(created?.id || created?._id || res?.success);
      if (success) {
        if (selectedPropertyId) {
          setSelectedPropertyId(selectedPropertyId);
        }
        if (created) {
          setPayments(prev => [...prev, created]);
          if (!options.skipNavigate) setCurrentPage('paymentsList');
          return created;
        }

        if (!options.skipNavigate) setCurrentPage('paymentsList');
        return null;
      }

      console.error('Create payment failed', res?.message || res);
      return null;
    } catch (err) {
      console.error('Create payment error', err);
      if (err.response) {
        console.error('Status:', err.response.status, 'Data:', err.response.data);
        if (err.response.data?.message) alert(`Failed to create payment: ${err.response.data.message}`);
      } else {
        alert('Failed to create payment. See console for details.');
      }
      return null;
    }
  }

  const handleNavigate = (page) => setCurrentPage(page)

  const handleUpdateProperty = async (updatedProperty) => {
    try {
      const res = await buildingService.updateBuilding(updatedProperty.id, updatedProperty);
      if (res.success) {
        setProperties(prev => prev.map(p => p.id === updatedProperty.id ? res.data : p));
      } else { console.error('Update property failed', res.error); }
    } catch (err) { console.error('Update property error', err); }
  }

  const handleDeleteProperty = async (id) => {
    try {
      const res = await buildingService.deleteBuilding(id);
      if (res.success) setProperties(prev => prev.filter(p => p.id !== id));
      else console.error('Delete property failed', res.error);
    } catch (err) { console.error('Delete property error', err); }
  }

  const handleUpdateTenant = async (updatedTenant) => {
    try {
      const res = await api.put(`/tenants/${updatedTenant.id}`, updatedTenant);
      if (res.data?.success) {
        const returned = res.data.data || {};

        // The backend PUT response may only include raw fields (buildingId/
        // unitId) without propertyName/unitNumber attached — same gap we saw
        // on initial GET. Merge in what we already resolved client-side (from
        // the edit form's dropdowns) as a fallback, so the row doesn't flash
        // N/A right after saving.
        const merged = {
          ...updatedTenant,
          ...returned,
          propertyName: returned.propertyName || updatedTenant.propertyName,
          unitNumber: returned.unitNumber || updatedTenant.unitNumber,
          unitType: returned.unitType || updatedTenant.unitType,
        };

        setTenants(prev => prev.map(t => t.id === updatedTenant.id ? merged : t));

        // If the tenant's unit actually changed, the old unit should now be
        // vacant and the new one occupied on the backend — refresh allUnits
        // so AddTenant / UnitsList reflect the new availability immediately.
        if (
          updatedTenant.previousUnitId &&
          String(updatedTenant.previousUnitId) !== String(updatedTenant.unitId)
        ) {
          try {
            const unitsRes = await api.get('/units');
            if (unitsRes.data?.success) setAllUnits(unitsRes.data.data);
          } catch (err) { console.error('Refresh units error:', err); }
        }
      } else { console.error('Update tenant failed', res.data?.message || res); }
    } catch (err) { console.error('Update tenant error', err); }
  }

  const handleDeleteTenant = async (id) => {
    try {
      const res = await api.delete(`/tenants/${id}`);
      if (res.data?.success) setTenants(prev => prev.filter(t => t.id !== id));
      else console.error('Delete tenant failed', res.data?.message || res);
    } catch (err) { console.error('Delete tenant error', err); }
  }

  const handleUpdateMaintenance = async (updatedMaintenance) => {
    try {
      const res = await api.put(`/maintenance/${updatedMaintenance.id}`, updatedMaintenance);
      if (res.data?.success) {
        setMaintenance(prev => prev.map(m => m.id === updatedMaintenance.id ? res.data.data : m));
      } else { console.error('Update maintenance failed', res.data?.message || res); }
    } catch (err) { console.error('Update maintenance error', err); }
  }

  const handleDeleteMaintenance = async (id) => {
    try {
      const res = await api.delete(`/maintenance/${id}`);
      if (res.data?.success) setMaintenance(prev => prev.filter(m => m.id !== id));
      else console.error('Delete maintenance failed', res.data?.message || res);
    } catch (err) { console.error('Delete maintenance error', err); }
  }

  const handleUpdatePayment = async (updatedPayment) => {
    try {
      const res = await api.put(`/payments/${updatedPayment.id}`, updatedPayment);
      if (res.data?.success) {
        setPayments(prev => prev.map(p => p.id === updatedPayment.id ? res.data.data : p));
      } else { console.error('Update payment failed', res.data?.message || res); }
    } catch (err) { console.error('Update payment error', err); }
  }

  const handleDeletePayment = async (id) => {
    try {
      const res = await api.delete(`/payments/${id}`);
      if (res.data?.success) setPayments(prev => prev.filter(p => p.id !== id));
      else console.error('Delete payment failed', res.data?.message || res);
    } catch (err) { console.error('Delete payment error', err); }
  }

  // Assigns an Unmatched M-Pesa payment to a unit and updates local state
  const handleMatchPayment = async (paymentId, unitId) => {
    try {
      const res = await matchPayment(paymentId, unitId);
      if (res?.success) {
        // Refresh the payments list for the currently selected building so the matched
        // payment shows updated status, tenant, and unit without a full page reload
        const p = await listPayments(selectedPropertyId);
        setPayments(p?.data ?? p ?? []);
      } else {
        console.error('Match payment failed', res);
        alert('Failed to match payment. Please try again.');
      }
    } catch (err) {
      console.error('Match payment error', err);
      alert('Failed to match payment. See console for details.');
    }
  }

  const handleAddUnit = async (newUnit) => {
    try {
      const res = await api.post('/units', newUnit);
      if (res.data?.success) {
        const created = res.data.data;
        setUnits(prev => [...prev, created]);
        setAllUnits(prev => [...prev, created]);
        setCurrentPage('unitsList');
      } else { console.error('Create unit failed', res.data?.message || res); }
    } catch (err) { console.error('Create unit error', err); }
  }

  const handleDeleteUnit = async (unitId) => {
    try {
      const res = await api.delete(`/units/${unitId}`);
      if (res.data?.success) {
        setUnits(prev    => prev.filter(u => u.id !== unitId));
        setAllUnits(prev => prev.filter(u => u.id !== unitId));
      } else { console.error('Delete unit failed', res.data?.message || res); }
    } catch (err) { console.error('Delete unit error', err); }
  }

  const handleUpdateUnit = async (updatedUnit) => {
    try {
      const res = await api.put(`/units/${updatedUnit.id}`, updatedUnit);
      if (res.data?.success) {
        const updated = res.data.data;
        setUnits(prev    => prev.map(u => u.id === updatedUnit.id ? updated : u));
        setAllUnits(prev => prev.map(u => u.id === updatedUnit.id ? updated : u));
      } else { console.error('Update unit failed', res.data?.message || res); }
    } catch (err) { console.error('Update unit error', err); }
  }

  const handleViewUnits = (propertyId) => {
    setSelectedPropertyId(propertyId);
    setCurrentPage('unitsList');
  }

  const handleSelectBuilding = (propertyId) => {
    setSelectedPropertyId(propertyId);
    // reset to Payments tab when switching building
    setCurrentPage('paymentsList');
  }

  const handleLoginSuccess = (userData) => {
    setUser(userData);
    setIsAuthenticated(true);
    setCurrentPage('dashboard');
  }

  const handleSignupSuccess = (userData) => {
    setUser(userData);
    setIsAuthenticated(true);
    setCurrentPage('dashboard');
  }

  const handleLogout = () => {
    authService.logout();
    setIsAuthenticated(false);
    setUser(null);
    setAuthMode('login');
    setCurrentPage('dashboard');
    setProperties([]);
    setTenants([]);
    setMaintenance([]);
    setPayments([]);
    setUnits([]);
    setAllUnits([]);
  }

  // ── Auth screens ─────────────────────────────────────────────────────────
  if (!isAuthenticated) {
    return (
      <>
        {authMode === 'login' ? (
          <Login
            onLoginSuccess={handleLoginSuccess}
            onSwitchToSignup={() => setAuthMode('signup')}
          />
        ) : (
          <Signup
            onSignupSuccess={handleSignupSuccess}
            onSwitchToLogin={() => setAuthMode('login')}
          />
        )}
      </>
    )
  }

  // ── Main app ─────────────────────────────────────────────────────────────
  return (
    <div className="app-wrapper">
      <aside className={`sidebar ${sidebarCollapsed ? 'collapsed' : ''}`}>
        <div className="sidebar-header">
          <div className="sidebar-header-top">
            <div className="sidebar-header-text">
              <h1 className="app-title">Faulu</h1>
              <p className="app-subtitle">Apartment Management</p>
            </div>
            <button
              type="button"
              className="sidebar-toggle"
              onClick={() => setSidebarCollapsed(prev => !prev)}
              aria-label={sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
              title={sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            >
              {sidebarCollapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
            </button>
          </div>
          {user && <p className="user-greeting">Welcome, {user.name || user.firstName}!</p>}
        </div>
        {/* My Buildings moved to Payments page */}

        <nav className="sidebar-nav">
          <div className="nav-section">
            <h3 className="nav-section-title">Overview</h3>
            <button className={`nav-item ${currentPage === 'dashboard' ? 'active' : ''}`} onClick={() => handleNavigate('dashboard')} title="Dashboard">
              <span className="nav-icon"><LayoutDashboard size={18} /></span><span className="nav-text">Dashboard</span>
            </button>
          </div>

          <div className="nav-section">
            <h3 className="nav-section-title">Properties</h3>
            <button className={`nav-item ${currentPage === 'addProperty' ? 'active' : ''}`} onClick={() => handleNavigate('addProperty')} title="Add Property">
              <span className="nav-icon"><PlusCircle size={18} /></span><span className="nav-text">Add Property</span>
            </button>
            <button className={`nav-item ${currentPage === 'propertiesList' ? 'active' : ''}`} onClick={() => handleNavigate('propertiesList')} title="Properties">
              <span className="nav-icon"><Building2 size={18} /></span><span className="nav-text">Properties </span>
            </button>
          </div>

          <div className="nav-section">
            <h3 className="nav-section-title">Tenants</h3>
            <button className={`nav-item ${currentPage === 'addTenant' ? 'active' : ''}`} onClick={() => handleNavigate('addTenant')} title="Add Tenant">
              <span className="nav-icon"><PlusCircle size={18} /></span><span className="nav-text">Add Tenant</span>
            </button>
            <button className={`nav-item ${currentPage === 'tenantsList' ? 'active' : ''}`} onClick={() => handleNavigate('tenantsList')} title="Tenants">
              <span className="nav-icon"><Users size={18} /></span><span className="nav-text">Tenants </span>
            </button>
          </div>

          <div className="nav-section">
            <h3 className="nav-section-title">Maintenance</h3>
            <button className={`nav-item ${currentPage === 'addMaintenance' ? 'active' : ''}`} onClick={() => handleNavigate('addMaintenance')} title="Add Request">
              <span className="nav-icon"><PlusCircle size={18} /></span><span className="nav-text">Add Request</span>
            </button>
            <button className={`nav-item ${currentPage === 'maintenanceList' ? 'active' : ''}`} onClick={() => handleNavigate('maintenanceList')} title="Requests">
              <span className="nav-icon"><Wrench size={18} /></span><span className="nav-text">Requests</span>
            </button>
          </div>

          <div className="nav-section">
            <h3 className="nav-section-title">Payments</h3>
            <button className={`nav-item ${currentPage === 'addPayment' ? 'active' : ''}`} onClick={() => handleNavigate('addPayment')} title="Add Manual Payment">
              <span className="nav-icon"><PlusCircle size={18} /></span><span className="nav-text">Add Manual Payment</span>
            </button>
            <button className={`nav-item ${currentPage === 'paymentsList' ? 'active' : ''}`} onClick={() => handleNavigate('paymentsList')} title="Payments">
              <span className="nav-icon"><Wallet size={18} /></span><span className="nav-text">Payments</span>
            </button>
          </div>
        </nav>

        <div className="sidebar-footer">
          <button className="logout-btn" onClick={handleLogout} title="Logout">
            <span className="nav-icon"><LogOut size={18} /></span><span className="nav-text">Logout</span>
          </button>
        </div>
      </aside>

      <div className="app-container">
        {currentPage === 'dashboard' && (
          <Dashboard properties={properties} tenants={tenants} maintenance={maintenance} payments={payments} />
        )}
        {currentPage === 'addProperty' && (
          <AddProperty onPropertyAdded={handleAddProperty} onNavigate={handleNavigate} />
        )}
        {currentPage === 'propertiesList' && (
          <PropertiesList
            properties={properties}
            onUpdateProperty={handleUpdateProperty}
            onDeleteProperty={handleDeleteProperty}
            onNavigate={handleNavigate}
            onViewUnits={handleViewUnits}
          />
        )}
        {currentPage === 'addTenant' && (
          <AddTenant
            properties={properties}
            units={units}
            onTenantAdded={handleAddTenant}
            onNavigate={handleNavigate}
          />
        )}
        {currentPage === 'tenantsList' && (
          <TenantsList
            tenants={tenants}
            properties={properties}
            units={allUnits}
            onUpdateTenant={handleUpdateTenant}
            onDeleteTenant={handleDeleteTenant}
            onNavigate={handleNavigate}
          />
        )}
        {currentPage === 'addMaintenance' && (
          <AddMaintenance
            properties={properties}
            tenants={tenants}
            units={units}
            onMaintenanceAdded={handleAddMaintenance}
            onNavigate={handleNavigate}
          />
        )}
        {currentPage === 'maintenanceList' && (
          <MaintenanceList
            maintenance={maintenance}
            properties={properties}
            tenants={tenants}
            onUpdateMaintenance={handleUpdateMaintenance}
            onDeleteMaintenance={handleDeleteMaintenance}
            onNavigate={handleNavigate}
          />
        )}
        {currentPage === 'addPayment' && (
          <AddPayment
            properties={properties}
            tenants={tenants}
            units={allUnits}
            onPaymentAdded={handleAddPayment}
            onNavigate={handleNavigate}
          />
        )}
        {currentPage === 'paymentsList' && (
          <PaymentsPage
            buildingId={selectedPropertyId}
            onBack={() => setCurrentPage('dashboard')}
          />
        )}
        {currentPage === 'addUnits' && selectedPropertyId && (
          <AddUnits
            property={properties.find(p => p.id === selectedPropertyId)}
            onUnitsAdded={handleAddUnit}
            onNavigate={handleNavigate}
          />
        )}
        {currentPage === 'unitsList' && selectedPropertyId && (
          <UnitsList
            property={properties.find(p => p.id === selectedPropertyId)}
            units={units}
            onDeleteUnit={handleDeleteUnit}
            onUpdateUnit={handleUpdateUnit}
            onNavigate={handleNavigate}
          />
        )}
      </div>
    </div>
  )
}

export default App