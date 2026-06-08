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
## Technologies & Libraries

### Required NuGet Packages

- Microsoft.EntityFrameworkCore 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- Microsoft.Data.SqlClient

---

## Data Security Class Work 1 – Authentication Update

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

Four different SQL Injection attacks were demonstrated:

#### Attacks via Email / Username field

1. Authentication Bypass  
Email: `' OR 1=1--`  
Password: anything  
This attack forces the SQL condition to always be true, allowing login without valid credentials (usually as the first user – Admin).

2. Login as Specific Admin (commenting password check)  
Email: `yaellevin1@gmail.com' --`  
Password: anything  
This attack closes the email string and comments out the password check, allowing login as a specific Admin user without knowing the password.

---

#### Attacks via Password field (with SHA256 inside SQL)

In this implementation, the password is hashed using SHA256 inside the SQL query.  
Therefore, the payload must first break out of the hash function before injecting SQL.

3. Authentication Bypass via Password  
Email: anything@gmail.com  
Password: `x'), 2)) OR 1=1--`  
This payload breaks out of the hash function and injects a condition that is always true, bypassing authentication.

4. Login as Specific Admin via Password  
Email: anything@gmail.com  
Password: `x'), 2)) OR (1=1 AND Email='yaellevin1@gmail.com')--`  
This attack injects a condition targeting a specific Admin user, allowing login without knowing the password.


These attacks were demonstrated only in the intentionally vulnerable `VulnerableLogin` endpoint, while the regular login remains secure using proper hashing and parameterized queries.
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

## Data Security – Classwork 3

This part focuses on SQL Injection mitigation and explains why the regular login system remains protected against SQL Injection attacks, while the intentionally vulnerable login is kept only for demonstration purposes.

### Secure Login Protection

Unlike the `VulnerableLogin` endpoint, the regular `Login` method uses secure mechanisms to prevent SQL Injection attacks.

The implemented protections include:

- Parameterized Queries (`@email`)
- Prevention of string concatenation inside SQL queries
- Password verification outside the SQL query itself
- SHA-256 password hashing
- Input validation for email addresses

The secure login retrieves the user using a parameterized SQL query:

```csharp
string sql = "SELECT * FROM Users WHERE Email=@email AND IsActive=1";
cmd.Parameters.AddWithValue("@email", email);
```

Because the user input is passed as a parameter, malicious SQL payloads are treated as plain text and cannot modify the structure of the SQL query.

In addition, the password itself is not included directly in the SQL statement.  
The system first retrieves the user record and then verifies the password hash using:

```csharp
PasswordHelper.VerifyPassword(...)
```

This separation prevents attackers from manipulating the password field in order to alter SQL query logic.

### Additional Validation

The login system also includes email validation and automatic email completion for invalid or incomplete email input.

These mechanisms improve input validation and reduce the chance of malformed user input reaching the system.

### Relation Between the Classworks

- Classwork 1 focused on secure authentication mechanisms, SHA-256 hashing, password recovery, and email verification.
- Classwork 2 demonstrated SQL Injection vulnerabilities through an intentionally insecure login endpoint.
- Classwork 3 explains how the secure login implementation prevents those attacks in practice.

---

## Notes – Git Configuration Issue

During development, an issue was encountered where Git did not recognize the `.gitignore` file due to incorrect file encoding (UTF-16 on Windows).

This was resolved by saving the `.gitignore` file using ASCII encoding and resetting the Git index to reapply ignore rules.

As a result, sensitive files such as `appsettings.json` and build directories (`bin`, `obj`) were successfully excluded from version control.
