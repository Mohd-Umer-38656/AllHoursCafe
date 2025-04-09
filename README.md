# All Hours Cafe

## Project Overview
All Hours Cafe is a full-featured restaurant website built with ASP.NET Core MVC. The application provides a complete online presence for a cafe, allowing customers to browse the menu, place orders, make reservations, and contact the restaurant. It also includes a comprehensive admin dashboard for managing menu items, categories, reservations, and user roles.

## Features

### Customer Features
- **Menu Browsing**: View all menu items categorized by food type
- **Shopping Cart**: Add items to cart, adjust quantities, and proceed to checkout
- **User Authentication**: Register and login to place orders and make reservations
- **Reservations**: Book a table with date, time, and party size
- **Contact Form**: Send inquiries directly to the restaurant
- **Responsive Design**: Optimized for all device sizes

### Admin Features
- **Dashboard**: Overview of orders, reservations, and user activity
- **Menu Management**: CRUD operations for menu items and categories
- **User Management**: View user details and manage roles
- **Reservation Management**: View and manage table reservations
- **Contact Form Submissions**: View and respond to customer inquiries
- **Role-based Authorization**: Secure access to admin features

## Project Structure
- **Models**: Database entities and view models
- **Views**: Razor views for rendering UI
- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic and data processing
- **wwwroot**: Static files (CSS, JavaScript, images)
- **Data**: Database context and migrations

## Technologies Used
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- HTML5, CSS3, JavaScript
- Bootstrap
- jQuery
- Font Awesome

## Setup and Installation

### Prerequisites
- .NET 6.0 SDK or later
- SQL Server (LocalDB or full version)
- Visual Studio 2022 or Visual Studio Code

### Steps to Run
1. Clone the repository
2. Open the solution in Visual Studio
3. Update the connection string in `appsettings.json` if needed
4. Run the following commands in Package Manager Console:
   ```
   Update-Database
   ```
5. Run the application (F5 or Ctrl+F5)

## Admin Access
To access the admin dashboard, use the following credentials:

- **Email**: admin@allhourscafe.com
- **Password**: Admin@123

## Key Pages

### Customer Pages
- **Home**: Landing page with featured menu items and cafe information
- **Menu**: Browse all menu items with filtering by category
- **Reservations**: Book a table at the cafe
- **Contact**: Send inquiries to the cafe
- **Cart & Checkout**: Review cart and complete orders

### Admin Pages
- **Dashboard**: Overview of site activity
- **Menu Management**: Add, edit, and delete menu items and categories
- **User Management**: View user details and manage roles
- **Reservations**: View and manage table reservations
- **Contact Submissions**: View customer inquiries

## Screenshots

### Customer Interface
- **Home Page**: Welcoming landing page with cafe ambiance and featured items
- **Menu Page**: Responsive menu with categories and item details
- **Cart**: User-friendly cart with item management
- **Reservation Form**: Easy-to-use booking system

### Admin Interface
- **Dashboard**: Comprehensive overview with key metrics
- **Menu Management**: Intuitive interface for managing menu items
- **User Management**: Secure role-based access control
- **Reservation Management**: Efficient reservation handling system

## Development Notes
- The project follows MVC architecture pattern
- Implements repository pattern for data access
- Uses dependency injection for loose coupling
- Includes client-side validation with jQuery
- Features responsive design with mobile-first approach

## Future Enhancements
- Online payment integration
- Customer loyalty program
- Order tracking system
- Staff scheduling module
- Analytics dashboard

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgements
- Images from Unsplash
- Icons from Font Awesome
- Developed by All Hours Cafe Team
