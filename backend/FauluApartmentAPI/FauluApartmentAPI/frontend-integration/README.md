# Frontend Integration Files

This directory contains all the service files you need to integrate the Faulu Apartment Management React frontend with the .NET 9 backend API.

## Files in This Directory

### Core Files
- **api.js** - HTTP client configuration with axios, interceptors, and token handling
- **authService.js** - Authentication service (login, register, logout, user state)

### Resource Service Files
- **buildingService.js** - Building CRUD operations
- **unitService.js** - Unit/Apartment CRUD operations
- **tenantService.js** - Tenant management
- **leaseService.js** - Lease management and renewal
- **paymentService.js** - Payment tracking and recording
- **maintenanceService.js** - Maintenance order management

### Configuration
- **.env.example** - Environment variables template

## Quick Start

### Step 1: Copy Service Files

Copy all `.js` files from this directory to your React project:

```bash
# From FauluApartmentAPI/frontend-integration
cp *.js ../../../FauluApartmentManagement/Faulu-apartment-management/src/services/
```

Or manually copy each file to:
```
src/services/
├── api.js
├── authService.js
├── buildingService.js
├── unitService.js
├── tenantService.js
├── leaseService.js
├── paymentService.js
└── maintenanceService.js
```

### Step 2: Setup Environment Variables

1. Create `.env` file in your React project root
2. Copy content from `.env.example`
3. Update `REACT_APP_API_URL` to match your backend URL

```env
REACT_APP_API_URL=https://localhost:5001
REACT_APP_API_TIMEOUT=30000
REACT_APP_DEBUG=true
```

### Step 3: Install Dependencies

Ensure axios is installed:
```bash
npm install axios
```

### Step 4: Use Services in Components

Example in a React component:

```javascript
import authService from '../services/authService';
import buildingService from '../services/buildingService';

function App() {
  const handleLogin = async () => {
	const result = await authService.login('admin@fauluapp.com', 'Admin@123');
	if (result.success) {
	  console.log('Logged in!', result.data);
	}
  };

  const handleFetchBuildings = async () => {
	const result = await buildingService.getAllBuildings();
	if (result.success) {
	  console.log('Buildings:', result.data);
	}
  };

  return (
	<div>
	  <button onClick={handleLogin}>Login</button>
	  <button onClick={handleFetchBuildings}>Load Buildings</button>
	</div>
  );
}
```

## API Service Methods

### Authentication (authService.js)
- `login(email, password)` - Login user
- `register(userData)` - Register new user
- `logout()` - Clear auth state
- `isAuthenticated()` - Check if user is logged in
- `getCurrentUser()` - Get logged-in user info
- `getUserRoles()` - Get user roles
- `hasRole(roleName)` - Check if user has specific role
- `getToken()` - Get JWT token

### Buildings (buildingService.js)
- `getAllBuildings()` - Get all buildings
- `getBuildingById(id)` - Get single building
- `createBuilding(data)` - Create building
- `updateBuilding(id, data)` - Update building
- `deleteBuilding(id)` - Delete building

### Units (unitService.js)
- `getUnitsByBuilding(buildingId)` - Get units in building
- `getUnitById(id)` - Get single unit
- `getVacantUnits(buildingId)` - Get vacant units
- `createUnit(data)` - Create unit
- `updateUnit(id, data)` - Update unit
- `deleteUnit(id)` - Delete unit

### Tenants (tenantService.js)
- `getAllTenants()` - Get all tenants
- `getTenantById(id)` - Get single tenant
- `searchTenants(name)` - Search by name
- `createTenant(data)` - Create tenant
- `updateTenant(id, data)` - Update tenant
- `deleteTenant(id)` - Delete tenant

### Leases (leaseService.js)
- `getLeaseById(id)` - Get single lease
- `getLeasesByTenant(tenantId)` - Get tenant's leases
- `getLeasesByUnit(unitId)` - Get leases for unit
- `getExpiringLeases(daysUntilExpiry)` - Get expiring leases
- `createLease(data)` - Create lease
- `updateLease(id, data)` - Update lease
- `renewLease(id, newEndDate)` - Renew lease
- `terminateLease(id)` - Terminate lease

