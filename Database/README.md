# Flexischools Database Scripts

This directory contains SQL Server database scripts for the Flexischools application.

## Files

- `CreateFlexischoolsDatabase.sql` - Complete database creation script

## Database Schema

The database includes the following tables:

### Core Tables
- **Parents** - Parent information with email, name, and wallet balance
- **Students** - Student information linked to parents
- **Canteens** - School canteen information with opening days and cut-off times
- **MenuItems** - Food items offered by canteens with pricing and allergen information
- **Orders** - Orders placed by parents for students
- **OrderItems** - Individual items within orders



## Usage

1. **Create Database**: Run `CreateFlexischoolsDatabase.sql` in SQL Server Management Studio or Azure Data Studio
2. **Verify**: Check that all tables, and sample data are created correctly
3. **Connect**: Use the connection string in your application configuration

## Sample Data

The script includes sample data:
- 3 Parents with different wallet balances
- 3 Canteens (Primary, High School, Sports)
- 4 Students linked to parents
- 7 Menu items across different canteens
- 3 Sample orders with various statuses

## Order Status Values
- 1 = Placed
- 2 = Fulfilled  
- 3 = Cancelled



## Stored Procedures
- `sp_GetOrdersByParent` - Retrieve all orders for a specific parent
- `sp_UpdateOrderStatus` - Update order status with timestamp

## Functions
- `fn_IsCanteenOpenOnDay` - Check if a canteen is open on a specific day

## Security
The script includes commented sections for creating application users and granting appropriate permissions. Uncomment and modify as needed for your environment.
