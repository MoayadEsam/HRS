# Hotel Reservation System (HRS)

## Short Description
A comprehensive hotel management system built with ASP.NET Core MVC that handles reservations, staff management, financial tracking, and inventory control with full Turkish localization support.

## Long Description
The Hotel Reservation System is a full-stack web application designed to streamline hotel operations and enhance guest experiences. The system provides role-based access for administrators, staff, and customers, each with tailored dashboards and functionalities.

**Key Features:**
- **Reservation Management**: Complete booking workflow with room availability checking, multi-criteria search, and reservation tracking
- **Staff Management**: Employee records, department organization, attendance tracking, and salary processing
- **Financial Operations**: Income and expense tracking, budget management, and comprehensive financial reporting with charts
- **Inventory Control**: Stock management with categories, transactions, and reorder level monitoring
- **Analytics Dashboard**: Real-time metrics, occupancy rates, revenue tracking, and performance indicators
- **Bilingual Support**: Full Turkish (tr-TR) and English localization with 385+ translated strings
- **Auto-provisioning**: Automatic staff account creation with default credentials for seamless onboarding

The system emphasizes user experience with a modern, responsive design and implements enterprise-grade security with ASP.NET Core Identity for authentication and role-based authorization.

## Technical Overview

**Architecture:**
- Clean Architecture pattern with separation of concerns (Core, Data, Services, Web layers)
- Repository and Unit of Work patterns for data access
- AutoMapper for object-to-object mapping
- Dependency Injection throughout the application

**Tech Stack:**
- **Backend**: ASP.NET Core 9.0 MVC, C#
- **Database**: PostgreSQL (Supabase hosted)
- **ORM**: Entity Framework Core 9.0 with Code-First migrations
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Frontend**: Razor Views, Bootstrap 5, Chart.js for data visualization
- **Localization**: Resource files (.resx) for multi-language support

**Database Design:**
- 15+ entities with proper relationships and constraints
- Optimized indexes for performance on frequently queried columns
- PostgreSQL-specific features (date functions compatibility)

**Key Implementation Details:**
- Async/await patterns for all database operations
- Performance optimizations: pagination, AsNoTracking queries, connection pooling
- Data seeding for development/testing with realistic Turkish sample data
- Input validation with Data Annotations and server-side checks
- Export functionality (CSV, PDF) for reports

**Security Features:**
- Password requirements enforcement (uppercase, lowercase, digit, special character, 6+ chars)
- Anti-forgery tokens for form submissions
- Email confirmation for user accounts
- Role-based access control (Admin, Staff, Customer)

**Deployment:**
- Configuration for Railway and Supabase deployment
- Environment-specific settings (Development/Production)
- Docker-ready setup available
