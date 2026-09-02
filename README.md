# Faulu Apartment Management

A full-stack SaaS platform designed for small-scale landlords in Kenya, providing a modern alternative to traditional real estate management agencies.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

## Overview

Faulu Apartment Management simplifies property management for Kenyan landlords by automating key operational tasks. The platform eliminates intermediaries and provides direct control over tenant relationships, lease agreements, maintenance schedules, and rent collection.

**Key Target Users:** Small-scale landlords managing residential properties in Kenya

**Primary Problem Solved:** Reducing management overhead and streamlining rent collection through integrated M-Pesa payments

## Features

### Tenant Management
- Digital tenant onboarding and application process
- Tenant profile management and communication portal
- Document storage and verification tracking
- Tenant history and reference checks
- Contact information management
- Emergency contact tracking

### Lease Management
- Digital lease agreement creation and execution
- Lease term tracking and renewal notifications
- Rent escalation management
- Lease document generation and storage
- Multiple lease types support
- Automatic lease expiry alerts

### Unit Management
- Property and unit inventory management
- Unit status tracking (occupied, vacant, maintenance)
- Multi-property support
- Unit history and maintenance logs
- Unit features and amenities documentation
- Rent rate management by unit

### Maintenance Tracking
- Work order creation and assignment
- Maintenance request tracking from tenants
- Contractor management
- Maintenance cost tracking and budgeting
- Priority level classification
- Maintenance history per unit
- Completion status tracking

### Rent Collection
- Integrated M-Pesa payment collection via Daraja API
- Payment tracking and reconciliation
- Automated payment reminders
- Rent arrears management
- Payment history and receipts
- Multiple payment methods support
- Payment scheduling and automation

## Tech Stack

### Frontend
- **React** with Vite for fast development and build
- **JavaScript/CSS** for styling and interactivity
- **SCSS** for advanced styling patterns
- Modern ES6+ JavaScript features
- Component-based architecture
- State management for complex interactions

### Backend
- **.NET/C#** for robust, scalable API
- RESTful API architecture
- Entity Framework for data access
- Database-first approach
- Async/await patterns for performance
- Dependency injection and middleware

### Database
- SQL Server for reliable data storage
- Entity Framework Core for ORM
- Database migrations support
- Transaction support for data integrity

### Integrations
- **Safaricom Daraja API** for M-Pesa payments
- Payment processing and settlement
- Real-time payment notifications
- Payment callback handling

### Infrastructure
- **Docker** for containerization
- Docker Compose for multi-container orchestration
- Cloud-ready deployment setup
- Environment-based configuration

## Prerequisites

### Frontend
- Node.js (v16 or higher)
- npm or yarn package manager
- Modern browser with ES6+ support
- Git for version control

### Backend
- .NET SDK (v7.0 or higher recommended)
- C# IDE (Visual Studio, VS Code, or JetBrains Rider)
- SQL Server (local or remote instance)
- Git for version control

### Optional
- Docker and Docker Compose for containerized development
- M-Pesa developer account (for payment integration testing)
- Postman or similar tool for API testing

## Installation

### Frontend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/wallstreetwolf1245/Faulu-Apartment-Management.git
   cd Faulu-Apartment-Management/frontend/Faulu-apartment-management
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start the development server**
   ```bash
   npm run dev
   ```
   The application will be available at `http://localhost:5173`

4. **Build for production**
   ```bash
   npm run build
   ```

5. **Preview production build**
   ```bash
   npm run preview
   ```

### Backend Setup

1. **Clone the repository** (if not already done)
   ```bash
   git clone https://github.com/wallstreetwolf1245/Faulu-Apartment-Management.git
   ```

2. **Navigate to backend directory**
   ```bash
   cd Faulu-Apartment-Management/backend
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Update database connection string**
   - Edit `appsettings.json` with your SQL Server connection details

5. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

6. **Run the API server**
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5000`

### Docker Setup

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

2. **Access services**
   - Frontend: `http://localhost:3000`
   - Backend API: `http://localhost:5000`
   - Database: Configured in docker-compose.yml

## Configuration

### Frontend Environment Variables

Create a `.env.local` file in the frontend directory:

```
VITE_API_URL=http://localhost:5000
VITE_MPESA_ENV=sandbox
VITE_APP_NAME=Faulu Apartment Management
VITE_LOG_LEVEL=info
```

### Backend Configuration

Update `appsettings.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FauluDB;Integrated Security=true;"
  },
  "MpesaSettings": {
    "ConsumerKey": "your_daraja_consumer_key",
    "ConsumerSecret": "your_daraja_consumer_secret",
    "BusinessShortCode": "your_business_shortcode",
    "PassKey": "your_passkey",
    "Environment": "sandbox"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Jwt": {
    "SecretKey": "your_secret_key_here_min_32_chars",
    "Issuer": "faulu.co.ke",
    "Audience": "faulu-app",
    "ExpirationMinutes": 60
  }
}
```

### appsettings.Production.json

