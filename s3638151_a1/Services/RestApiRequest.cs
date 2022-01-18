//using System.data;
using Newtonsoft.Json;
using s3638151_a1.Database;
using s3638151_a1.Models;

namespace s3638151_a1.Services

{
    public static class RestApiRequest
    {
        public static void GetAndSaveCustomers(string connectionString)
        {
            // Check if any customers already exist and if they do stop.
            var customerData = new CustomerData(connectionString);
            if (customerData.Customers.Any())
                return;

            const string Url = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";

            // Contact webservice.
            using var client = new HttpClient();
            var json = client.GetStringAsync(Url).Result;

            // Convert JSON into objects.
            var customers = JsonConvert.DeserializeObject<List<Customer>>(json, new JsonSerializerSettings
            {
                // See here for DateTime format string documentation:
                // https://docs.microsoft.com/en-au/dotnet/standard/base-types/custom-date-and-time-format-strings
                DateFormatString = "dd/MM/yyyy"
            });

            // Insert into database.
            IData data = new SqlHelper(connectionString);
            var accountData = new AccountData(connectionString);
            var loginData = new LoginData(connectionString, data);
            var transactionData = new TransactionData(connectionString);

            foreach (var customer in customers)
            {
                customerData.InsertCustomer(customer);

                foreach (var account in customer.Accounts)
                {
                    // Set pet's PersonID.
                    account.CustomerID = customer.CustomerID;
                    accountData.InsertAccount(account);

                    foreach (var trans in account.Transactions)
                    {
                        trans.TransactionType = "";

                        switch (trans.Comment)
                        {
                            case "Opening balance":
                                trans.TransactionType = "S";
                                break;
                            case "First deposit":
                            case "Second deposit":
                            case "Deposited $500":
                            case "Deposited $0.95":
                                trans.TransactionType = "D";
                                break;
                            case null:
                                trans.TransactionType = "D";
                                break;
                        }

                        trans.AccountNumber = account.AccountNumber;
                        trans.DestinationAccountNumber = account.AccountNumber;

                        trans.Comment = trans.Comment == null ? "" : trans.Comment;

                        transactionData.InsertTransaction(trans);
                    }
                }

                customer.Login.CustomerID = customer.CustomerID;

                loginData.InsertLogin(customer.Login);

            }

        }
    }
}

