# Faulu Apartment Management API - Quick Start Guide

## Prerequisites
- .NET 9 SDK installed
- SQL Server LocalDB or SQL Server instance
- Visual Studio 2022 or Visual Studio Code
- Disk space for NuGet packages and build output

## Getting Started

### Step 1: Free Up Disk Space
The build failed due to insufficient disk space. Please:
1. Delete temporary files from `C:\Users\ADMIN\AppData\Local\Temp`
2. Run `Disk Cleanup` utility
3. Target at least 5GB free space on C: drive

### Step 2: Restore NuGet Packages
```bash
cd C:\Users\ADMIN\Desktop\FauluApartmentAPI
dotnet restore
```

### Step 3: Build the Project
```bash
dotnet build
```

### Step 4: Create Database Migration
```bash
dotnet ef migrations add InitialCreate
```

### Step 5: Update Database
```bash
dotnet ef database update
```

### Step 6: Run the Application
```bash
dotnet run
```

The API will start at: `https://localhost:5001` or `http://localhost:5000`

## API Testing

### 1. Login to Get JWT Token
**Endpoint**: `POST /api/auth/login`

**Request**:
```json
{
  "email": "admin@fauluapp.com",
  "password": "Admin@123"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
	"userId": 1,
	"email": "admin@fauluapp.com",
	"firstName": "Admin",
	"lastName": "User",
	"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
	"refreshToken": "...",
	"roles": ["Admin"]
  }
}
```

### 2. Use Token for Authenticated Requests
Add the token to the Authorization header:
```
Authorization: Bearer <token>
```

### 3. Create a Building
**Endpoint**: `POST /api/buildings`

**Request**:
```json
{
  "name": "Downtown Apartments",
  "address": "123 Main Street",
  "city": "Nairobi",
  "postalCode": "00100",
  "description": "Modern apartment complex",
  "totalUnits": 50,
  "ownerId": 1,
  "propertyValue": 50000000
}
```

### 4. Create a Unit
**Endpoint**: `POST /api/units`

**Request**:
```json
{
  "unitNumber": "101",
  "unitType": "1-Bedroom",
  "buildingId": 1,
  "floorNumber": 1,
  "monthlyRent": 45000,
  "deposit": 45000,
  "bedroomCount": 1,
  "bathroomCount": 1,
  "squareFootage": 550,
  "isFurnished": true,
  "amenities": "Balcony, Parking, WiFi"
}
```

### 5. Create a Tenant
**Endpoint**: `POST /api/tenants`

