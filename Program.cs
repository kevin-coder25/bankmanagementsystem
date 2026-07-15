using System;

namespace BankingSystem
{
    public class Transaction
    {
        public string Description { get; set; }
        public double Amount { get; set; } 
        public double ResultingBalance { get; set; }

        public Transaction(string desc, double amt, double bal)
        {
            Description = desc;
            Amount = amt;
            ResultingBalance = bal;
        }
    }

    public class ChequeRequest
    {
        public string RequestId { get; set; }
        public string AccountNumber { get; set; }
        public int LeavesCount { get; set; }
        public string Status { get; set; }

        public ChequeRequest(string id, string accNum, int leaves)
        {
            RequestId = id;
            AccountNumber = accNum;
            LeavesCount = leaves;
            Status = "Pending";
        }
    }

    public abstract class BankAccount
    {
        private string _accountNumber;
        private string _accountHolderName;
        private string _pinCode;
        private string _phoneNumber;
        private string _addressLine;
        private bool _isFrozen;
        protected double _balance;
        protected double _loanBalance;

        private Transaction[] _ledger;
        private int _transactionCount;

        private string[] _beneficiaries;
        private int _beneficiaryCount;

        public string AccountNumber { get { return _accountNumber; } }
        public string AccountHolderName { get { return _accountHolderName; } }
        public string PhoneNumber { get { return _phoneNumber; } set { _phoneNumber = value ?? "Not Specified"; } }
        public string AddressLine { get { return _addressLine; } set { _addressLine = value ?? "Not Specified"; } }
        public bool IsFrozen { get { return _isFrozen; } set { _isFrozen = value; } }
        public double Balance { get { return _balance; } }
        public double LoanBalance { get { return _loanBalance; } }
        public Transaction[] Ledger { get { return _ledger; } }
        public int TransactionCount { get { return _transactionCount; } }
        public string[] Beneficiaries { get { return _beneficiaries; } }
        public int BeneficiaryCount { get { return _beneficiaryCount; } }
        public abstract string AccountType { get; }

        public BankAccount(string accNum, string name, string pin, double initialDeposit)
        {
            _accountNumber = accNum ?? throw new ArgumentNullException(nameof(accNum));
            _accountHolderName = name ?? "Unknown";
            _pinCode = pin ?? "0000";
            _balance = initialDeposit;
            _loanBalance = 0.0;
            _phoneNumber = "Not Specified";
            _addressLine = "Not Specified";
            _isFrozen = false;

            _ledger = new Transaction[4];
            _transactionCount = 0;

            _beneficiaries = new string[4];
            _beneficiaryCount = 0;

            RecordTransaction("Account Opened", initialDeposit);
        }

        public bool VerifyPin(string inputPin)
        {
            return !string.IsNullOrEmpty(inputPin) && _pinCode == inputPin;
        }

        public void UpdatePin(string newPin)
        {
            if (!string.IsNullOrWhiteSpace(newPin))
            {
                _pinCode = newPin;
            }
        }

        public void RecordTransaction(string desc, double amt)
        {
            if (_transactionCount >= _ledger.Length)
            {
                Transaction[] temp = new Transaction[_ledger.Length * 2];
                for (int i = 0; i < _ledger.Length; i++)
                {
                    temp[i] = _ledger[i];
                }
                _ledger = temp;
            }
            _ledger[_transactionCount] = new Transaction(desc, amt, _balance);
            _transactionCount++;
        }

        public void AddBeneficiary(string accNum)
        {
            if (string.IsNullOrWhiteSpace(accNum))
            {
                return;
            }
            if (_beneficiaryCount >= _beneficiaries.Length)
            {
                string[] temp = new string[_beneficiaries.Length * 2];
                for (int i = 0; i < _beneficiaries.Length; i++)
                {
                    temp[i] = _beneficiaries[i];
                }
                _beneficiaries = temp;
            }
            _beneficiaries[_beneficiaryCount] = accNum;
            _beneficiaryCount++;
        }

        public void ClearBeneficiaries()
        {
            _beneficiaries = new string[4];
            _beneficiaryCount = 0;
        }

        public bool ContainsBeneficiary(string accNum)
        {
            if (string.IsNullOrWhiteSpace(accNum))
            {
                return false;
            }
            for (int i = 0; i < _beneficiaryCount; i++)
            {
                if (_beneficiaries[i] == accNum) return true;
            }
            return false;
        }

        public virtual void Deposit(double amount)
        {
            if (_isFrozen)
            {
                Console.WriteLine("Account is frozen.");
                return;
            }
            if (amount <= 0)
            {
                Console.WriteLine("Invalid deposit amount.");
                return;
            }
            _balance += amount;
            RecordTransaction("Deposit", amount);
            Console.WriteLine("Deposited: " + amount + " PKR");
        }