### Payments (paymentService.js)
- `getPaymentById(id)` - Get single payment
- `getPaymentsByTenant(tenantId)` - Get tenant's payments
- `getPaymentsByLease(leaseId)` - Get lease payments
- `getOverduePayments()` - Get overdue payments
- `getTotalOutstanding(tenantId)` - Get total owed
- `createPayment(data)` - Create payment record
- `recordPayment(paymentId, amount, method, ref)` - Record payment received
- `updatePayment(id, data)` - Update payment
- `deletePayment(id)` - Delete payment

### Maintenance (maintenanceService.js)
- `getOrderById(id)` - Get single order
- `getOrdersByBuilding(buildingId)` - Get building's orders
- `getOpenOrders()` - Get open orders
- `getUrgentOrders()` - Get urgent orders
- `createOrder(data)` - Create order
- `updateOrder(id, data)` - Update order
- `completeOrder(id, notes)` - Complete order
- `deleteOrder(id)` - Delete order

## API Response Format

All service methods return a standardized object:

```javascript
{
  success: boolean,      // true if operation succeeded
  data: any,            // Response data (null if failed)
  error: string         // Error message (only if success=false)
}
```

Example:
```javascript
const result = await buildingService.getAllBuildings();

if (result.success) {
  console.log('Buildings:', result.data);
} else {
  console.error('Error:', result.error);
}
```

## Authentication Flow

1. **Login**: Call `authService.login(email, password)`
   - Token stored in localStorage as `authToken`
   - User data stored in localStorage as `user`
   - User roles stored in localStorage as `userRoles`

2. **Automatic Authorization**: All API requests automatically include the JWT token in the Authorization header

3. **Logout**: Call `authService.logout()` to clear stored data

4. **Token Expiry**: If token expires (401 response), user is automatically redirected to login page

## Error Handling

Implement error handling in your components:

```javascript
try {
  const result = await buildingService.getAllBuildings();

  if (result.success) {
	// Handle success
	setBuildings(result.data);
  } else {
	// Handle API error
	setError(result.error);
  }
} catch (error) {
  // Handle network or unexpected error
  console.error('Unexpected error:', error);
}
```

## Running Backend & Frontend

### Terminal 1: Backend
```bash
cd FauluApartmentAPI
dotnet run
# Backend runs on https://localhost:5001
```

### Terminal 2: Frontend
```bash
cd FauluApartmentManagement/Faulu-apartment-management
npm install
npm start
# Frontend runs on http://localhost:3000
```

## Testing Integration

Test the integration in your browser console:

```javascript
// Test login
import authService from './services/authService';
authService.login('admin@fauluapp.com', 'Admin@123').then(r => console.log(r));

// Test getting buildings
import buildingService from './services/buildingService';
buildingService.getAllBuildings().then(r => console.log(r));
```

## Troubleshooting

### CORS Errors
- Ensure backend CORS is configured correctly
- Check that `REACT_APP_API_URL` matches backend URL
- Verify backend is running

### 401 Unauthorized
- Login first
- Check localStorage has `authToken`
- Verify token in Network tab Authorization header

### 404 Not Found
- Check endpoint URL is correct
- Verify resource ID exists
- Check backend logs for details

### Network Timeout
- Ensure backend is running
- Check `REACT_APP_API_URL` is correct
- For HTTPS on localhost, accept self-signed certificate

## Next Steps

1. Copy service files to `src/services/`
2. Create `.env` file with API URL
3. Test login endpoint with admin credentials
4. Build a login page component
5. Build CRUD pages for buildings, units, tenants, leases
6. Implement payment tracking dashboard
7. Add maintenance order management
8. Deploy to production with updated API URL

## Support

For issues with:
- **Backend API**: Check `API_DOCUMENTATION.md` in the FauluApartmentAPI project
- **Service methods**: Refer to inline JSDoc comments in each service file
- **CORS/Auth**: See CORS configuration section in this file