**Request**:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+254712345678",
  "identificationNumber": "12345678",
  "identificationType": "NationalID",
  "dateOfBirth": "1990-01-15",
  "occupation": "Software Engineer",
  "employer": "Tech Company",
  "emergencyContactName": "Jane Doe",
  "emergencyContactPhone": "+254712345679",
  "emergencyContactRelation": "Sister"
}
```

### 6. Create a Lease
**Endpoint**: `POST /api/leases`

**Request**:
```json
{
  "tenantId": 1,
  "unitId": 1,
  "startDate": "2024-01-01",
  "endDate": "2025-01-01",
  "monthlyRent": 45000,
  "depositAmount": 45000,
  "leaseTermType": "12-Months",
  "renewalPolicy": "Auto",
  "allowsPets": false
}
```

### 7. Create a Payment
**Endpoint**: `POST /api/payments`

**Request**:
```json
{
  "tenantId": 1,
  "leaseId": 1,
  "amount": 45000,
  "dueDate": "2024-02-01",
  "paymentType": "RentPayment"
}
```

### 8. Record a Payment
**Endpoint**: `POST /api/payments/{paymentId}/record`

**Request**:
```json
{
  "paymentId": 1,
  "amount": 45000,
  "paymentMethod": "Bank",
  "transactionReference": "TXN123456"
}
```

### 9. Create Maintenance Order
**Endpoint**: `POST /api/maintenance`

**Request**:
```json
{
  "buildingId": 1,
  "unitId": 1,
  "title": "Fix Broken Door",
  "description": "Bedroom door lock is broken",
  "priority": "High",
  "category": "Carpentry",
  "estimatedCost": 5000,
  "assignedVendor": "John's Repairs"
}
```

### 10. Get Building Occupancy
**Endpoint**: `GET /api/buildings/{id}`

Returns building details including occupancy rate:
```json
{
  "success": true,
  "data": {
	"id": 1,
	"name": "Downtown Apartments",
	"address": "123 Main Street",
	"city": "Nairobi",
	"totalUnits": 50,
	"occupancyRate": 80,
	"createdAt": "2024-01-01T00:00:00Z"
  }
}
```

## Available Endpoints Summary

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register new user

### Buildings
- `GET /api/buildings` - List all
- `GET /api/buildings/{id}` - Get by ID
- `POST /api/buildings` - Create
- `PUT /api/buildings/{id}` - Update
- `DELETE /api/buildings/{id}` - Delete

### Units
- `GET /api/units/building/{buildingId}` - List by building
- `GET /api/units/{id}` - Get by ID
- `GET /api/units/building/{buildingId}/vacant` - Get vacant units
- `POST /api/units` - Create
- `PUT /api/units/{id}` - Update
- `DELETE /api/units/{id}` - Delete

### Tenants
- `GET /api/tenants` - List all
- `GET /api/tenants/{id}` - Get by ID
- `GET /api/tenants/search/{name}` - Search by name
- `POST /api/tenants` - Create
- `PUT /api/tenants/{id}` - Update
- `DELETE /api/tenants/{id}` - Delete

### Leases
- `GET /api/leases/{id}` - Get by ID
- `GET /api/leases/tenant/{tenantId}` - List by tenant
- `GET /api/leases/unit/{unitId}` - List by unit
- `GET /api/leases/expiring/{daysUntilExpiry}` - Get expiring
- `POST /api/leases` - Create
- `PUT /api/leases/{id}` - Update
- `POST /api/leases/{id}/renew` - Renew
- `POST /api/leases/{id}/terminate` - Terminate

### Payments
- `GET /api/payments/{id}` - Get by ID
- `GET /api/payments/tenant/{tenantId}` - List by tenant
- `GET /api/payments/lease/{leaseId}` - List by lease
- `GET /api/payments/status/overdue` - Get overdue
- `GET /api/payments/tenant/{tenantId}/outstanding` - Get outstanding
- `POST /api/payments` - Create
- `POST /api/payments/{id}/record` - Record payment
- `PUT /api/payments/{id}` - Update
- `DELETE /api/payments/{id}` - Delete

### Maintenance
- `GET /api/maintenance/{id}` - Get by ID
- `GET /api/maintenance/building/{buildingId}` - List by building
- `GET /api/maintenance/status/open` - Get open orders
- `GET /api/maintenance/priority/urgent` - Get urgent orders
- `POST /api/maintenance` - Create
- `PUT /api/maintenance/{id}` - Update
- `POST /api/maintenance/{id}/complete` - Complete
- `DELETE /api/maintenance/{id}` - Delete

## Testing with Postman

1. Import the endpoints into Postman
2. Set the base URL to `https://localhost:5001`
3. Login first to get the JWT token
4. Add token to `Authorization` header for subsequent requests
5. Use the request examples above

## Database

The application uses SQL Server LocalDB by default. Connection string:
```
Server=(localdb)\mssqllocaldb;Database=FauluApartmentDb;Trusted_Connection=true;
```

To change to a different database:
1. Update `appsettings.json` with your connection string
2. Run migrations again: `dotnet ef database update`

## Troubleshooting

### Build Failed - Disk Space
- Free up disk space (at least 5GB)
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Run `dotnet clean` and `dotnet build` again

### Database Connection Failed
- Ensure SQL Server is running
- Check connection string in `appsettings.json`
- Run `dotnet ef database update` again

### Migration Issues
- Delete bin/obj folders: `rm -r bin obj`
- Clear NuGet cache
- Run `dotnet ef migrations remove` (if needed)
- Run `dotnet ef migrations add InitialCreate`
- Run `dotnet ef database update`

### JWT Token Errors
- Update `Jwt:Key` in appsettings.json (use a secure key)
- Ensure token is in `Authorization: Bearer <token>` format
- Check token hasn't expired

## Security Checklist

- [ ] Change default admin password
- [ ] Update JWT secret key to a secure value (32+ characters)
- [ ] Update CORS policy for production
- [ ] Enable HTTPS enforcement
- [ ] Use environment variables for sensitive data
- [ ] Set up database encryption
- [ ] Implement rate limiting
- [ ] Add request logging
- [ ] Configure backups
- [ ] Set up monitoring and alerts

## Support

For issues or questions:
1. Check the error message in the API response
2. Review logs in console output
3. Check database with SQL Server Management Studio
4. Review code comments in implementation files
5. Refer to IMPLEMENTATION_SUMMARY.md for architecture details
