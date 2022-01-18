using s3638151_a1.Database;
using s3638151_a1.Models;
using s3638151_a1.Utilities;

namespace s3638151_a1;

public class Menu
{
    private readonly CustomerData _customerData;
    private readonly LoginData _loginData;
    private readonly AccountData _accountData;
    private readonly TransactionData _transactionData;
    private Logins _login;

    public Menu(string connectionString)
    {
        _customerData = new CustomerData(connectionString);
        _loginData = new LoginData(connectionString);
        _accountData = new AccountData(connectionString);
        _transactionData = new TransactionData(connectionString);
    }

    public void Run()
    {
        Login();

        var runMenu = true;
        while (runMenu)
        {
            PrintMenu();

            Console.Write("\rEnter an option: ");
            var input = Console.ReadLine();
            Console.WriteLine();

            if (!int.TryParse(input, out var option) || !option.IsInRange(1, 6))
            {
                Console.WriteLine("Invalid input.");
                Console.WriteLine();
                continue;
            }

            switch (option)
            {
                case 1:
                    Deposit();
                    break;
                case 2:
                    Withdraw();
                    break;
                case 3:
                    Transfer();
                    break;
                case 4:
                    MyStatement();
                    break;
                case 5:
                    _login = new Logins();
                    Console.WriteLine("Logout successfully！");
                    Console.Clear();
                    Login();
                    break;
                case 6:
                    Console.WriteLine("Program ending.");
                    runMenu = false;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        Console.WriteLine("Good bye!");
    }

    private void Login()
    {
        var loginResult = false;
        //Login
        while (!loginResult)
        {
            Console.Write("Enter Login ID: ");
            string loginID = Console.ReadLine();
            Console.Write("Enter Password: ");
            string password = "";

            //Loop in your password
            var enterPassword = true;
            while (enterPassword)
            {
                var akey = Console.ReadKey(true);
                var key = akey.KeyChar.ToString();
                if (key != "\r")
                {
                    password += key;
                    Console.Write("*");
                }
                else
                {
                    enterPassword = false;
                }
            }

            //Check login
            _login = _loginData.GetLogin(loginID);

            if (_login != null && _login.CustomerID > 0)
            {
                loginResult = SimpleHashing.PBKDF2.Verify(_login.PasswordHash, password);

                if (!loginResult)
                {
                    Console.WriteLine();
                    Console.WriteLine("Login failed！");
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Login failed！");
            }
        }
    }

    private void PrintMenu()
    {
        const string MenuOutput =
            @"===============Mattew Bolger=================
                [1] Deposit
                [2] Withdraw
                [3] Transfer
                [4] My Statement
                [5] Logout
                [6] Exit";

        Console.WriteLine();
        Console.WriteLine(MenuOutput.TrimStartOnAllLines());
        Console.WriteLine();
    }

    private void Deposit()
    {
        int customerID = _login.CustomerID;
        List<Account> accounts = _accountData.GetAccounts(customerID);

        showAccount(accounts);

        Console.WriteLine("Please enter the account number:");
        string number = Console.ReadLine();

        int acountNumbe = 0;
        int.TryParse(number, out acountNumbe);

        if (acountNumbe == 0)
        {
            Console.WriteLine("Please enter the correct account number!");
            return;
        }

        if (accounts.Count > 0)
        {
            Account account = accounts.FindLast(item => item.AccountNumber == acountNumbe);

            if (account != null)
            {
                Console.WriteLine("Please enter the deposit amount:");
                string str = Console.ReadLine();

                decimal money = 0;
                decimal.TryParse(str, out money);

                if (money > 0)
                {
                    Console.WriteLine("Please enter comment:");
                    string comment = Console.ReadLine();

                    account.Balance = account.Balance + money;
                    if (_accountData.UpdateAccount(account))
                    {
                        //Insert transaction record
                        Transaction transaction = new Transaction()
                        {
                            TransactionType = "D",
                            AccountNumber = account.AccountNumber,
                            DestinationAccountNumber = account.AccountNumber,
                            Amount = money,
                            Comment = comment,
                            TransactionTimeUtc = DateTime.UtcNow.ToString()
                        };
                        _transactionData.InsertTransaction(transaction);

                        Console.WriteLine("Deposit of ${0} successfully, account balance is now ${1}", money, Math.Round(account.Balance, 2));
                    }
                }
                else
                {
                    Console.WriteLine("Please enter the correct amount!");
                }
            }
            else
            {
                Console.WriteLine("Account not found!");
            }
        }
        else
        {
            Console.WriteLine("Unopened account!");
        }
    }

    private void Withdraw()
    {
        int customerID = _login.CustomerID;
        List<Account> accounts = _accountData.GetAccounts(customerID);

        showAccount(accounts);

        Console.WriteLine("Please enter the account number:");
        string number = Console.ReadLine();
        bool feeFree = true;

        int acountNumbe = 0;
        int.TryParse(number, out acountNumbe);

        if (acountNumbe == 0)
        {
            Console.WriteLine("Please enter the correct account number!");
            return;
        }

        if (accounts.Count > 0)
        {
            Account account = accounts.FindLast(item => item.AccountNumber == acountNumbe);

            if (account != null)
            {
                Console.WriteLine("Please enter the withdraw amount:");
                string str = Console.ReadLine();

                decimal money = 0;
                decimal.TryParse(str, out money);

                if (money > 0)
                {
                    Console.WriteLine("Please enter comment:");
                    string comment = Console.ReadLine();

                    feeFree = !CheckFee(account);

                    //The withdrawal amount is greater than the balance
                    if (money > account.Balance)
                    {
                        Console.WriteLine("Not sufficient funds !");
                        return;
                    }

                    //The withdrawal amount is equal to the balance, determine whether there is a handling fee
                    if (account.Balance - money < 0.05M)
                    {
                        if (!feeFree)
                        {
                            Console.WriteLine("Not sufficient funds !");
                            return;
                        }
                    }

                    // The minimum balance allowed in a savings account is $0
                    if (account.AccountType == "C")
                    {
                        if (account.Balance - money < 300)
                        {
                            Console.WriteLine("The minimum balance allowed in a savings account is $300!");
                            return;
                        }

                        //The withdrawal amount is equal to the balance, determine whether there is a handling fee
                        if (account.Balance - money < 300.05M)
                        {
                            if (!feeFree)
                            {
                                Console.WriteLine("The minimum balance allowed in a savings account is $300!");
                                return;
                            }
                        }
                    }

                    account.Balance = account.Balance - money;
                    if (!feeFree)
                    {
                        account.Balance = account.Balance - 0.05M;
                    }

                    if (_accountData.UpdateAccount(account))
                    {
                        //Insert transaction record
                        Transaction transaction = new Transaction()
                        {
                            TransactionType = "W",
                            AccountNumber = account.AccountNumber,
                            DestinationAccountNumber = account.AccountNumber,
                            Amount = money,
                            Comment = comment,
                            TransactionTimeUtc = DateTime.UtcNow.ToString()
                        };
                        _transactionData.InsertTransaction(transaction);

                        if (!feeFree)
                        {
                            Transaction transactionFee = new Transaction()
                            {
                                TransactionType = "S",
                                AccountNumber = account.AccountNumber,
                                DestinationAccountNumber = account.AccountNumber,
                                Amount = 0.05M,
                                Comment = "Service charge  $0.05",
                                TransactionTimeUtc = DateTime.UtcNow.ToString()
                            };
                            _transactionData.InsertTransaction(transactionFee);
                        }

                        Console.WriteLine("Withdraw of ${0} successfully, account balance is now ${1}", money, Math.Round(account.Balance, 2));
                    }
                }
                else
                {
                    Console.WriteLine("Please enter the correct amount!");
                }
            }
            else
            {
                Console.WriteLine("Account not found!");
            }
        }
        else
        {
            Console.WriteLine("Unopened account!");
        }
    }

    private void Transfer()
    {
        int customerID = _login.CustomerID;
        Console.WriteLine("Please enter the account number:");
        string number = Console.ReadLine();
        bool feeFree = true;

        int acountNumbe = 0;
        int.TryParse(number, out acountNumbe);

        if (acountNumbe == 0)
        {
            Console.WriteLine("Please enter the correct account number!");
            return;
        }

        Console.WriteLine("Please enter the transfer account:");
        number = Console.ReadLine();

        int acountNumbe2 = 0;
        int.TryParse(number, out acountNumbe2);

        if (acountNumbe2 == 0)
        {
            Console.WriteLine("Please enter the correct account number!");
            return;
        }

        //Do not transfer money to yourself
        if (acountNumbe == acountNumbe2)
        {
            Console.WriteLine("Do not transfer money to yourself!");
            return;
        }

        List<Account> accounts = _accountData.GetAccounts(customerID);

        if (accounts.Count > 0)
        {
            Account account = accounts.FindLast(item => item.AccountNumber == acountNumbe);
            Account transferAccount = new Account();

            if (account != null)
            {
                List<Account> transferAccounts = _accountData.GetAccountsByAccountNumber(acountNumbe2);

                if (transferAccounts.Count > 0)
                {
                    transferAccount = transferAccounts[0];
                }
                else
                {
                    Console.WriteLine("Transfer account not found!");
                    return;
                }

                Console.WriteLine("Please enter the transfer amount:");
                string str = Console.ReadLine();

                decimal money = 0;
                decimal.TryParse(str, out money);

                if (money > 0)
                {
                    Console.WriteLine("Please enter comment:");
                    string comment = Console.ReadLine();

                    feeFree = !CheckFee(account);

                    //The withdrawal amount is greater than the balance
                    if (money > account.Balance)
                    {
                        Console.WriteLine("Not sufficient funds !");
                        return;
                    }

                    //The withdrawal amount is equal to the balance, determine whether there is a handling fee
                    if (account.Balance - money < 0.1M)
                    {
                        if (!feeFree)
                        {
                            Console.WriteLine("Not sufficient funds !");
                            return;
                        }
                    }

                    // The minimum balance allowed in a savings account is $0
                    if (account.AccountType == "C")
                    {
                        if (account.Balance - money < 300)
                        {
                            Console.WriteLine("The minimum balance allowed in a savings account is $300!");
                            return;
                        }

                        //The withdrawal amount is equal to the balance, determine whether there is a handling fee
                        if (account.Balance - money < 300.1M)
                        {
                            if (!feeFree)
                            {
                                Console.WriteLine("The minimum balance allowed in a savings account is $300!");
                                return;
                            }
                        }
                    }

                    account.Balance = account.Balance - money;
                    if (!feeFree)
                    {
                        account.Balance = account.Balance - 0.1M;
                    }

                    transferAccount.Balance = transferAccount.Balance + money;

                    if (_accountData.UpdateAccount(account) && _accountData.UpdateAccount(transferAccount))
                    {
                        //Insert transaction record
                        Transaction transaction = new Transaction()
                        {
                            TransactionType = "T",
                            AccountNumber = account.AccountNumber,
                            DestinationAccountNumber = transferAccount.AccountNumber,
                            Amount = money,
                            Comment = comment,
                            TransactionTimeUtc = DateTime.UtcNow.ToString()
                        };
                        _transactionData.InsertTransaction(transaction);

                        if (!feeFree)
                        {
                            Transaction transactionFee = new Transaction()
                            {
                                TransactionType = "S",
                                AccountNumber = account.AccountNumber,
                                DestinationAccountNumber = account.AccountNumber,
                                Amount = 0.1M,
                                Comment = "Service charge  $0.1",
                                TransactionTimeUtc = DateTime.UtcNow.ToString()
                            };
                            _transactionData.InsertTransaction(transactionFee);
                        }

                        Console.WriteLine("Transfer successfully!");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter the correct amount!");
                }
            }
            else
            {
                Console.WriteLine("Account not found!");
            }
        }
        else
        {
            Console.WriteLine("Unopened account!");
        }
    }

    private void showAccount(List<Account> accounts)
    {
        foreach (Account account in accounts)
        {
            string type = account.AccountType == "S" ? "Savings" : "Checking";

            decimal aBalance = 0;

            if (account.AccountType == "S")
            {
                aBalance = account.Balance;
            }
            else if (account.AccountType == "C")
            {
                if (account.Balance > 300)
                {
                    aBalance = account.Balance - 300;
                }
                else
                {
                    aBalance = 0;
                }
            }

            Console.WriteLine(type + " " + account.AccountNumber + ", Balance: $" + Math.Round(account.Balance, 2) + ", Available Balance: $" + Math.Round(aBalance, 2));
        }
    }

    private bool CheckFee(Account account)
    {
        List<Transaction> transactions = _transactionData.GetTransactions(account.AccountNumber);

        if (transactions.Count > 0)
        {
            List<Transaction> feeTransactions = transactions.FindAll(item => item.TransactionType == "W" || item.TransactionType == "T");

            if (feeTransactions != null && feeTransactions.Count >= 2)
            {
                return true;
            }
        }

        return false;
    }

    private void MyStatement()
    {
        int customerID = _login.CustomerID;
        Console.WriteLine("Please enter the account number:");
        string number = Console.ReadLine();

        int acountNumbe = 0;
        int.TryParse(number, out acountNumbe);

        if (acountNumbe == 0)
        {
            Console.WriteLine("Please enter the correct account number!");
            return;
        }

        List<Account> accounts = _accountData.GetAccounts(customerID);

        if (accounts.Count > 0)
        {
            Account account = accounts.FindLast(item => item.AccountNumber == acountNumbe);

            if (account != null)
            {
                Console.WriteLine("{0, -17}{1, -15}{2, -11}", "AccountNumber", "AccountType", "Balance");
                //Console.WriteLine("{0}\t{1}\t{2}", account.AccountNumber, account.AccountType == "S" ? "Savings" : "Checking", account.Balance);
                account.Run();

                List<Transaction> transactions = _transactionData.GetTransactions(account.AccountNumber);

                if (transactions.Count > 0)
                {
                    int size = 4;
                    int startIndex = 0;
                    int endIndex = size;
                    int count = transactions.Count;

                    if (endIndex > count)
                    {
                        endIndex = count;
                    }

                    int timespan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;

                    //Arrange transactions in descending chronological order
                    transactions.Sort((x, y) => -DateTime.Parse(x.TransactionTimeUtc).CompareTo(DateTime.Parse(y.TransactionTimeUtc)));

                    Console.WriteLine("Transaction:");
                    Console.WriteLine("{0, -19}{1, -17}{2, -28}{3, -10}{4, -22}{5, -11}", "TransactionType", "AccountNumber", "DestinationAccountNumber", "Amount", "TransactionTimeUtc", "Comment");
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", transactions[i].TransactionType, transactions[i].AccountNumber, transactions[i].DestinationAccountNumber, Math.Round(transactions[i].Amount, 2), DateTime.Parse(transactions[i].TransactionTimeUtc).AddHours(timespan).ToString("dd/MM/yyyy"));
                        transactions[i].Run();
                    }
                    int page = count / size;

                    if (count % size > 0)
                    {
                        page++;
                    }

                    Console.WriteLine("Page {0} of {1}", 1, page);

                    while (startIndex > 0 || endIndex < count)
                    {
                        Console.WriteLine("Please use LeftArrow RightArrow to turn pages, press Enter to exit page turning:");
                        var akey = Console.ReadKey(true);
                        var key = akey.Key.ToString();

                        if (key != "Enter")
                        {
                            if (key == "LeftArrow")
                            {
                                if (endIndex > 0 && startIndex - 4 >= 0)
                                {
                                    if (endIndex == count)
                                    {
                                        endIndex = startIndex;
                                        startIndex -= size;
                                    }
                                    else
                                    {
                                        startIndex -= size;
                                        endIndex -= size;
                                    }

                                    for (int i = startIndex; i < endIndex; i++)
                                    {
                                        //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", transactions[i].TransactionType, transactions[i].AccountNumber, transactions[i].DestinationAccountNumber, Math.Round(transactions[i].Amount, 2), DateTime.Parse(transactions[i].TransactionTimeUtc).AddHours(timespan).ToString("dd/MM/yyyy"));
                                        transactions[i].Run();
                                    }

                                    Console.WriteLine("Page {0} of {1}", (startIndex + size) / size, page);
                                }
                                else
                                {
                                    Console.WriteLine("This is page one!");
                                }
                            }
                            else if (key == "RightArrow")
                            {
                                if (endIndex < count)
                                {
                                    if (endIndex + size <= count)
                                    {
                                        startIndex = endIndex;
                                        endIndex += size;
                                    }
                                    else
                                    {
                                        startIndex = endIndex;
                                        endIndex = count;
                                    }

                                    for (int i = startIndex; i < endIndex; i++)
                                    {
                                        //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", transactions[i].TransactionType, transactions[i].AccountNumber, transactions[i].DestinationAccountNumber, Math.Round(transactions[i].Amount, 2), DateTime.Parse(transactions[i].TransactionTimeUtc).AddHours(timespan).ToString("dd/MM/yyyy"));
                                        transactions[i].Run();
                                    }

                                    Console.WriteLine("Page {0} of {1}", (startIndex + size) / size, page);
                                }
                                else
                                {
                                    Console.WriteLine("This is the last page!");
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Account not found!");
            }
        }
        else
        {
            Console.WriteLine("Unopened account!");
        }
    }
}

//Template Method
//Code reuse is realized
//Ability to be flexible to sub - step changes, in accordance with the open - closed principle
public abstract class Show
{
    protected abstract void Display();

    public void Run()
    {
        Display();
    }
}