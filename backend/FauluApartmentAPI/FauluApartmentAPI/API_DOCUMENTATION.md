# Faulu Apartment Management API - Complete Documentation

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Authentication](#authentication)
4. [API Endpoints](#api-endpoints)
5. [Data Models](#data-models)
6. [Error Handling](#error-handling)
7. [Validation Rules](#validation-rules)
8. [Database Schema](#database-schema)

---

## Overview

**Faulu Apartment Management API** is a comprehensive REST API for managing apartment buildings, units, tenants, leases, payments, and maintenance operations.

### Key Features
- JWT-based authentication and authorization
- Role-based access control (Admin, Manager, Tenant, Owner)
- Complete apartment lifecycle management
- Payment tracking and overdue management
- Maintenance request and work order system
- Automatic database seeding with default admin
- Global exception handling and validation
- Soft delete support for all entities

### Technology Stack
- **.NET 9** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT** - Authentication
- **FluentValidation** - Request validation

---

## Architecture

### Layered Architecture

```
┌─────────────────────────────────────┐
│      API Controllers Layer          │  Handles HTTP requests/responses
├─────────────────────────────────────┤
│      Service Layer                  │  Business logic and validation
├─────────────────────────────────────┤
│      Repository Layer               │  Data access abstraction
├─────────────────────────────────────┤
│      Entity Framework Core          │  ORM layer
├─────────────────────────────────────┤
│      SQL Server Database            │  Data persistence
└─────────────────────────────────────┘
```

### Key Components

#### Controllers
- **AuthController** - User authentication and registration
- **BuildingsController** - Property management
- **UnitsController** - Unit/apartment management
- **TenantsController** - Tenant information
- **LeasesController** - Lease agreements
- **PaymentsController** - Payment tracking
- **MaintenanceController** - Maintenance operations

#### Services
- Business logic layer
- Validation and error handling
- Complex operations (e.g., lease renewal, payment recording)

#### Repositories
- Generic `IRepository<T>` for CRUD operations
- Specialized repositories with domain-specific queries
- Soft delete implementation

#### Models
- Domain entities
- DTOs for API contracts
- Separate create/update/read models

---

## Authentication

### JWT Token Flow

```
1. User submits credentials (email + password)
   ↓
2. Credentials validated against database
   ↓
3. JWT token generated with claims
   ↓
4. Token returned to client
   ↓
5. Client includes token in Authorization header
   ↓
6. Server validates token and authorizes request
```

### Token Structure

JWT tokens include:
- **iss** (Issuer): FauluApartmentAPI
- **aud** (Audience): FauluApartmentClient
- **sub** (Subject): User ID
- **email**: User email
- **name**: Full name
- **roles**: Array of role claims
- **exp** (Expiration): Unix timestamp

### Token Configuration

**appsettings.json**:
```json
"Jwt": {
  "Key": "your-secret-key-minimum-32-characters",
  "Issuer": "FauluApartmentAPI",
  "Audience": "FauluApartmentClient",
  "ExpirationMinutes": 60
}
```

### Authorization Policies

```csharp
"Admin"   - Requires Admin role
"Manager" - Requires Manager or Admin role
"Tenant"  - Requires Tenant role
```

### Roles

| Role | Permissions |
|------|------------|
| **Admin** | Full system access, user management |
| **Manager** | Property management, payments, maintenance |
| **Tenant** | Own leases, payments, service requests |
| **Owner** | Property overview, financial reports |

---

## API Endpoints

### Authentication Endpoints

#### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@fauluapp.com",
  "password": "Admin@123"
}
```

**Response (200 OK)**:
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

#### Register
```
POST /api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "Secure@123",
  "confirmPassword": "Secure@123",
  "userType": "Tenant"
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Registration successful. You can now login",
  "data": {
	"success": true,
	"message": "Registration successful. You can now login",
	"userId": "2"
  }
}
```

---

### Buildings Endpoints

#### List All Buildings
```
GET /api/buildings
Authorization: Bearer <token>
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": [
	{
	  "id": 1,
	  "name": "Downtown Apartments",
	  "address": "123 Main Street",
	  "city": "Nairobi",
	  "postalCode": "00100",
	  "country": "Kenya",
	  "description": "Modern apartment complex",
	  "totalUnits": 50,
	  "ownerId": 1,
	  "propertyValue": 50000000,
	  "occupancyRate": 80,
	  "createdAt": "2024-01-01T00:00:00Z",
	  "updatedAt": null
	}
  ]
}
```

#### Get Building by ID
```
GET /api/buildings/{id}
Authorization: Bearer <token>
```

#### Create Building
```
POST /api/buildings
Authorization: Bearer <token>
Content-Type: application/json

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

#### Update Building
```
PUT /api/buildings/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Name",
  "address": "456 New Street",
  "city": "Nairobi",
  "postalCode": "00200",
  "description": "Updated description",
  "totalUnits": 60,
  "propertyValue": 60000000
}
```

#### Delete Building
```
DELETE /api/buildings/{id}
Authorization: Bearer <token>
```

---

### Units Endpoints

#### List Units by Building
```
GET /api/units/building/{buildingId}
Authorization: Bearer <token>
```

#### Get Unit by ID
```
GET /api/units/{id}
Authorization: Bearer <token>
```

#### Get Vacant Units
```
GET /api/units/building/{buildingId}/vacant
Authorization: Bearer <token>
```

#### Create Unit
```
POST /api/units
Authorization: Bearer <token>
Content-Type: application/json

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
  "amenities": "Balcony, Parking, WiFi",
  "notes": "Corner unit with great view"
}
```

#### Update Unit
```
PUT /api/units/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "unitNumber": "101A",
  "monthlyRent": 50000,
  "status": "Occupied"
}
```

#### Delete Unit
```
DELETE /api/units/{id}
Authorization: Bearer <token>
```

---

### Tenants Endpoints

#### List All Tenants
```
GET /api/tenants
Authorization: Bearer <token>
```

#### Get Tenant by ID
```
GET /api/tenants/{id}
Authorization: Bearer <token>
```

#### Search Tenants
```
GET /api/tenants/search/{name}
Authorization: Bearer <token>
```

Example: `GET /api/tenants/search/John`

#### Create Tenant
```
POST /api/tenants
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+254712345678",
  "identificationNumber": "12345678",
  "identificationType": "NationalID",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "occupation": "Software Engineer",
  "employer": "Tech Company",
  "emergencyContactName": "Jane Doe",
  "emergencyContactPhone": "+254712345679",
  "emergencyContactRelation": "Sister"
}
```

#### Update Tenant
```
PUT /api/tenants/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "email": "newemail@example.com",
  "phoneNumber": "+254712345680",
  "status": "Active"
}
```

#### Delete Tenant
```
DELETE /api/tenants/{id}
Authorization: Bearer <token>
```

---

### Leases Endpoints

#### Get Lease by ID
```
GET /api/leases/{id}
Authorization: Bearer <token>
```

#### Get Leases by Tenant
```
GET /api/leases/tenant/{tenantId}
Authorization: Bearer <token>
```

#### Get Leases by Unit
```
GET /api/leases/unit/{unitId}
Authorization: Bearer <token>
```

#### Get Expiring Leases
```
GET /api/leases/expiring/{daysUntilExpiry}
Authorization: Bearer <token>
```

Example: `GET /api/leases/expiring/30` (Leases expiring within 30 days)

#### Create Lease
```
POST /api/leases
Authorization: Bearer <token>
Content-Type: application/json

{
  "tenantId": 1,
  "unitId": 1,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2025-01-01T00:00:00Z",
  "monthlyRent": 45000,
  "depositAmount": 45000,
  "secondaryDeposit": 0,
  "leaseTermType": "12-Months",
  "renewalPolicy": "Auto",
  "allowsPets": false,
  "leaseDocument": null
}
```

#### Update Lease
```
PUT /api/leases/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "endDate": "2025-06-01T00:00:00Z",
  "monthlyRent": 50000,
  "status": "Active"
}
```

#### Renew Lease
```
POST /api/leases/{id}/renew
Authorization: Bearer <token>
Content-Type: application/json

{
  "newEndDate": "2026-01-01T00:00:00Z"
}
```

#### Terminate Lease
```
POST /api/leases/{id}/terminate
Authorization: Bearer <token>
```

---

### Payments Endpoints

#### Get Payment by ID
```
GET /api/payments/{id}
Authorization: Bearer <token>
```

#### Get Payments by Tenant
```
GET /api/payments/tenant/{tenantId}
Authorization: Bearer <token>
```

#### Get Payments by Lease
```
GET /api/payments/lease/{leaseId}
Authorization: Bearer <token>
```

#### Get Overdue Payments
```
GET /api/payments/status/overdue
Authorization: Bearer <token>
```

#### Get Total Outstanding
```
GET /api/payments/tenant/{tenantId}/outstanding
Authorization: Bearer <token>
```

**Response**:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": 45000
}
```

#### Create Payment
```
POST /api/payments
Authorization: Bearer <token>
Content-Type: application/json

{
  "tenantId": 1,
  "leaseId": 1,
  "amount": 45000,
  "dueDate": "2024-02-01T00:00:00Z",
  "paymentType": "RentPayment"
}
```

#### Record Payment
```
POST /api/payments/{id}/record
Authorization: Bearer <token>
Content-Type: application/json

{
  "paymentId": 1,
  "amount": 45000,
  "paymentMethod": "Bank",
  "transactionReference": "TXN123456"
}
```

#### Update Payment
```
PUT /api/payments/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "amount": 45000,
  "status": "Pending",
  "lateFees": 0
}
```

#### Delete Payment
```
DELETE /api/payments/{id}
Authorization: Bearer <token>
```

---

### Maintenance Endpoints

#### Get Maintenance Order by ID
```
GET /api/maintenance/{id}
Authorization: Bearer <token>
```

#### Get Orders by Building
```
GET /api/maintenance/building/{buildingId}
Authorization: Bearer <token>
```

#### Get Open Orders
```
GET /api/maintenance/status/open
Authorization: Bearer <token>
```

#### Get Urgent Orders
```
GET /api/maintenance/priority/urgent
Authorization: Bearer <token>
```

#### Create Maintenance Order
```
POST /api/maintenance
Authorization: Bearer <token>
Content-Type: application/json

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