For production deployments:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your_production_connection_string"
  },
  "MpesaSettings": {
    "Environment": "production"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Error"
    }
  }
}
```

### M-Pesa Integration Setup

1. **Create account**
   - Visit [Safaricom Developer Portal](https://developer.safaricom.co.ke)
   - Sign up or log in with your credentials

2. **Register application**
   - Create a new application in the developer portal
   - Choose "M-Pesa" as the API category
   - Accept the terms and conditions

3. **Obtain credentials**
   - Consumer Key
   - Consumer Secret
   - Business Short Code
   - Pass Key

4. **Configure in backend**
   - Add credentials to `appsettings.json`
   - Use sandbox credentials for testing first

5. **Test integration**
   - Use Safaricom sandbox environment for testing
   - Verify callback URLs are correctly configured
   - Test payment flow end-to-end

6. **Go live**
   - Switch to production credentials
   - Update environment to "production"
   - Monitor transactions and handle errors

## Usage

### For Landlords

1. **Sign Up/Login**
   - Create account with email and password
   - Verify email address
   - Log in with credentials

2. **Add Properties**
   - Navigate to Properties section
   - Click "Add New Property"
   - Enter property details (address, type, number of units)
   - Upload property images
   - Set base rent amount

3. **Manage Units**
   - Add individual units/apartments for each property
   - Set unit-specific rent amounts
   - Upload unit images
   - Add unit features and amenities
   - Track unit status (occupied/vacant)

4. **Onboard Tenants**
   - Create tenant profiles
   - Upload identity documents
   - Add emergency contacts
   - Generate and sign lease agreements digitally
   - Set lease start and end dates

5. **Manage Payments**
   - Receive M-Pesa rent payments directly
   - View payment history and receipts
   - Set up payment reminders
   - Track late payments and arrears
   - Generate payment reports

6. **Track Maintenance**
   - Receive maintenance requests from tenants
   - Create work orders
   - Assign to contractors
   - Track completion status
   - Manage maintenance costs

7. **View Reports**
   - Access payment history and summaries
   - View occupancy reports
   - Track arrears and late payments
   - Generate financial reports
   - Export data for accounting

### For Developers

#### Development Commands

```bash
# Frontend
npm run dev              # Start dev server with hot reload
npm run build           # Create production build
npm run preview         # Preview production build locally
npm run lint            # Run ESLint for code quality
npm run format          # Format code with Prettier

# Backend
dotnet build            # Build the solution
dotnet run              # Run the application
dotnet test             # Run unit tests
dotnet publish          # Publish for deployment
dotnet ef migrations add MigrationName  # Create new migration
dotnet ef database update              # Apply migrations
```

#### Testing

```bash
# Frontend
npm run test            # Run Jest tests
npm run test:coverage   # Generate coverage report

# Backend
dotnet test /p:CollectCoverage=true  # Run tests with coverage
```

#### Code Quality

```bash
# Frontend
npm run lint            # Check code quality
npm run format          # Auto-format code

