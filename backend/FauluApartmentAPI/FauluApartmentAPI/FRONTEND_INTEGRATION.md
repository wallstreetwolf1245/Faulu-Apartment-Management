# Frontend-Backend Integration Guide

## Overview
This guide explains how to integrate the Faulu Apartment Management React frontend with the .NET 9 backend API.

---

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    React Frontend                             │
│          (C:\Users\ADMIN\FauluApartmentManagement)            │
│                                                               │
│  - User Interface                                             │
│  - State Management                                           │
│  - API Calls using Axios/Fetch                               │
└──────────────────┬──────────────────────────────────────────┘
				   │
				   │ HTTP/HTTPS
				   │ REST API Calls
				   │
┌──────────────────▼──────────────────────────────────────────┐
│                 .NET 9 Backend API                           │
│          (C:\Users\ADMIN\Desktop\FauluApartmentAPI)          │
│                                                               │
│  - Controllers (7 controllers)                                │
│  - Services (6 services)                                      │
│  - Repositories (6 repositories)                              │
│  - Database (SQL Server)                                      │
└──────────────────────────────────────────────────────────────┘
```

---

## Backend API Configuration

### Base URL
```
Development:  http://localhost:5000
			 https://localhost:5001 (HTTPS)
Production:   https://api.yourdomain.com
```

### Default Admin Credentials
```
Email:    admin@fauluapp.com
Password: Admin@123
```

### API Endpoints Summary

#### Authentication
- `POST /api/auth/login` - Login user
- `POST /api/auth/register` - Register new user

#### Buildings
- `GET /api/buildings` - List all buildings
- `GET /api/buildings/{id}` - Get building by ID
- `POST /api/buildings` - Create building
- `PUT /api/buildings/{id}` - Update building
- `DELETE /api/buildings/{id}` - Delete building

#### Units
- `GET /api/units/building/{buildingId}` - Get units by building
- `GET /api/units/{id}` - Get unit by ID
- `GET /api/units/building/{buildingId}/vacant` - Get vacant units
- `POST /api/units` - Create unit
- `PUT /api/units/{id}` - Update unit
- `DELETE /api/units/{id}` - Delete unit

#### Tenants
- `GET /api/tenants` - List all tenants
- `GET /api/tenants/{id}` - Get tenant by ID
- `GET /api/tenants/search/{name}` - Search tenants
- `POST /api/tenants` - Create tenant
- `PUT /api/tenants/{id}` - Update tenant
- `DELETE /api/tenants/{id}` - Delete tenant

#### Leases
- `GET /api/leases/{id}` - Get lease by ID
- `GET /api/leases/tenant/{tenantId}` - Get leases by tenant
- `GET /api/leases/unit/{unitId}` - Get leases by unit
- `GET /api/leases/expiring/{daysUntilExpiry}` - Get expiring leases
- `POST /api/leases` - Create lease
- `PUT /api/leases/{id}` - Update lease
- `POST /api/leases/{id}/renew` - Renew lease
- `POST /api/leases/{id}/terminate` - Terminate lease

#### Payments
- `GET /api/payments/{id}` - Get payment by ID
- `GET /api/payments/tenant/{tenantId}` - Get payments by tenant
- `GET /api/payments/lease/{leaseId}` - Get payments by lease
- `GET /api/payments/status/overdue` - Get overdue payments
- `GET /api/payments/tenant/{tenantId}/outstanding` - Get outstanding balance
- `POST /api/payments` - Create payment
- `POST /api/payments/{id}/record` - Record payment
- `PUT /api/payments/{id}` - Update payment
- `DELETE /api/payments/{id}` - Delete payment

#### Maintenance
- `GET /api/maintenance/{id}` - Get order by ID
- `GET /api/maintenance/building/{buildingId}` - Get orders by building
- `GET /api/maintenance/status/open` - Get open orders
- `GET /api/maintenance/priority/urgent` - Get urgent orders
- `POST /api/maintenance` - Create order
- `PUT /api/maintenance/{id}` - Update order
- `POST /api/maintenance/{id}/complete` - Complete order
- `DELETE /api/maintenance/{id}` - Delete order

---

## Frontend Integration Steps

### Step 1: Create API Service File

Create `src/services/api.js`:

```javascript
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:5001';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
	'Content-Type': 'application/json',
  },
});

// Add JWT token to requests
api.interceptors.request.use(
  (config) => {
	const token = localStorage.getItem('authToken');
	if (token) {
	  config.headers.Authorization = `Bearer ${token}`;
	}
	return config;
  },
  (error) => {
	return Promise.reject(error);
  }
);

// Handle response errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
	if (error.response?.status === 401) {
	  // Clear token and redirect to login
	  localStorage.removeItem('authToken');
	  window.location.href = '/login';
	}
	return Promise.reject(error);
  }
);

export default api;
```

### Step 2: Create Authentication Service

Create `src/services/authService.js`:

```javascript
import api from './api';

const authService = {
  login: async (email, password) => {
	try {
	  const response = await api.post('/auth/login', { email, password });
	  if (response.data.success && response.data.data.token) {
		localStorage.setItem('authToken', response.data.data.token);
		localStorage.setItem('user', JSON.stringify(response.data.data));
		return { success: true, data: response.data.data };
	  }
	  return { success: false, error: response.data.message };
	} catch (error) {
	  return { success: false, error: error.message };
	}
  },

  register: async (email, firstName, lastName, password) => {
	try {
	  const response = await api.post('/auth/register', {
		email,
		firstName,
		lastName,
		password,
		confirmPassword: password,
		userType: 'Tenant',
	  });
	  return { success: response.data.success, data: response.data.data, error: response.data.message };
	} catch (error) {
	  return { success: false, error: error.message };
	}
  },

  logout: () => {
	localStorage.removeItem('authToken');
	localStorage.removeItem('user');
  },

  getCurrentUser: () => {
	const user = localStorage.getItem('user');
	return user ? JSON.parse(user) : null;
  },

  isAuthenticated: () => {
	return !!localStorage.getItem('authToken');
  },
};