        public virtual void Deposit(double amount, string source)
        {
            if (_isFrozen)
            {
                Console.WriteLine("Account is frozen.");
                return;
            }
            if (amount <= 0)
            {
                Console.WriteLine("Invalid deposit amount.");
                return;
            }
            _balance += amount;
            RecordTransaction("Deposit via " + (source ?? "Unknown Source"), amount);
            Console.WriteLine("Deposited: " + amount + " PKR via " + source);
        }

        public abstract void Withdraw(double amount);
        public abstract void Withdraw(double amount, bool isATM);

        public void AddLoan(double loanAmount)
        {
            if (_isFrozen)
            {
                return;
            }
            _loanBalance += loanAmount;
            _balance += loanAmount;
            RecordTransaction("Loan Disbursed", loanAmount);
        }

        public void AddLoan(double loanAmount, double processingFee)
        {
            if (_isFrozen)
            {
                return;
            }
            _loanBalance += (loanAmount + processingFee);
            _balance += loanAmount;
            RecordTransaction("Loan Disbursed", loanAmount);
            RecordTransaction("Processing Fee Charged", -processingFee); 
        }

        public void RepayLoan(double paymentAmount)
        {
            if (_isFrozen)
            {
                return;
            }
            if (paymentAmount <= 0)
            {
                return;
            }
            if (_balance < paymentAmount)
            {
                Console.WriteLine("Insufficient balance for repayment.");
                return;
            }
            if (paymentAmount > _loanBalance)
            {
                Console.WriteLine("Payment exceeds outstanding loan.");
                return;
            }

            _balance -= paymentAmount;
            _loanBalance -= paymentAmount;
            RecordTransaction("Loan Repayment", -paymentAmount);
            Console.WriteLine("Paid loan amount: " + paymentAmount + " PKR");
        }
    }

    public class SavingsAccount : BankAccount
    {
        private const double AtmFee = 50.0; 

        public override string AccountType { get { return "Savings"; } }
        public SavingsAccount(string accNum, string name, string pin, double initialDeposit) 
            : base(accNum, name, pin, initialDeposit) { }

        public override void Withdraw(double amount)
        {
            if (IsFrozen)
            {
                Console.WriteLine("Account is frozen.");
                return;
            }
            if (amount <= 0 || _balance < amount)
            {
                Console.WriteLine("Insufficient funds.");
                return;
            }
            _balance -= amount;
            RecordTransaction("Withdrawal", -amount);
            Console.WriteLine("Withdrew: " + amount + " PKR");
        }

        public override void Withdraw(double amount, bool isATM)
        {
            double fee = isATM ? AtmFee : 0.0;
            if (IsFrozen)
            {
                return;
            }
            if (amount <= 0 || _balance < (amount + fee))
            {
                Console.WriteLine("Insufficient funds including ATM fee.");
                return;
            }
            _balance -= (amount + fee);
            RecordTransaction(isATM ? "ATM Withdrawal" : "Withdrawal", -amount);
            if (isATM)
            {
                RecordTransaction("ATM Transaction Fee", -fee);
            }
            Console.WriteLine("Withdrew: " + amount + " PKR" + (isATM ? $" (Fee: {AtmFee} PKR)" : ""));
        }
    }

    public class CheckingAccount : BankAccount
    {
        private const double OverdraftLimit = -50000.0; 

        public override string AccountType { get { return "Checking"; } }
        public CheckingAccount(string accNum, string name, string pin, double initialDeposit) 
            : base(accNum, name, pin, initialDeposit) { }

        public override void Withdraw(double amount)
        {
            if (IsFrozen)
            {
                Console.WriteLine("Account is frozen.");
                return;
            }
            if (amount <= 0 || _balance - amount < OverdraftLimit)
            {
                Console.WriteLine($"Rejected: Exceeds overdraft limit ({Math.Abs(OverdraftLimit)} PKR).");
                return;
            }
            _balance -= amount;
            RecordTransaction("Withdrawal", -amount); 
            Console.WriteLine("Withdrew: " + amount + " PKR");
        }

        public override void Withdraw(double amount, bool isATM)
        {
            
            Withdraw(amount);
        }
    }

    public class FixedDepositAccount : BankAccount
    {
        private bool _isMatured;
        public override string AccountType { get { return "Fixed Deposit"; } }
        public bool IsMatured { get { return _isMatured; } set { _isMatured = value; } }

        public FixedDepositAccount(string accNum, string name, string pin, double initialDeposit)
            : base(accNum, name, pin, initialDeposit)
        {
            _isMatured = false;
        }