# Backend
dotnet format           # Format C# code
```

## Project Structure

```
Faulu-Apartment-Management/
├── frontend/
│   └── Faulu-apartment-management/
│       ├── src/
│       │   ├── components/
│       │   │   ├── Common/           # Shared components (Header, Footer, Sidebar)
│       │   │   ├── Tenants/          # Tenant-related components
│       │   │   ├── Properties/       # Property management components
│       │   │   ├── Payments/         # Payment-related components
│       │   │   └── Maintenance/      # Maintenance-related components
│       │   ├── pages/
│       │   │   ├── Dashboard.jsx
│       │   │   ├── Properties.jsx
│       │   │   ├── Tenants.jsx
│       │   │   ├── Payments.jsx
│       │   │   ├── Maintenance.jsx
│       │   │   ├── Reports.jsx
│       │   │   ├── Login.jsx
│       │   │   └── Settings.jsx
│       │   ├── hooks/
│       │   │   ├── useAuth.js
│       │   │   ├── useFetch.js
│       │   │   └── useForm.js
│       │   ├── services/
│       │   │   ├── api.js            # Axios/Fetch configuration
│       │   │   ├── auth.js           # Authentication service
│       │   │   ├── properties.js     # Properties API calls
│       │   │   ├── tenants.js        # Tenants API calls
│       │   │   ├── payments.js       # Payments API calls
│       │   │   └── maintenance.js    # Maintenance API calls
│       │   ├── styles/
│       │   │   ├── global.scss
│       │   │   ├── variables.scss
│       │   │   └── components.scss
│       │   ├── utils/
│       │   │   ├── formatters.js
│       │   │   ├── validators.js
│       │   │   └── constants.js
│       │   ├── App.jsx
│       │   └── main.jsx
│       ├── public/
│       │   └── images/
│       ├── vite.config.js
│       ├── package.json
│       └── README.md
├── backend/
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Property.cs
│   │   ├── Unit.cs
│   │   ├── Tenant.cs
│   │   ├── Lease.cs
│   │   ├── Payment.cs
│   │   ├── Maintenance.cs
│   │   └── MaintenanceWorkOrder.cs
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── PropertiesController.cs
│   │   ├── TenantsController.cs
│   │   ├── LeaseController.cs
│   │   ├── PaymentsController.cs
│   │   └── MaintenanceController.cs
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── PropertyService.cs
│   │   ├── TenantService.cs
│   │   ├── PaymentService.cs
│   │   ├── MpesaService.cs
│   │   └── MaintenanceService.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Migrations/
│   │   └── Repositories/
│   ├── Middleware/
│   │   ├── ErrorHandlingMiddleware.cs
│   │   └── AuthMiddleware.cs
│   ├── DTOs/
│   │   ├── UserDTO.cs
│   │   ├── PropertyDTO.cs
│   │   ├── TenantDTO.cs
│   │   └── PaymentDTO.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── Faulu.Backend.csproj
├── docker-compose.yml
├── Dockerfile
├── .gitignore
├── LICENSE
└── README.md
```

## API Documentation

### Authentication Endpoints

```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET /api/auth/profile
```

### Property Endpoints

```
GET /api/properties                 # Get all user properties
GET /api/properties/{id}            # Get property details
POST /api/properties                # Create new property
PUT /api/properties/{id}            # Update property
DELETE /api/properties/{id}         # Delete property
```

### Unit Endpoints

```
GET /api/properties/{propertyId}/units
POST /api/properties/{propertyId}/units
PUT /api/units/{id}
DELETE /api/units/{id}
```

### Tenant Endpoints

```
GET /api/tenants                    # Get all tenants
GET /api/tenants/{id}               # Get tenant details
POST /api/tenants                   # Create new tenant
PUT /api/tenants/{id}               # Update tenant
DELETE /api/tenants/{id}            # Delete tenant
```

### Lease Endpoints

```
GET /api/leases
GET /api/leases/{id}
POST /api/leases
PUT /api/leases/{id}
DELETE /api/leases/{id}
```

### Payment Endpoints

```
GET /api/payments                   # Get payment history
GET /api/payments/{id}              # Get payment details
POST /api/payments/initiate         # Initiate M-Pesa payment
GET /api/payments/callback          # M-Pesa callback handler
GET /api/payments/arrears           # Get arrears report
```

### Maintenance Endpoints

```
GET /api/maintenance/workorders
POST /api/maintenance/workorders
PUT /api/maintenance/workorders/{id}
GET /api/maintenance/workorders/{id}
```

For detailed API documentation, refer to [API-DOCS.md](./backend/API-DOCS.md)

## Contributing

### Getting Started

1. Fork the repository
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/Faulu-Apartment-Management.git
   ```
3. Create a new branch for your feature:
   ```bash
   git checkout -b feature/amazing-feature
   ```

### Development Workflow

1. Make your changes
2. Test thoroughly
3. Commit with clear messages:
   ```bash
   git commit -m 'Add amazing feature'
   ```
4. Push to your fork:
   ```bash
   git push origin feature/amazing-feature
   ```
5. Open a Pull Request with:
   - Clear description of changes
   - Reference to related issues
   - Screenshots for UI changes
   - Test results

### Coding Standards

- Frontend: Follow React and JavaScript best practices
- Backend: Follow C# and .NET conventions
- Use meaningful variable and function names
- Add comments for complex logic
- Write unit tests for new features
- Keep commits atomic and focused

### Pull Request Process

1. Update documentation as needed
2. Add tests for new functionality
3. Ensure all tests pass locally
4. Request review from maintainers
5. Address feedback and requested changes
6. Merge once approved

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

### Getting Help

For support, feature requests, or bug reports:

- **Issues**: Open an [issue](https://github.com/wallstreetwolf1245/Faulu-Apartment-Management/issues)
- **Discussions**: Join [discussions](https://github.com/wallstreetwolf1245/Faulu-Apartment-Management/discussions)
- **Contact**: [your contact method]
- **Documentation**: [link to docs]

### Common Issues

#### M-Pesa Integration Not Working
- Verify consumer key and secret are correct
- Ensure you're using sandbox credentials for testing
- Check that callback URL is accessible from internet
- Review M-Pesa transaction logs for error details

#### Database Connection Errors
- Confirm SQL Server is running
- Verify connection string is correct
- Check database user has proper permissions
- Ensure database exists or migrations have run

#### Build Failures
- Clear npm cache: `npm cache clean --force`
- Delete node_modules: `rm -rf node_modules`
- Reinstall: `npm install`
- Check Node.js version matches prerequisites

#### CORS Issues
- Verify backend CORS policy includes frontend URL
- Check frontend API URL is correctly configured
- Ensure requests use correct HTTP methods

### Troubleshooting

1. Check logs in browser console (frontend) and terminal (backend)
2. Verify environment variables are set correctly
3. Ensure all services are running (frontend, backend, database)
4. Clear browser cache and restart dev servers
5. Check GitHub issues for similar problems

---

Built with dedication for Kenyan landlords