export default authService;
```

### Step 3: Create Resource Services

Create `src/services/buildingService.js`:

```javascript
import api from './api';

const buildingService = {
  getAllBuildings: async () => {
	try {
	  const response = await api.get('/buildings');
	  return { success: response.data.success, data: response.data.data };
	} catch (error) {
	  return { success: false, error: error.message };
	}
  },

  getBuildingById: async (id) => {
	try {
	  const response = await api.get(`/buildings/${id}`);
	  return { success: response.data.success, data: response.data.data };
	} catch (error) {
	  return { success: false, error: error.message };
	}
  },

  createBuilding: async (buildingData) => {
	try {
	  const response = await api.post('/buildings', buildingData);
	  return { success: response.data.success, data: response.data.data };
	} catch (error) {
	  return { success: false, error: error.message };
	}
  },

  updateBuilding: async (id, buildingData) => {
	try {
	  const response = await api.put(`/buildings/${id}`, { id, ...buildingData });
	  return { success: response.data.success, data: response.data.data };
	} catch (error) {
	  return { success: false, error: error.message };
	}
  },

  deleteBuilding: async (id) => {
	try {
	  const response = await api.delete(`/buildings/${id}`);
	  return { success: response.data.success };
	} catch (error) {
	  return { success: false, error: error.message };
	}
  },
};

export default buildingService;
```

### Step 4: Create Environment Configuration

Create `.env` file in frontend root:

```env
REACT_APP_API_URL=https://localhost:5001
REACT_APP_API_TIMEOUT=30000
REACT_APP_DEBUG=false
```

Create `.env.production` for production:

```env
REACT_APP_API_URL=https://api.yourdomain.com
REACT_APP_API_TIMEOUT=30000
REACT_APP_DEBUG=false
```

---

## Running Both Applications

### Terminal 1: Run Backend API
```bash
cd C:\Users\ADMIN\Desktop\FauluApartmentAPI
dotnet run
# Backend runs on: https://localhost:5001
```

### Terminal 2: Run Frontend
```bash
cd C:\Users\ADMIN\FauluApartmentManagement\Faulu-apartment-management
npm start
# Frontend runs on: http://localhost:3000
```

---

## CORS Configuration

The backend is configured with CORS enabled. If you get CORS errors:

1. Update `Program.cs` to add specific origins:
```csharp
options.AddPolicy("Frontend", builder =>
{
	builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
		.AllowAnyMethod()
		.AllowAnyHeader();
});
```

2. Update middleware:
```csharp
app.UseCors("Frontend");
```

---

## Testing Integration

### Login Test
```javascript
import authService from './services/authService';

const testLogin = async () => {
  const result = await authService.login('admin@fauluapp.com', 'Admin@123');
  console.log(result);
  if (result.success) {
	console.log('Login successful!', result.data);
  }
};

testLogin();
```

### Building List Test
```javascript
import buildingService from './services/buildingService';

const testGetBuildings = async () => {
  const result = await buildingService.getAllBuildings();
  console.log(result);
};

testGetBuildings();
```

---

## Security Considerations

1. **HTTPS in Production**: Always use HTTPS URLs
2. **JWT Token**: Stored in localStorage (consider using httpOnly cookies)
3. **CORS**: Restrict to specific origins in production
4. **API Key**: Consider adding API key validation
5. **Rate Limiting**: Implement on backend for API protection
6. **Input Validation**: Validate all inputs on frontend and backend

---

## Error Handling

All API responses follow this format:

```json
{
  "success": true|false,
  "message": "Description",
  "data": {},
  "errors": {}
}
```

Handle errors in frontend:
```javascript
try {
  const result = await api.get('/buildings');
  if (result.data.success) {
	// Handle success
  } else {
	// Handle API error
	console.error(result.data.message);
  }
} catch (error) {
  // Handle network error
  console.error(error.message);
}
```

---

## State Management Integration

### With Redux
```javascript
// Store API token in Redux state
const authSlice = createSlice({
  name: 'auth',
  initialState: {
	token: localStorage.getItem('authToken'),
	user: JSON.parse(localStorage.getItem('user') || 'null'),
  },
});
```

### With React Context
```javascript
const AuthContext = createContext();

function AuthProvider({ children }) {
  const [user, setUser] = useState(authService.getCurrentUser());

  const login = async (email, password) => {
	const result = await authService.login(email, password);
	if (result.success) {
	  setUser(result.data);
	}
	return result;
  };

  return (
	<AuthContext.Provider value={{ user, login }}>
	  {children}
	</AuthContext.Provider>
  );
}
```

---

## Troubleshooting

### CORS Error
- Ensure backend CORS is configured
- Check frontend API URL matches backend URL
- Verify backend is running

### 401 Unauthorized
- Login first to get JWT token
- Check token is valid (not expired)
- Ensure token is sent in Authorization header

### 404 Not Found
- Check endpoint URL is correct
- Verify resource ID exists
- Check request path spelling

### Network Error
- Ensure backend is running
- Check API URL in environment
- Verify HTTPS certificate (development)

---

## Next Steps

1. Copy service files to frontend `src/services/`
2. Update `.env` file with backend URL
3. Run both applications
4. Test login endpoint
5. Test building list endpoint
6. Integrate with React components
7. Test all CRUD operations

---

## Support

- Backend API docs: `/API_DOCUMENTATION.md` in backend project
- Common errors: See "Troubleshooting" section
- More examples: See service files