#### Update Maintenance Order
```
PUT /api/maintenance/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "status": "InProgress",
  "priority": "Urgent",
  "scheduledDate": "2024-01-15T10:00:00Z",
  "actualCost": 6000
}
```

#### Complete Maintenance Order
```
POST /api/maintenance/{id}/complete
Authorization: Bearer <token>
```

#### Delete Maintenance Order
```
DELETE /api/maintenance/{id}
Authorization: Bearer <token>
```

---

## Data Models

### Building
```json
{
  "id": 1,
  "name": "string",
  "address": "string",
  "city": "string",
  "postalCode": "string",
  "country": "string",
  "description": "string",
  "totalUnits": 0,
  "ownerId": 0,
  "propertyValue": 0,
  "yearBuilt": null,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "isDeleted": false
}
```

### Unit
```json
{
  "id": 1,
  "unitNumber": "string",
  "unitType": "string",
  "buildingId": 0,
  "floorNumber": 0,
  "monthlyRent": 0,
  "deposit": 0,
  "bedroomCount": 0,
  "bathroomCount": 0,
  "squareFootage": 0,
  "isFurnished": false,
  "status": "Vacant",
  "amenities": "string",
  "notes": "string",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "isDeleted": false
}
```

### Tenant
```json
{
  "id": 1,
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phoneNumber": "string",
  "identificationNumber": "string",
  "identificationType": "NationalID",
  "dateOfBirth": "2024-01-01T00:00:00Z",
  "occupation": "string",
  "employer": "string",
  "emergencyContactName": "string",
  "emergencyContactPhone": "string",
  "emergencyContactRelation": "string",
  "status": "Active",
  "notes": "string",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "isDeleted": false
}
```

