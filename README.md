
# Book Rental Service

## Overview
The **Book Rental Service** is a comprehensive API-based project that handles book rental operations, including searching, renting, and returning books, managing waiting lists, overdue notifications, and extending rental due dates. It utilizes `SendGrid` for email notifications and follows a service-based architecture with dependency injection.

## Table of Contents
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
  - [Search Books](#search-books)
  - [Rent a Book](#rent-a-book)
  - [Return a Book](#return-a-book)
  - [Extend Due Date](#extend-due-date)
  - [Add to Waiting List](#add-to-waiting-list)
- [Services](#services)
  - [Book Service](#book-service)
  - [Email Service](#email-service)
  - [Background Job](#background-job)
- [Configuration](#configuration)
  - [SendGrid Setup](#sendgrid-setup)
- [Logging](#logging)
- [Database Schema](#database-schema)
- [How to Run](#how-to-run)

## Project Structure

The project is divided into the following core components:

1. **Controllers**: 
   - `BooksController.cs` handles all API requests related to book operations.
   
2. **Services**: 
   - `BookService.cs` contains the business logic for book rentals and management.
   - `EmailService.cs` manages email notifications using SendGrid.
   - `EmailBackgroundService.cs` is a background service that periodically sends overdue notifications.

3. **Repository**:
   - Contains repository interfaces and implementations for accessing and managing data.

4. **Models**:
   - Defines the data models like `Book`, `Rental`, `WaitingList`, etc.

## API Endpoints

### Search Books
**Endpoint**: `GET /api/books/search`  
**Description**: Allows searching for books by title and/or genre.  
**Parameters**:
  - `title` (optional): Title of the book.
  - `genre` (optional): Genre of the book.

**Example**:  
```bash
GET /api/books/search?title=Harry&genre=Fantasy
```

### Rent a Book
**Endpoint**: `POST /api/books/rent`  
**Description**: Allows a user to rent a book. If the book is not available, the user is added to the waiting list.  
**Request Body**:
```json
{
  "userId": 1,
  "bookId": 101
}
```

**Example**:  
```bash
POST /api/books/rent
Content-Type: application/json
{
  "userId": 1,
  "bookId": 101
}
```

### Return a Book
**Endpoint**: `POST /api/books/return/{rentalId}`  
**Description**: Allows a user to return a rented book and notifies the next user in line if there's a waiting list.  
**Path Parameter**: `rentalId` - The ID of the rental to be returned.

**Example**:  
```bash
POST /api/books/return/5
```

### Extend Due Date
**Endpoint**: `POST /api/books/extend-due-date/{rentalId}`  
**Description**: Allows extending the due date of a rented book by a specified number of days.  
**Path Parameter**: `rentalId` - The ID of the rental to be extended.  
**Query Parameter**: `days` - Number of days to extend.

**Example**:  
```bash
POST /api/books/extend-due-date/5?days=7
```

### Add to Waiting List
**Endpoint**: `POST /api/books/waiting-list`  
**Description**: Adds a user to the waiting list for a specified book.  
**Request Body**:
```json
{
  "userId": 1,
  "bookId": 101
}
```

**Example**:  
```bash
POST /api/books/waiting-list
Content-Type: application/json
{
  "userId": 1,
  "bookId": 101
}
```

## Services

### Book Service
The `BookService` manages the core business logic for book-related operations:
- **SearchBooksAsync**: Searches for books based on title and genre.
- **RentBookAsync**: Handles the renting of a book, marking it as rented and adding users to the waiting list if necessary.
- **ReturnBookAsync**: Manages the return of a book and checks if users are waiting for that book.
- **ExtendDueDateAsync**: Extends the due date for a rental, with a maximum extension limit.
- **AddToWaitingListAsync**: Adds users to the waiting list for unavailable books.
- **NotifyNextInLineAsync**: Sends notifications to the next user in the waiting list when a book becomes available.

### Email Service
The `EmailService` handles sending email notifications using SendGrid:
- **SendOverdueNotificationAsync**: Sends overdue notifications to users with overdue rentals.
- **SendReservationNotificationAsync**: Notifies the next user in the waiting list when a book becomes available.
- **SendOverdueNotificationsAsync**: Fetches all overdue rentals and sends notifications.

### Background Job
The `EmailBackgroundService` is a background task that runs periodically (every hour) to send overdue notifications:
- **ExecuteAsync**: Runs in an infinite loop, checking for overdue rentals and sending notifications.

## Configuration

### SendGrid Setup
To enable email functionality, configure SendGrid with your API key in the `appsettings.json` file or environment variables:
```json
{
  "SendGridOptions": {
    "ApiKey": "YOUR_SENDGRID_API_KEY"
  }
}
```

## Logging
The project employs extensive logging using `ILogger`:
- Logs all API endpoint invocations.
- Logs information, warnings, and errors for book rental operations.
- Tracks email notifications and background job activities.

Logs help monitor system behavior, identify errors, and track user actions for better debugging and analysis.

## Database Schema

Below is the SQL script to create the database schema for the **Book Rental Service**:

```sql
-- Database Schema for Book Rental Service

-- Users Table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE
);

-- Books Table
CREATE TABLE Books (
    BookId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Genre NVARCHAR(100),
    Author NVARCHAR(100),
    IsRented BIT DEFAULT 0
);

-- Rentals Table
CREATE TABLE Rentals (
    RentalId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    RentedOn DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    ReturnedOn DATETIME NULL,
    ExtensionCount INT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (BookId) REFERENCES Books(BookId)
);

-- Waiting List Table
CREATE TABLE WaitingLists (
    WaitingListId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    RequestedOn DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (BookId) REFERENCES Books(BookId)
);



INSERT INTO Books (Title, Author, ISBN, Genre, IsRented)
VALUES 
('The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', 'Classics', 0),
('To Kill a Mockingbird', 'Harper Lee', '9780060935467', 'Classics', 0),
('1984', 'George Orwell', '9780451524935', 'Dystopian', 0),
('Pride and Prejudice', 'Jane Austen', '9780141199078', 'Romance', 0),
('The Catcher in the Rye', 'J.D. Salinger', '9780316769488', 'Classics', 0),
('The Hobbit', 'J.R.R. Tolkien', '9780547928227', 'Fantasy', 0),
('Fahrenheit 451', 'Ray Bradbury', '9781451673319', 'Science Fiction', 0),
('The Book Thief', 'Markus Zusak', '9780375842207', 'Historical Fiction', 0),
('Moby-Dick', 'Herman Melville', '9781503280786', 'Classics', 0),
('War and Peace', 'Leo Tolstoy', '9781400079988', 'Historical Fiction', 0);



INSERT INTO Users (Name, Email)
VALUES
('John Doe', 'john.doe@example.com'),
('Jane Smith', 'jane.smith@example.com'),
('Alice Johnson', 'alice.johnson@example.com'),
('Bob Brown', 'bob.brown@example.com'),
('Charlie Davis', 'charlie.davis@example.com'),
('Emily Wilson', 'emily.wilson@example.com'),
('Frank Miller', 'frank.miller@example.com'),
('Grace Lee', 'grace.lee@example.com'),
('Henry Clark', 'henry.clark@example.com'),
('Isabella Lewis', 'isabella.lewis@example.com');




```

## How to Run
1. Clone the repository.
2. Set up the database and run the schema script provided above.
3. Update the connection strings in `appsettings.json`.
4. Configure the SendGrid API key.
5. Run the project using the command:
   ```bash
   dotnet run
   ```
6. Access the API via Swagger UI or any REST client.
