using System.Data;
using Microsoft.Data.SqlClient;
using s3638151_a1.Models;
using s3638151_a1.Utilities;

namespace s3638151_a1.Database;

public class AccountData
{
    private readonly string _connectionString;

    public AccountData(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Account> GetAccounts(int customerID)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "select * from Account where CustomerID = @customerID";
        command.Parameters.AddWithValue("customerID", customerID);

        return command.GetDataTable().Select().Select(x => new Account
        {
            AccountNumber = x.Field<int>("AccountNumber"),
            AccountType = x.Field<string>("AccountType"),
            CustomerID = customerID,
            Balance = x.Field<decimal>("Balance"),

        }).ToList();
    }

    public List<Account> GetAccountsByAccountNumber(int acountNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "select * from Account where AccountNumber = @acountNumber";
        command.Parameters.AddWithValue("acountNumber", acountNumber);

        return command.GetDataTable().Select().Select(x => new Account
        {
            AccountNumber = acountNumber,
            AccountType = x.Field<string>("AccountType"),
            CustomerID = x.Field<int>("CustomerID"),
            Balance = x.Field<decimal>("Balance"),

        }).ToList();
    }

    public void InsertAccount(Account account)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"insert into Account (AccountNumber, AccountType, CustomerID, Balance)
            values (@accountNumber, @accountType, @customerID, @balance)";
        command.Parameters.AddWithValue("accountNumber", account.AccountNumber);
        command.Parameters.AddWithValue("accountType", account.AccountType);
        command.Parameters.AddWithValue("customerID", account.CustomerID);
        command.Parameters.AddWithValue("balance", account.Balance);

        command.ExecuteNonQuery();
    }

    public bool UpdateAccount(Account account)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"update Account set Balance = @balance WHERE CustomerID = @customerID AND AccountNumber = @accountNumber";
        command.Parameters.AddWithValue("accountNumber", account.AccountNumber);
        command.Parameters.AddWithValue("accountType", account.AccountType);
        command.Parameters.AddWithValue("customerID", account.CustomerID);
        command.Parameters.AddWithValue("balance", account.Balance);

        int rows = command.ExecuteNonQuery();

        return rows > 0;
    }
}