### Lease
```json
{
  "id": 1,
  "tenantId": 0,
  "unitId": 0,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T00:00:00Z",
  "monthlyRent": 0,
  "depositAmount": 0,
  "secondaryDeposit": 0,
  "status": "Active",
  "leaseTermType": "12-Months",
  "renewalPolicy": "Auto",
  "allowsPets": false,
  "leaseDocument": "string",
  "signedDate": null,
  "notes": "string",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "isDeleted": false
}
```

### Payment
```json
{
  "id": 1,
  "tenantId": 0,
  "leaseId": 0,
  "amount": 0,
  "lateFees": 0,
  "paymentType": "RentPayment",
  "status": "Pending",
  "dueDate": "2024-02-01T00:00:00Z",
  "paidDate": null,
  "paymentMethod": "Bank",
  "transactionReference": "string",
  "notes": "string",
  "paidAmount": 0,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "isDeleted": false
}
```

### MaintenanceOrder
```json
{
  "id": 1,
  "buildingId": 0,
  "unitId": 0,
  "title": "string",
  "description": "string",
  "priority": "Normal",
  "status": "Open",
  "category": "string",
  "reportedDate": "2024-01-01T00:00:00Z",
  "scheduledDate": null,
  "completedDate": null,
  "estimatedCost": 0,
  "actualCost": 0,
  "assignedVendor": "string",
  "notes": "string",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "isDeleted": false
}
```