        public override void Withdraw(double amount)
        {
            if (IsFrozen)
            {
                return;
            }
            if (!_isMatured)
            {
                Console.WriteLine("Rejected: Fixed deposit is still locked.");
                return;
            }
            if (amount <= 0 || _balance < amount)
            {
                Console.WriteLine("Insufficient funds.");
                return;
            }

            _balance -= amount;
            RecordTransaction("Withdrawal", -amount); 
            Console.WriteLine("Withdrew: " + amount + " PKR");
        }

        public override void Withdraw(double amount, bool isATM)
        {
            
            Withdraw(amount);
        }
    }

    public class CentralBankData
    {
        public static BankAccount[] Database = new BankAccount[4];
        public static int AccountCount = 0;

        public static ChequeRequest[] ChequeRegistry = new ChequeRequest[4];
        public static int ChequeCount = 0;

        public static int ChequeCounter = 5001;

        public static void AddAccount(BankAccount acc)
        {
            if (acc == null) return;
            if (AccountCount >= Database.Length)
            {
                BankAccount[] temp = new BankAccount[Database.Length * 2];
                for (int i = 0; i < Database.Length; i++)
                {
                    temp[i] = Database[i];
                }
                Database = temp;
            }
            Database[AccountCount] = acc;
            AccountCount++;
        }

        public static void RemoveAccount(BankAccount acc)
        {
            if (acc == null) return;
            int index = -1;
            for (int i = 0; i < AccountCount; i++)
            {
                if (Database[i].AccountNumber == acc.AccountNumber)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                for (int i = index; i < AccountCount - 1; i++)
                {
                    Database[i] = Database[i + 1];
                }
                Database[AccountCount - 1] = null;
                AccountCount--;
            }
        }

        public static void AddChequeRequest(ChequeRequest req)
        {
            if (req == null) return;
            if (ChequeCount >= ChequeRegistry.Length)
            {
                ChequeRequest[] temp = new ChequeRequest[ChequeRegistry.Length * 2];
                for (int i = 0; i < ChequeRegistry.Length; i++)
                {
                    temp[i] = ChequeRegistry[i];
                }
                ChequeRegistry = temp;
            }
            ChequeRegistry[ChequeCount] = req;
            ChequeCount++;
        }

        public static BankAccount Find(string accNum)
        {
            if (string.IsNullOrWhiteSpace(accNum)) return null;
            for (int i = 0; i < AccountCount; i++)
            {
                if (Database[i].AccountNumber == accNum) return Database[i];
            }
            return null;
        }
    }

    public class Login
    {
        public static bool CustomerStart;
        public static bool AdminStart;
        public static bool running = true;
        protected static BankAccount currentActiveAccount = null;

        public virtual void LogOut() { }

