# Hubtel Wallet API

## Description

This project provides an API service for managing user wallets on the Hubtel app. The API is built with **ASP.NET 8** and uses **SQL Server Management Studio (SSMS)** for data storage. The service includes endpoints for adding and removing wallets, with business rules in place to prevent duplicate wallets, limit the number of wallets per user, and store only the first six digits of the card number for security.

The project also integrates **Token-based encryption** for secure transactions and includes **Swagger** for easy API documentation and testing.

## Features

- **POST Endpoint** to add a wallet:
  - Prevents duplicate wallet additions.
  - A single user can only have up to 5 wallets.
  - Stores only the first six digits of the card number.
  
- **DELETE Endpoint** to remove a wallet.

- **Security**: Token-based encryption to ensure secure access to the endpoints.

- **API Documentation**: Swagger is integrated for easy access to API details and testing.

## Technologies Used

- **ASP.NET Core 8**
- **XUnit For Testing**
- **SQL Server Management Studio (SSMS)**
- **JWT Token-based Authentication**
- **Swagger for API Documentation**
- **Entity Framework for Data Access**

## Setup and Installation

### Prerequisites

Before setting up this project, ensure you have the following installed:

- **.NET 8 SDK**
- **SQL Server Management Studio (SSMS)**
- **Visual Studio (or your preferred IDE for .NET development)**
- **Postman** (optional, for testing the API endpoints)

### Clone the Repository


## PROJECT STRUCTURE IMAGE
![Screenshot 2025-01-26 130443](https://github.com/user-attachments/assets/7a488030-02fa-4798-8176-fe1a0f1cf681)
![Screenshot 2025-01-26 130500](https://github.com/user-attachments/assets/c01177a6-9033-4d5a-adaa-a708f3887bc2)
![Screenshot 2025-01-26 130510](https://github.com/user-attachments/assets/f5e97baa-e5ca-4227-9843-c9d9e93d7e9e)



### DATABASE SCHEMA!
![image](https://github.com/user-attachments/assets/249021a0-a61c-4cd1-8ad0-c25cb51e8439)

### XUNIT TEST
![Screenshot 2025-01-26 130209](https://github.com/user-attachments/assets/378d16d3-a4ca-49fe-8ccf-a7c6cff912bb)


### LIVE SWAGGER IMAGES
![Screenshot 2025-01-26 124700](https://github.com/user-attachments/assets/575838a5-ed6f-4e0d-abfa-2256f25347db)
![Screenshot 2025-01-26 124715](https://github.com/user-attachments/assets/efad3cb3-8178-43d7-8173-f6d970af4357)
![Screenshot 2025-01-26 124734](https://github.com/user-attachments/assets/5514303c-d821-44f1-9baa-6dd46b5e3490)





