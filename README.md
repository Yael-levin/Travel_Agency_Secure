## Data Security Classwork Submission

**Submitted by:**  
Yael Levin  
Noam Maimon

---

## Project Overview

This project is based on a full-stack **Travel Agency Web Application** developed using ASP.NET Core MVC.

The system simulates a real-world booking platform similar to Booking.com, supporting:
- User registration and login
- Role-based access (Admin / User)
- Trip booking and payments
- Waiting list management
- Email notifications and PDF generation

The Data Security classwork extends this existing system by focusing on:
- Secure authentication mechanisms (Classwork 1)
- SQL Injection vulnerabilities and data exposure (Classwork 2)

---

## Data Security Class Work – Authentication Update

This project was adapted to meet the **Data Security** class work requirements for a secure login-based web application.

### Implemented Security Requirements

- **User authentication with email and password**
  - Users log in through a dedicated login page.
  - The system validates credentials against the SQL database.
  - Separate fields are used for email and password in the code.

- **Role-based access control**
  - The system differentiates between **Admin** and **User** roles.
  - The **first registered user** is automatically assigned the **Admin** role.
  - All later users are assigned the **User** role.

- **Password protection with SHA-256**
  - Passwords are **never stored in plain text**.
  - Password hashing is implemented using **SHA-256** through a dedicated helper class.
  - During login, the entered password is hashed again and compared with the stored hash.

- **Forgot Password flow**
  - A **"Forgot Password?"** option is available.
  - The user enters their email address.
  - A verification code is sent by email.
  - After code verification, the user is redirected to create a new password.
  - The new password is hashed with **SHA-256** before being saved in the database.

- **Password recovery policy**
  - The original password cannot be restored or displayed at any stage.
  - Only a password reset process is supported, which complies with secure password handling requirements.

### Relevant Implementation Files

- `Helpers/PasswordHelper.cs`
  - Responsible for SHA-256 password hashing and password verification.

- `Controllers/UsersController.cs`
  - Handles:
    - login validation
    - role assignment for first user/admin
    - forgot password flow
    - reset code verification
    - password reset with SHA-256

- `Services/EmailService.cs`
  - Sends the password reset verification code by email.

### SQL Requirement

The database is implemented in **SQL Server**, as required.  
The project includes an SQL script file for recreating the database schema.

---

## Data Security – Classwork 2

This part extends the system to demonstrate SQL Injection vulnerabilities and exposure of sensitive data.

### Credit Card Storage

Each user now has stored (unencrypted) credit card details in the database:
- FirstName, LastName
- IDNumber
- CreditCardNumber
- ValidDate (MM/YY)
- CVC

Implemented in:
- `Models/User.cs`
- `Controllers/UsersController.cs` → `Register(...)`
- `Views/Users/Register.cshtml`

All fields are validated using Regex and logic checks (including expiration date validation).

---

### Vulnerable Login

A separate vulnerable login page was created:

- `Controllers/UsersController.cs` → `VulnerableLogin(...)`
- `Views/Users/VulnerableLogin.cshtml`

This login builds SQL queries using string concatenation, making it vulnerable to SQL Injection.

---

### SQL Injection Attacks

Two different attacks were demonstrated:

1. Authentication Bypass  
Email: ' OR 1=1 --  
Password: anything  
This attack forces the SQL condition to always be true, allowing login without valid credentials (usually as the first user – Admin).

2. Login as Specific Admin (commenting password check)  
Email: yaellevin1@gmail.com' --  
Password: anything  
This attack closes the email string and comments out the password check, allowing login as a specific Admin user without knowing the password.

---

### Data Exposure

After successful SQL Injection, the attacker gains Admin access and can navigate to:  
/AdminUsers

This page displays all users including:
- Personal details
- Credit card information

---

### Summary

This demonstrates how insecure SQL queries can:
- Bypass authentication
- Escalate privileges to Admin
- Expose sensitive user data

---