        public void CheckingLogin()
        {
            while (running)
            {
                Console.WriteLine("\n=====================================");
                Console.WriteLine("             BANK SYSTEM             ");
                Console.WriteLine("=====================================");
                Console.WriteLine(" 1. Customer Login");
                Console.WriteLine(" 2. Administrator Login");
                Console.WriteLine(" 3. Exit Application");
                Console.WriteLine("=====================================");
                Console.Write("Selection: ");
                
                string inputStr = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(inputStr) || !int.TryParse(inputStr, out int selection))
                {
                    Console.WriteLine("Invalid entry. Use numbers.");
                    continue;
                }

                if (selection == 1)
                {
                    Console.Write(" Account Number: ");
                    string _accountNumber = Console.ReadLine();

                    BankAccount targetAccount = CentralBankData.Find(_accountNumber);

                    if (targetAccount != null)
                    {
                        if (targetAccount.IsFrozen)
                        {
                            Console.WriteLine("Account is frozen.");
                            continue;
                        }

                        bool wrongpin = false;
                        int attempts = 0;
                        do
                        {
                            Console.Write(" Enter PIN: ");
                            string _pinNumber = Console.ReadLine();

                            if (targetAccount.VerifyPin(_pinNumber))
                            {
                                wrongpin = false;
                                currentActiveAccount = targetAccount;
                                CustomerStart = true;
                                
                                CustomerLogin clientView = new CustomerLogin();
                                clientView.Start();
                            }
                            else
                            {
                                attempts++;
                                Console.WriteLine("Wrong PIN. Remaining: " + (3 - attempts));
                                if (attempts >= 3)
                                {
                                    Console.WriteLine("Max attempts reached.");
                                    wrongpin = false;
                                }
                                else
                                {
                                    wrongpin = true;
                                }
                            }
                        } while (wrongpin);
                    }
                    else
                    {
                        Console.WriteLine("Account not found.");
                    }
                }
                else if (selection == 2)
                {
                    Console.Write(" Admin ID: ");
                    string adminId = Console.ReadLine();

                    if (adminId == "admin")
                    {
                        bool wrongpin = false;
                        do
                        {
                            Console.Write(" Enter Admin PIN: ");
                            string pinNumber = Console.ReadLine();
                            string adminPin = "1234";
                            if (pinNumber == adminPin)
                            {
                                wrongpin = false;
                                AdminStart = true;

                                AdminLogin adminView = new AdminLogin();
                                adminView.Start();
                            }
                            else
                            {
                                Console.WriteLine("Wrong PIN.");
                                wrongpin = true;
                            }
                        } while (wrongpin);
                    }
                    else
                    {
                        Console.WriteLine("Wrong ID.");
                    }
                }
                else if (selection == 3)
                {
                    running = false;
                }
                else
                {
                    Console.WriteLine("Invalid option.");
                }
            }
        }
    }

    public class CustomerLogin : Login
    {
        
        private const double ElectricityBillCost = 12000.00;
        private const double WaterBillCost = 1500.50;
        private const double InternetBillCost = 3500.90;

        private const double ConversionRateUsd = 278.5;
        private const double ConversionRateEur = 302.2;
        private const double ConversionRateAed = 75.8;

        public void Start()
        {
            while (CustomerStart)
            {
                Console.WriteLine("\n=====================================");
                Console.WriteLine("          CUSTOMER ACCOUNT         ");
                Console.WriteLine("=====================================");
                Console.WriteLine(" User: " + currentActiveAccount.AccountHolderName + " [" + currentActiveAccount.AccountNumber + "]");
                Console.WriteLine("=====================================");
                Console.WriteLine(" 1. Account Info");
                Console.WriteLine(" 2. Deposit");
                Console.WriteLine(" 3. Withdraw");
                Console.WriteLine(" 4. Transfer Funds");
                Console.WriteLine(" 5. Transaction Statement");
                Console.WriteLine(" 6. Manage Beneficiaries");
                Console.WriteLine(" 7. Loan Repayment");
                Console.WriteLine(" 8. Profile Maintenance");
                Console.WriteLine(" 9. Pay Utility Bills");
                Console.WriteLine(" 10. Check Credit Score");
                Console.WriteLine(" 11. Currency Converter (from PKR)");
                Console.WriteLine(" 12. Request Cheque Book");
                Console.WriteLine(" 13. Log Out");
                Console.WriteLine("=====================================");
                Console.Write("Selection: ");

                string choice = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(choice)) continue;

                switch (choice)
                {
                    case "1": ShowInfo(); break;
                    case "2": ShowDeposit(); break;
                    case "3": ShowWithdraw(); break;
                    case "4": ExecuteTransfer(); break;
                    case "5": ShowStatement(); break;
                    case "6": ExecuteBeneficiaryMenu(); break;
                    case "7": ExecuteLoanRepayment(); break;
                    case "8": OpenMaintenance(); break;
                    case "9": ExecuteBillPayment(); break;
                    case "10": CheckCredit(); break;
                    case "11": RunCurrency(); break;
                    case "12": RequestCheque(); break;
                    case "13": LogOut(); break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        private void ShowInfo()
        {
            Console.WriteLine("\n--- INFO ---");
            Console.WriteLine("ID: " + currentActiveAccount.AccountNumber);
            Console.WriteLine("Name: " + currentActiveAccount.AccountHolderName);
            Console.WriteLine("Type: " + currentActiveAccount.AccountType);
            Console.WriteLine("Phone: " + currentActiveAccount.PhoneNumber);
            Console.WriteLine("Address: " + currentActiveAccount.AddressLine);
            Console.WriteLine("Balance: " + currentActiveAccount.Balance + " PKR");
            Console.WriteLine("Loan Owed: " + currentActiveAccount.LoanBalance + " PKR");
            Console.WriteLine("Frozen Status: " + currentActiveAccount.IsFrozen);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        private void ShowDeposit()
        {
            Console.WriteLine("\n 1. Standard Cash Deposit");
            Console.WriteLine(" 2. External Check/Transfer Deposit");
            Console.Write("Selection: ");
            string sub = Console.ReadLine();

            Console.Write("Enter amount to deposit: ");
            if (double.TryParse(Console.ReadLine(), out double amt))
            {
                if (sub == "2")
                {
                    Console.Write("Enter Source Name: ");
                    string src = Console.ReadLine();
                    currentActiveAccount.Deposit(amt, src);
                }
                else
                {
                    currentActiveAccount.Deposit(amt);
                }
            }
            else
            {
                Console.WriteLine("Invalid numbers input.");
            }
        }

        private void ShowWithdraw()
        {
            Console.WriteLine("\n 1. Counter Withdrawal");
            Console.WriteLine(" 2. ATM Withdrawal");
            Console.Write("Selection: ");
            string sub = Console.ReadLine();

            Console.Write("Enter amount to withdraw: ");
            if (double.TryParse(Console.ReadLine(), out double amt))
            {
                if (sub == "2")
                {
                    currentActiveAccount.Withdraw(amt, true);
                }
                else
                {
                    currentActiveAccount.Withdraw(amt);
                }
            }
            else
            {
                Console.WriteLine("Invalid numbers input.");
            }
        }

        private void ShowStatement()
        {
            Console.WriteLine("\n--- LEDGER STATEMENT ---");
            if (currentActiveAccount.TransactionCount == 0)
            {
                Console.WriteLine("No transactions yet.");
            }
            else
            {
                for (int i = 0; i < currentActiveAccount.TransactionCount; i++)
                {
                    Transaction entry = currentActiveAccount.Ledger[i];
                    
                    string direction = entry.Amount >= 0 ? "+" : "";
                    Console.WriteLine("[" + (i + 1) + "] " + entry.Description + " | Delta: " + direction + entry.Amount + " PKR | Balance: " + entry.ResultingBalance + " PKR");
                }
            }
            Console.ReadLine();
        }

        private void ExecuteTransfer()
        {
            Console.WriteLine("\n--- TRANSFER ---");
            Console.WriteLine(" 1. Enter Manual Account Number");
            Console.WriteLine(" 2. Select Stored Beneficiary");
            Console.Write("Choice: ");
            string pathway = Console.ReadLine();

            string destNum = "";
            if (pathway == "1")
            {
                Console.Write("Enter Destination Account Number: ");
                destNum = Console.ReadLine();
            }
            else if (pathway == "2")
            {
                if (currentActiveAccount.BeneficiaryCount == 0)
                {
                    Console.WriteLine("Beneficiary list is empty.");
                    return;
                }
                for (int i = 0; i < currentActiveAccount.BeneficiaryCount; i++)
                {
                    Console.WriteLine(" [" + (i + 1) + "] Stored Account: " + currentActiveAccount.Beneficiaries[i]);
                }
                Console.Write("Selection Number: ");
                if (int.TryParse(Console.ReadLine(), out int idx))
                {
                    int adjustedIdx = idx - 1;
                    if (adjustedIdx >= 0 && adjustedIdx < currentActiveAccount.BeneficiaryCount)
                    {
                        destNum = currentActiveAccount.Beneficiaries[adjustedIdx];
                    }
                    else
                    {
                        Console.WriteLine("Invalid Selection index.");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            BankAccount destAcc = CentralBankData.Find(destNum);
            if (destAcc != null)
            {
                if (destAcc.AccountNumber == currentActiveAccount.AccountNumber)
                {
                    Console.WriteLine("Cannot transfer money to yourself.");
                    return;
                }

                Console.Write("Enter amount to transfer: ");
                if (double.TryParse(Console.ReadLine(), out double transferAmt))
                {
                    if (transferAmt <= 0)
                    {
                        return;
                    }
                    double initialBalance = currentActiveAccount.Balance;
                    currentActiveAccount.Withdraw(transferAmt);

                    if (currentActiveAccount.Balance < initialBalance)
                    {
                        destAcc.Deposit(transferAmt);
                        currentActiveAccount.Ledger[currentActiveAccount.TransactionCount - 1].Description = "Transferred Out to " + destAcc.AccountNumber;
                        destAcc.Ledger[destAcc.TransactionCount - 1].Description = "Transferred In from " + currentActiveAccount.AccountNumber;
                        Console.WriteLine("Transfer complete.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Destination account not found.");
            }
        }

        private void ExecuteBeneficiaryMenu()
        {
            bool loop = true;
            while (loop)
            {
                Console.WriteLine("\n--- BENEFICIARY MENU ---");
                Console.WriteLine(" 1. View Saved Accounts");
                Console.WriteLine(" 2. Add New Account");
                Console.WriteLine(" 3. Clear List");
                Console.WriteLine(" 4. Back");
                Console.Write("Choice: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    for (int i = 0; i < currentActiveAccount.BeneficiaryCount; i++)
                    {
                        Console.WriteLine(" -> Stored: " + currentActiveAccount.Beneficiaries[i]);
                    }
                }
                else if (choice == "2")
                {
                    Console.Write("Enter Account Number to add: ");
                    string entry = Console.ReadLine();
                    
                    if (CentralBankData.Find(entry) != null)
                    {
                        if (entry == currentActiveAccount.AccountNumber)
                        {
                            continue;
                        }
                        if (!currentActiveAccount.ContainsBeneficiary(entry))
                        {
                            currentActiveAccount.AddBeneficiary(entry);
                            Console.WriteLine("Added successfully.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Account does not exist.");
                    }
                }
                else if (choice == "3")
                {
                    currentActiveAccount.ClearBeneficiaries();
                    Console.WriteLine("List cleared.");
                }
                else if (choice == "4")
                {
                    loop = false;
                }
            }
        }

        private void ExecuteLoanRepayment()
        {
            Console.WriteLine("\n--- LOAN REPAYMENT ---");
            Console.WriteLine("Owed: " + currentActiveAccount.LoanBalance + " PKR");
            Console.Write("Enter payment amount: ");
            if (double.TryParse(Console.ReadLine(), out double payVal))
            {
                currentActiveAccount.RepayLoan(payVal);
            }
            else
            {
                Console.WriteLine("Invalid financial amount.");
            }
        }

        private void OpenMaintenance()
        {
            bool loop = true;
            while (loop)
            {
                Console.WriteLine("\n--- PROFILE SETUP ---");
                Console.WriteLine(" 1. Edit Phone");
                Console.WriteLine(" 2. Edit Address");
                Console.WriteLine(" 3. Change PIN");
                Console.WriteLine(" 4. Back");
                Console.Write("Choice: ");
                string select = Console.ReadLine();

                if (select == "1")
                {
                    Console.Write("Enter Phone: ");
                    currentActiveAccount.PhoneNumber = Console.ReadLine();
                }
                else if (select == "2")
                {
                    Console.Write("Enter Address: ");
                    currentActiveAccount.AddressLine = Console.ReadLine();
                }
                else if (select == "3")
                {
                    Console.Write("Enter old PIN: ");
                    
                    if (currentActiveAccount.VerifyPin(Console.ReadLine()))
                    {
                        Console.Write("Enter new PIN: ");
                        currentActiveAccount.UpdatePin(Console.ReadLine());
                        Console.WriteLine("PIN updated.");
                    }
                }
                else if (select == "4")
                {
                    loop = false;
                }
            }
        }

        private void ExecuteBillPayment()
        {
            Console.WriteLine("\n--- BILL PAYMENT ---");
            Console.WriteLine($" 1. Electricity Bill ({ElectricityBillCost:F2} PKR)");
            Console.WriteLine($" 2. Water Bill ({WaterBillCost:F2} PKR)");
            Console.WriteLine($" 3. Internet Bill ({InternetBillCost:F2} PKR)");
            Console.Write("Selection: ");
            string billChoice = Console.ReadLine();

            double billCost = 0;
            string billName = "";

            if (billChoice == "1")
            {
                billCost = ElectricityBillCost; billName = "Electricity Bill";
            }
            else if (billChoice == "2")
            {
                billCost = WaterBillCost; billName = "Water Bill";
            }
            else if (billChoice == "3")
            {
                billCost = InternetBillCost; billName = "Internet Bill";
            }
            else
            {
                return;
            }

            if (currentActiveAccount.Balance >= billCost)
            {
                currentActiveAccount.Withdraw(billCost);
                currentActiveAccount.Ledger[currentActiveAccount.TransactionCount - 1].Description = "Paid " + billName;
                Console.WriteLine("Bill paid successfully.");
            }
            else
            {
                Console.WriteLine("Insufficient funds.");
            }
        }

        private void CheckCredit()
        {
            Console.WriteLine("\n--- CREDIT SCORE ---");
            int score = 600;

            if (currentActiveAccount.Balance > 500000)
            {
                score += 120;
            }
            
            if (currentActiveAccount.LoanBalance > 0)
            {
                score -= 150;
            }
            else
            {
                score += 80;
            }
            
            if (currentActiveAccount.TransactionCount > 5)
            {
                score += 50;
            }

            if (score > 850)
            {
                score = 850;
            }
            if (score < 300)
            {
                score = 300;
            }

            Console.WriteLine("Your score is: " + score + " / 850");
            Console.ReadLine();
        }

        private void RunCurrency()
        {
            Console.WriteLine("\n--- CURRENCY MULTIPLIER (PKR CONVERSION) ---");
            Console.WriteLine("USD: " + (currentActiveAccount.Balance / ConversionRateUsd) + " USD");
            Console.WriteLine("EUR: " + (currentActiveAccount.Balance / ConversionRateEur) + " EUR");
            Console.WriteLine("AED: " + (currentActiveAccount.Balance / ConversionRateAed) + " AED");
            Console.ReadLine();
        }

        private void RequestCheque()
        {
            Console.WriteLine("\n--- REQUEST CHEQUE BOOK ---");
            Console.Write("Enter number of leaves required (25 or 50): ");
            
            if (int.TryParse(Console.ReadLine(), out int leaves))
            {
                if (leaves == 25 || leaves == 50)
                {
                    string id = "REQ" + CentralBankData.ChequeCounter;
                    CentralBankData.ChequeCounter++;

                    ChequeRequest req = new ChequeRequest(id, currentActiveAccount.AccountNumber, leaves);
                    CentralBankData.AddChequeRequest(req);
                    Console.WriteLine("Request logged under tracking ID: " + id);
                }
                else
                {
                    Console.WriteLine("Invalid selection. Only books of 25 or 50 leaves allowed.");
                }
            }
            else
            {
                Console.WriteLine("Invalid entry data.");
            }
        }

        public override void LogOut()
        {
            CustomerStart = false;
            currentActiveAccount = null;
            Console.WriteLine("Logged out safely.");
        }
    }

    public class AdminLogin : Login
    {
        public void Start()
        {
            while (AdminStart)
            {
                Console.WriteLine("\n=====================================");
                Console.WriteLine("           ADMINISTRATOR            ");
                Console.WriteLine("=====================================");
                Console.WriteLine(" 1. Create Account");
                Console.WriteLine(" 2. Audit All Records");
                Console.WriteLine(" 3. Issue Credit Loan");
                Console.WriteLine(" 4. Mature Fixed Deposit");
                Console.WriteLine(" 5. Freeze/Thaw Account");
                Console.WriteLine(" 6. Process Cheque Queue");
                Console.WriteLine(" 7. Filter High Wealth Profiles");
                Console.WriteLine(" 8. View Bank Debtors List");
                Console.WriteLine(" 9. Delete Account Record");
                Console.WriteLine(" 10. Log Out");
                Console.WriteLine("=====================================");
                Console.Write("Selection: ");
                
                string choice = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(choice)) continue;

                switch (choice)
                {
                    case "1": OpenNewAccount(); break;
                    case "2": AuditAll(); break;
                    case "3": IssueLoan(); break;
                    case "4": ForceMaturity(); break;
                    case "5": ToggleFreeze(); break;
                    case "6": ReviewCheques(); break;
                    case "7": HighWealthFilter(); break;
                    case "8": ViewDebtors(); break;
                    case "9": DeleteAccount(); break;
                    case "10": LogOut(); break;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
        }

        public override void LogOut()
        {
            AdminStart = false;
            Console.WriteLine("Admin logged out.");
        }

        private void OpenNewAccount()
        {
            Console.WriteLine("\n--- NEW ACCOUNT ---");
            Console.Write("Holder Name: ");
            string name = Console.ReadLine();
            Console.Write("Account Number: ");
            string num = Console.ReadLine();
            if (CentralBankData.Find(num) != null)
            {
                Console.WriteLine("ID already exists."); return;
            }
            Console.Write("PIN: ");
            string pin = Console.ReadLine();
            Console.WriteLine("Type: 1=Savings, 2=Checking, 3=FixedDeposit");
            Console.Write("Selection: ");
            string typeStr = Console.ReadLine();
            Console.Write("Initial Deposit: ");
            
            
            if (!double.TryParse(Console.ReadLine(), out double deposit))
            {
                Console.WriteLine("Invalid allocation amount logic entered.");
                return;
            }

            BankAccount acc = null;
            if (typeStr == "1") acc = new SavingsAccount(num, name, pin, deposit);
            else if (typeStr == "2") acc = new CheckingAccount(num, name, pin, deposit);
            else if (typeStr == "3") acc = new FixedDepositAccount(num, name, pin, deposit);
            else return;

            Console.Write("Phone: ");
            acc.PhoneNumber = Console.ReadLine();
            Console.Write("Address: ");
            acc.AddressLine = Console.ReadLine();

            CentralBankData.AddAccount(acc);
            Console.WriteLine("Account created successfully.");
        }

        private void AuditAll()
        {
            Console.WriteLine("\n--- BANK GENERAL AUDIT REPORT ---");
            double totalCapital = 0;
            double totalDebt = 0;

            for (int i = 0; i < CentralBankData.AccountCount; i++)
            {
                BankAccount acc = CentralBankData.Database[i];
                totalCapital += acc.Balance;
                totalDebt += acc.LoanBalance;
                Console.WriteLine("Acc: " + acc.AccountNumber + " | Holder: " + acc.AccountHolderName + " | Bal: " + acc.Balance + " PKR | Loan: " + acc.LoanBalance + " PKR | Frozen: " + acc.IsFrozen);
            }
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Total Holding Deposits: " + totalCapital + " PKR");
            Console.WriteLine("Total Outstanding Loans: " + totalDebt + " PKR");
            Console.ReadLine();
        }

        private void IssueLoan()
        {
            Console.Write("\nEnter Account Number: ");
            BankAccount acc = CentralBankData.Find(Console.ReadLine());
            
            if (acc != null)
            {
                Console.WriteLine("1. Standard Loan");
                Console.WriteLine("2. Loan with Upfront Processing Fee");
                Console.Write("Select Route: ");
                string path = Console.ReadLine();

                Console.Write("Enter Loan Amount: ");
                
                if (!double.TryParse(Console.ReadLine(), out double amt))
                {
                    Console.WriteLine("Invalid financial assignment amount.");
                    return;
                }

                if (path == "2")
                {
                    Console.Write("Enter Processing Fee: ");
                    if (!double.TryParse(Console.ReadLine(), out double fee))
                    {
                        Console.WriteLine("Invalid tracking processing assignment fee.");
                        return;
                    }
                    acc.AddLoan(amt, fee);
                }
                else
                {
                    acc.AddLoan(amt);
                }
                Console.WriteLine("Loan added to system.");
            }
        }

        private void ForceMaturity()
        {
            Console.Write("\nEnter Fixed Deposit Account Number: ");
            BankAccount acc = CentralBankData.Find(Console.ReadLine());
            
            if (acc != null && acc is FixedDepositAccount fixedDepositAccount)
            {
                fixedDepositAccount.IsMatured = true;
                Console.WriteLine("Asset status switched to matured.");
            }
        }

        private void ToggleFreeze()
        {
            Console.Write("\nEnter Account Number: ");
            BankAccount acc = CentralBankData.Find(Console.ReadLine());
            
            if (acc != null)
            {
                Console.WriteLine("1=Freeze, 2=Thaw");
                string sel = Console.ReadLine();
                acc.IsFrozen = (sel == "1");
                Console.WriteLine("Status updated.");
            }
        }

        private void ReviewCheques()
        {
            Console.WriteLine("\n--- CHEQUE REQUEST QUEUE ---");
            
            if (CentralBankData.ChequeCount == 0) return;

            for (int i = 0; i < CentralBankData.ChequeCount; i++)
            {
                ChequeRequest r = CentralBankData.ChequeRegistry[i];
                Console.WriteLine("ID: " + r.RequestId + " | Acc: " + r.AccountNumber + " | Leaves: " + r.LeavesCount + " | Status: " + r.Status);
            }

            Console.Write("Enter Request ID to handle: ");
            
            string id = Console.ReadLine();
            ChequeRequest match = null;
            
            for (int i = 0; i < CentralBankData.ChequeCount; i++)
            {
                if (CentralBankData.ChequeRegistry[i].RequestId == id) match = CentralBankData.ChequeRegistry[i];
            }

            if (match != null && match.Status == "Pending")
            {
                Console.WriteLine("1=Approve, 2=Reject");
                string sel = Console.ReadLine();
                match.Status = (sel == "1") ? "Approved" : "Rejected";
                Console.WriteLine("Request updated.");
            }
        }

        private void HighWealthFilter()
        {
            Console.Write("\nEnter floor wealth filter amount: ");
            
            if (double.TryParse(Console.ReadLine(), out double floor))
            {
                for (int i = 0; i < CentralBankData.AccountCount; i++)
                {
                    BankAccount acc = CentralBankData.Database[i];
                    if (acc.Balance >= floor)
                    {
                        Console.WriteLine("Target Profile Match -> " + acc.AccountNumber + " : " + acc.AccountHolderName + " (" + acc.Balance + " PKR)");
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid matching floor pattern criteria entry.");
            }
            Console.ReadLine();
        }

        private void ViewDebtors()
        {
            Console.WriteLine("\n--- ACTIVE BANK DEBTORS ---");
            
            for (int i = 0; i < CentralBankData.AccountCount; i++)
            {
                BankAccount acc = CentralBankData.Database[i];
                if (acc.LoanBalance > 0)
                {
                    Console.WriteLine("User: " + acc.AccountHolderName + " [" + acc.AccountNumber + "] owes " + acc.LoanBalance + " PKR");
                }
            }
            Console.ReadLine();
        }

        private void DeleteAccount()
        {
            Console.Write("\nEnter Account ID to delete: ");
            
            BankAccount acc = CentralBankData.Find(Console.ReadLine());
            
            if (acc != null)
            {
                if (acc.Balance > 0 || acc.LoanBalance > 0)
                {
                    Console.WriteLine("Cannot delete account with active balances or loans.");
                    return;
                }
                CentralBankData.RemoveAccount(acc);
                Console.WriteLine("Account purged safely.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SavingsAccount user1 = new SavingsAccount("ACC1", "Saqib", "1111", 50000.0);
            user1.PhoneNumber = "0300-1234567";
            user1.AddressLine = "Block 4, Gulshan-e-Iqbal";
            CentralBankData.AddAccount(user1);

            CheckingAccount user2 = new CheckingAccount("ACC2", "Abdullah", "2222", 150000.0);
            user2.PhoneNumber = "0333-7654321";
            user2.AddressLine = "Phase 6, DHA";
            CentralBankData.AddAccount(user2);

            Login runtime = new Login();
            runtime.CheckingLogin();
        }
    }
}