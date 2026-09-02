# Integration Checklist

## Overview
This checklist guides you through integrating the .NET 9 backend API with the React frontend at `C:\Users\ADMIN\FauluApartmentManagement\Faulu-apartment-management`.

## Phase 1: Backend Preparation

### 1.1 Free Disk Space
- [ ] Free up at least 5GB of disk space on C: drive
  - Delete old downloads, temp files, or unnecessary programs
  - Run Disk Cleanup: `cleanmgr`
  - Check available space: `Get-Volume C: | Select-Object SizeRemaining` (PowerShell)

### 1.2 Build Backend
- [ ] Open PowerShell and navigate to backend:
  ```powershell
  cd C:\Users\ADMIN\Desktop\FauluApartmentAPI
  ```

- [ ] Restore NuGet packages:
  ```powershell
  dotnet restore
  ```

- [ ] Build the project:
  ```powershell
  dotnet build
  ```

- [ ] Verify no build errors

### 1.3 Setup Database
- [ ] Create EF Core migration:
  ```powershell
  dotnet ef migrations add InitialCreate
  ```

- [ ] Apply migration to database:
  ```powershell
  dotnet ef database update
  ```

- [ ] Verify database was created (check SQL Server or LocalDB)

### 1.4 Test Backend
- [ ] Run the backend:
  ```powershell
  dotnet run
  ```

- [ ] Verify backend starts successfully
- [ ] Note the URL (usually `https://localhost:5001`)
- [ ] Keep this terminal open

---

## Phase 2: Frontend Preparation

### 2.1 Copy Service Files
- [ ] Navigate to frontend project:
  ```powershell
  cd C:\Users\ADMIN\FauluApartmentManagement\Faulu-apartment-management
  ```

- [ ] Create services directory if it doesn't exist:
  ```powershell
  mkdir -p src\services
  ```

- [ ] Copy all service files from backend integration folder:
  ```
  FauluApartmentAPI/frontend-integration/*.js → 
  FauluApartmentManagement/Faulu-apartment-management/src/services/
  ```

  Files to copy:
  - [ ] api.js
  - [ ] authService.js
  - [ ] buildingService.js
  - [ ] unitService.js
  - [ ] tenantService.js
  - [ ] leaseService.js
  - [ ] paymentService.js
  - [ ] maintenanceService.js

### 2.2 Setup Environment Variables
- [ ] Create `.env` file in frontend root:
  ```
  C:\Users\ADMIN\FauluApartmentManagement\Faulu-apartment-management\.env
  ```

- [ ] Add content:
  ```env
  REACT_APP_API_URL=https://localhost:5001
  REACT_APP_API_TIMEOUT=30000
  REACT_APP_DEBUG=true
  REACT_APP_LOG_REQUESTS=true
  ```

- [ ] Save the file

### 2.3 Install Dependencies
- [ ] Install axios (if not already installed):
  ```powershell
  npm install axios
  ```

- [ ] Verify installation:
  ```powershell
  npm list axios
  ```

### 2.4 Start Frontend
- [ ] Open a new PowerShell terminal and start React:
  ```powershell
  cd C:\Users\ADMIN\FauluApartmentManagement\Faulu-apartment-management
  npm start
  ```

- [ ] Wait for the app to compile and open in browser
- [ ] Frontend should be at `http://localhost:3000`

---

## Phase 3: Integration Testing

### 3.1 Test Login
- [ ] Open browser console (F12)
- [ ] Run this command:
  ```javascript
  import authService from './services/authService';
  authService.login('admin@fauluapp.com', 'Admin@123').then(r => console.log(r));
  ```

- [ ] You should see success response with user data and token

### 3.2 Test Building List
- [ ] In browser console, run:
  ```javascript
  import buildingService from './services/buildingService';
  buildingService.getAllBuildings().then(r => console.log(r));
  ```

- [ ] You should see list of buildings (may be empty initially)

### 3.3 Create Login Component
- [ ] Create `src/components/LoginPage.jsx`:
  ```javascript
  import { useState } from 'react';
  import authService from '../services/authService';

  function LoginPage() {
	const [email, setEmail] = useState('admin@fauluapp.com');
	const [password, setPassword] = useState('Admin@123');
	const [error, setError] = useState('');
	const [loading, setLoading] = useState(false);

	const handleLogin = async (e) => {
	  e.preventDefault();
	  setLoading(true);
	  setError('');

	  const result = await authService.login(email, password);
	  setLoading(false);

	  if (result.success) {
		// Redirect to dashboard or home
		window.location.href = '/dashboard';
	  } else {
		setError(result.error);
	  }
	};

	return (
	  <div className="login-container">
		<h2>Faulu Apartment Management</h2>
		{error && <div className="error">{error}</div>}
		<form onSubmit={handleLogin}>
		  <input
			type="email"
			value={email}
			onChange={(e) => setEmail(e.target.value)}
			placeholder="Email"
			disabled={loading}
		  />
		  <input
			type="password"
			value={password}
			onChange={(e) => setPassword(e.target.value)}
			placeholder="Password"
			disabled={loading}
		  />
		  <button type="submit" disabled={loading}>
			{loading ? 'Logging in...' : 'Login'}
		  </button>
		</form>
	  </div>
	);
  }

  export default LoginPage;
  ```

