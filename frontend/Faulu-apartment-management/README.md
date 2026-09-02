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

### Lease Management
- Digital lease agreement creation and execution
- Lease term tracking and renewal notifications
- Rent escalation management
- Lease document generation and storage

### Unit Management
- Property and unit inventory management
- Unit status tracking (occupied, vacant, maintenance)
- Multi-property support
- Unit history and maintenance logs

### Maintenance Tracking
- Work order creation and assignment
- Maintenance request tracking from tenants
- Contractor management
- Maintenance cost tracking and budgeting

### Rent Collection
- Integrated M-Pesa payment collection via Daraja API
- Payment tracking and reconciliation
- Automated payment reminders
- Rent arrears management
- Payment history and receipts

## Tech Stack

### Frontend
- **React** with Vite for fast development and build
- **JavaScript/CSS** for styling and interactivity
- **SCSS** for advanced styling patterns

### Backend
- **.NET/C#** for robust, scalable API
- RESTful API architecture
- Entity Framework for data access

### Integrations
- **Safaricom Daraja API** for M-Pesa payments
- Payment processing and settlement

### Infrastructure
- **Docker** for containerization
- Cloud-ready deployment setup

## Prerequisites

### Frontend
- Node.js (v16 or higher)
- npm or yarn package manager
- Modern browser with ES6+ support

### Backend
- .NET SDK (version specified in project)
- C# IDE (Visual Studio, VS Code, or JetBrains Rider)

### Optional
- Docker and Docker Compose for containerized development
- M-Pesa developer account (for payment integration testing)

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

### Backend Setup

1. **Navigate to backend directory**
   ```bash
   cd ../backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the API server**
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5000`

## Configuration

### Frontend Environment Variables

Create a `.env.local` file in the frontend directory:

```
VITE_API_URL=http://localhost:5000
VITE_MPESA_ENV=sandbox  # Use 'sandbox' for testing, 'production' for live
```

### Backend Configuration

Update `appsettings.json` with:

```json
{
  "MpesaSettings": {
    "ConsumerKey": "your_daraja_consumer_key",
    "ConsumerSecret": "your_daraja_consumer_secret",
    "BusinessShortCode": "your_business_shortcode",
    "PassKey": "your_passkey"
  },
  "DatabaseConnection": "your_connection_string"
}
```

### M-Pesa Integration

1. Create an account at [Safaricom Developer Portal](https://developer.safaricom.co.ke)
2. Register your application and obtain credentials
3. Configure credentials in backend settings
4. Test using sandbox environment first

## Usage

### For Landlords

1. **Sign Up/Login** - Create account or log in with credentials
2. **Add Properties** - Register rental units/apartments
3. **Onboard Tenants** - Add tenant information and upload lease agreements
4. **Manage Payments** - Receive M-Pesa rent payments directly
5. **Track Maintenance** - Log and monitor maintenance requests
6. **View Reports** - Access payment history, arrears, and occupancy reports

### For Developers

```bash
# Development
npm run dev              # Start dev server
npm run lint            # Run ESLint
npm run build           # Production build

# Backend
dotnet build            # Build solution
dotnet test             # Run tests
dotnet publish          # Publish for deployment
```

## Project Structure

```
Faulu-Apartment-Management/
├── frontend/
│   └── Faulu-apartment-management/
│       ├── src/
│       │   ├── components/    # Reusable React components
│       │   ├── pages/         # Page components
│       │   ├── hooks/         # Custom React hooks
│       │   ├── services/      # API services
│       │   └── App.jsx
│       ├── vite.config.js
│       └── package.json
├── backend/
│   ├── Models/
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   └── appsettings.json
└── docker-compose.yml
```

## API Documentation

Refer to the backend API documentation for available endpoints:

- `GET /api/properties` - Retrieve user properties
- `POST /api/tenants` - Add new tenant
- `GET /api/payments` - Retrieve payment history
- `POST /api/maintenance/workorders` - Create maintenance request

[View full API documentation](./backend/API-DOCS.md)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support, feature requests, or bug reports:

- Open an [issue](https://github.com/wallstreetwolf1245/Faulu-Apartment-Management/issues)
- Contact: [your contact method]
- Documentation: [link to docs]

---

Built with dedication for Kenyan landlords