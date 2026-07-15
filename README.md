# BankingSystem

A console-based banking system built in C#. It simulates core banking operations — account management, deposits, withdrawals, transfers, loans, cheque book requests, and admin oversight — all in a single-file, menu-driven application.

## Features

### Customer
- View account info (balance, loan, contact details, frozen status)
- Deposit funds (standard cash or external check/transfer)
- Withdraw funds (including ATM withdrawals with a flat fee)
- Transfer funds to other accounts
- View a full transaction statement / ledger
- Manage a list of saved beneficiaries
- Repay outstanding loans
- Update profile (phone number, address)
- Pay utility bills (electricity, water, internet)
- Check a simple, transaction-based credit score
- Convert account balance to USD, EUR, and AED
- Request a cheque book (25 or 50 leaves)

### Admin
- Create new accounts (Savings, Checking, or Fixed Deposit)
- Run a full audit report of all accounts (total deposits and loans)
- Issue loans (standard or with an upfront processing fee)
- Force-mature a Fixed Deposit account so it can be withdrawn from
- Freeze / unfreeze accounts
- Approve or reject pending cheque book requests
- Filter accounts by a minimum balance ("high wealth" profiles)
- View all customers with outstanding loans
- Delete an account (only if it has a zero balance and no outstanding loan)

### Account Types
| Type | Notes |
|---|---|
| **Savings** | Standard withdrawals; ATM withdrawals incur a fixed fee |
| **Checking** | Allows overdrafts up to a fixed negative limit |
| **Fixed Deposit** | Withdrawals are locked until the deposit is marked as matured |

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or later recommended)

### Running the project
```bash
git clone https://github.com/<your-username>/<repo-name>.git
cd <repo-name>
dotnet run
```

On startup, the app seeds two sample accounts:

| Account Number | Holder | PIN | Type | Initial Balance |
|---|---|---|---|---|
| `ACC1` | Saqib | `1111` | Savings | 50,000 PKR |
| `ACC2` | Abdullah | `2222` | Checking | 150,000 PKR |

### Logging In
From the main menu you can choose:
- **Customer Login** — enter an account number and PIN (3 attempts allowed)
- **Administrator Login** — use ID `admin` and PIN `1234`

## Project Structure
Everything currently lives in `Program.cs`:
- `BankAccount` (abstract) — base class with shared logic; `SavingsAccount`, `CheckingAccount`, and `FixedDepositAccount` inherit from it
- `Transaction` / `ChequeRequest` — simple data models for the ledger and cheque queue
- `CentralBankData` — static in-memory store for all accounts and cheque requests
- `Login` — shared login/menu loop; `CustomerLogin` and `AdminLogin` extend it with role-specific menus

## Notes
- Data is stored in-memory only and resets every time the application restarts — there is no database or file persistence.
- Currency conversion rates and bill amounts are hardcoded for demonstration purposes.
- Admin credentials are hardcoded and intended for demo/testing use only; do not use this as-is in a production environment.

## License
All rights reserved. This code is shared publicly for portfolio/demonstration purposes only. No permission is granted to copy, modify, or redistribute it without explicit consent.