### 3.4 Create Protected Route
- [ ] Create `src/components/ProtectedRoute.jsx`:
  ```javascript
  import authService from '../services/authService';
  import { Navigate } from 'react-router-dom';

  function ProtectedRoute({ children }) {
	if (!authService.isAuthenticated()) {
	  return <Navigate to="/login" />;
	}
	return children;
  }

  export default ProtectedRoute;
  ```

### 3.5 Update App.jsx with Routing
- [ ] Update main app component to include routes

---

## Phase 4: Handle CORS Issues (if needed)

### 4.1 Check CORS Configuration
- [ ] If you get CORS errors, the backend CORS might be too restrictive
- [ ] Open backend file: `Program.cs`
- [ ] Look for CORS policy configuration

### 4.2 Update CORS in Backend (if needed)
- [ ] In `Program.cs`, find the CORS policy section
- [ ] Update to restrict to frontend origin:
  ```csharp
  options.AddPolicy("Frontend", builder =>
  {
	  builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
		  .AllowAnyMethod()
		  .AllowAnyHeader();
  });
  ```

- [ ] Update app.UseCors to use the policy:
  ```csharp
  app.UseCors("Frontend");
  ```

- [ ] Restart backend: `dotnet run`

---

## Phase 5: Production Preparation

### 5.1 Update Environment for Production
- [ ] Create `.env.production` in frontend root:
  ```env
  REACT_APP_API_URL=https://api.yourdomain.com
  REACT_APP_API_TIMEOUT=30000
  REACT_APP_DEBUG=false
  REACT_APP_LOG_REQUESTS=false
  ```

- [ ] Update backend appsettings.json for production

### 5.2 Build Frontend for Production
- [ ] Build React app:
  ```powershell
  npm run build
  ```

- [ ] This creates optimized build in `build/` folder

### 5.3 Deploy Backend
- [ ] Publish .NET backend:
  ```powershell
  dotnet publish -c Release
  ```

- [ ] Deploy to hosting service (Azure, AWS, etc.)

---

## Troubleshooting

### Issue: "Cannot find module axios"
- **Solution**: Run `npm install axios`

### Issue: CORS Error in browser console
- **Solution**: 
  1. Ensure backend is running
  2. Check CORS configuration in backend Program.cs
  3. Verify API URL in .env matches backend URL
  4. Restart both frontend and backend

### Issue: 401 Unauthorized
- **Solution**:
  1. Check login credentials (admin@fauluapp.com / Admin@123)
  2. Verify token is stored: `localStorage.getItem('authToken')`
  3. Check Network tab to see Authorization header is present
  4. Verify backend is running

### Issue: 404 Not Found
- **Solution**:
  1. Check endpoint URL in Network tab
  2. Verify backend is running and responding
  3. Check resource ID exists (e.g., building with ID 1)
  4. Review backend logs for errors

### Issue: Network timeout
- **Solution**:
  1. Ensure backend is running: `dotnet run`
  2. Verify API URL is correct
  3. Check firewall isn't blocking port 5001
  4. Try increasing timeout in .env: `REACT_APP_API_TIMEOUT=60000`

### Issue: Self-signed certificate warning on HTTPS
- **For Development**: This is normal for localhost; accept the warning
- **For Production**: Use a valid SSL certificate

---

## Quick Reference

### Default Admin Credentials
- **Email**: admin@fauluapp.com
- **Password**: Admin@123

### URLs
- **Backend API**: https://localhost:5001
- **Frontend Dev**: http://localhost:3000
- **API Docs**: https://localhost:5001/swagger (if Swagger is enabled)

### Key Files
- Backend: `C:\Users\ADMIN\Desktop\FauluApartmentAPI\`
- Frontend: `C:\Users\ADMIN\FauluApartmentManagement\Faulu-apartment-management\`
- Services: `src/services/` in frontend
- Environment: `.env` in frontend root

### Common Commands
```powershell
# Backend
cd C:\Users\ADMIN\Desktop\FauluApartmentAPI
dotnet run                           # Start backend
dotnet build                         # Build project
dotnet ef migrations add <name>      # Create migration
dotnet ef database update            # Apply migrations

# Frontend
cd C:\Users\ADMIN\FauluApartmentManagement\Faulu-apartment-management
npm start                            # Start dev server
npm install <package>                # Install dependency
npm run build                        # Build for production
npm test                             # Run tests
```

---

## Next Steps

1. ✅ Free disk space
2. ✅ Build and run backend
3. ✅ Copy service files to frontend
4. ✅ Test login and building endpoints
5. ✅ Create login and dashboard pages
6. ✅ Build CRUD pages for each resource
7. ✅ Add error handling and loading states
8. ✅ Style components with CSS/Tailwind
9. ✅ Deploy to production
10. ✅ Monitor and maintain

---

## Support Resources

- **Backend Documentation**: See `API_DOCUMENTATION.md` in FauluApartmentAPI
- **Integration Guide**: See `FRONTEND_INTEGRATION.md` in FauluApartmentAPI
- **Service Methods**: Check JSDoc comments in each service file
- **React Docs**: https://react.dev
- **Axios Docs**: https://axios-http.com
