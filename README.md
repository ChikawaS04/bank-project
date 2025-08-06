# 🏦 Bank Client Management System

An internal web application built with **ASP.NET Core** and **SQLite** to manage clients, accounts, and financial transactions within a simulated banking environment.

## 🚀 Project Summary

This system simulates an internal banking tool used by employees to manage:

- Client records and linked bank accounts
- Deposits, withdrawals, and internal transfers
- Transaction histories with timestamps and type classification
- Account status tracking (Active, Inactive, Closed)

## 🚀 Live Demo
Hosted on Render: [bank-project-k1ct.onrender.com](https://bank-project-k1ct.onrender.com)

## ⚙️ Technologies Used

- **ASP.NET Core 7.0**
- **SQLite Database**
- **Razor Pages (MVVM Pattern)**
- **Entity Framework Core**
- **Bootstrap 5** for styling
- **C#**

## 💡 Key Features

- 👤 **Client Management**: Add, edit, and view clients
- 💳 **Account Management**:
  - Create, edit, and deactivate accounts
  - Active accounts: allow all forms of transactions
  - Inactive accounts: allow deposits only
  - Frozen accounts: all forms of transactions are blocked but can be reverted back to active
  - Closed accounts: view-only access, cannot be reverted back to active
- 💸 **Transaction System**:
  - Deposit: adds funds
  - Withdraw/Transfer: subtracts funds
  - Enum-based transaction type selector
- ⚠️ **Validation & Popups**:
  - Input validation with real-time error handling
  - Confirmation popups for create/edit/delete actions
- 📄 **Pagination** for Index & Details views

## 🗃️ Database Structure

- **Client** ↔ **Account** (1-to-many)
- **Account** ↔ **Transactions** (1-to-many)
- Enum for `TransactionType`:
  - `Deposit`
  - `Withdraw`
  - `Transfer`