---

## Error Handling

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "errors": null
}
```

### HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Authorization required |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error |

### Common Errors

#### Validation Error
```json
{
  "success": false,
  "message": "Building name is required",
  "errors": {
	"name": ["Building name is required"]
  }
}
```

#### Unauthorized
```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

#### Not Found
```json
{
  "success": false,
  "message": "Building not found"
}
```

---

## Validation Rules

### Building Creation
- **name**: Required, max 255 characters
- **address**: Required, max 500 characters
- **city**: Required, max 100 characters
- **totalUnits**: Required, > 0
- **ownerId**: Required, valid user ID

### Unit Creation
- **unitNumber**: Required
- **buildingId**: Required, valid building ID
- **monthlyRent**: Required, > 0
- **bedroomCount**: >= 0
- **bathroomCount**: >= 0

### Tenant Creation
- **firstName**: Required, max 100 characters
- **lastName**: Required, max 100 characters
- **email**: Required, valid email format
- **phoneNumber**: Required, valid format (+254712345678)
- **identificationNumber**: Required

### Lease Creation
- **tenantId**: Required, valid tenant ID
- **unitId**: Required, valid unit ID
- **monthlyRent**: Required, > 0
- **startDate**: Required, >= today
- **endDate**: Required, > startDate

### Payment Creation
- **tenantId**: Required, valid tenant ID
- **leaseId**: Required, valid lease ID
- **amount**: Required, > 0
- **dueDate**: Required

---

## Database Schema

### Entity Relationships

```
User (1) ──────────────── (Many) Building
  │
  └──────────────── (Many) ServiceRequest

Building (1) ──────────────── (Many) Unit
  │
  └──────────────── (Many) MaintenanceOrder

Unit (1) ──────────────── (Many) Lease

Tenant (1) ──────────────── (Many) Lease
  │
  ├──────────────── (Many) Payment
  │
  └──────────────── (Many) ServiceRequest

Lease (1) ──────────────── (Many) Payment
```

### Key Indexes
- `Tenant.IdentificationNumber` - UNIQUE
- `Tenant.Email` - INDEX
- `Payment.Status` - INDEX
- `Payment.DueDate` - INDEX
- `MaintenanceOrder.Status` - INDEX
- `MaintenanceOrder.Priority` - INDEX
- `ServiceRequest.Status` - INDEX
- `ServiceRequest.Priority` - INDEX
- `Unit.BuildingId + UnitNumber` - UNIQUE

### Soft Delete
All entities have `IsDeleted` boolean field. Query filters automatically exclude deleted records.

---

## Response Examples

### Success Response with Data
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": null
}
```

### Success Response without Data
```json
{
  "success": true,
  "message": "Building deleted successfully",
  "errors": null
}
```

### Error Response
```json
{
  "success": false,
  "message": "Building name is required",
  "errors": {
	"name": ["Building name is required"]
  }
}
```

---

**Documentation Generated**: January 2024
**API Version**: 1.0
**Status**: Production Ready (pending deployment)
