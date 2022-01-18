using System.Data;
using Microsoft.Data.SqlClient;
using s3638151_a1.Models;
using s3638151_a1.Utilities;

namespace s3638151_a1.Database;

public class TransactionData
{
    private readonly string _connectionString;

    public TransactionData(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Transaction> GetTransactions(int accountID)
    {
        Logins login = new Logins();
        using var connection = new SqlConnection(_connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "select * from [Transaction] where [AccountNumber] = @accountID";
        command.Parameters.AddWithValue("accountID", accountID);

        return command.GetDataTable().Select().Select(x => new Transaction
        {
            TransactionID = x.Field<int>("TransactionID"),
            TransactionType = x.Field<string>("TransactionType"),
            AccountNumber = accountID,
            DestinationAccountNumber = x.Field<int>("DestinationAccountNumber"),
            Amount = x.Field<decimal>("Amount"),
            Comment = x.Field<string>("Comment"),
            TransactionTimeUtc = x.Field<DateTime>("TransactionTimeUtc").ToString()
        }).ToList();
    }

    public void InsertTransaction(Transaction transaction)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            @"insert into [Transaction] (TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc)
            values (@transactionType, @accountNumber, @destinationAccountNumber, @amount, @comment, @transactionTimeUtc)";
        command.Parameters.AddWithValue("transactionType", transaction.TransactionType);
        command.Parameters.AddWithValue("accountNumber", transaction.AccountNumber);
        command.Parameters.AddWithValue("destinationAccountNumber", transaction.DestinationAccountNumber);
        command.Parameters.AddWithValue("amount", transaction.Amount);
        command.Parameters.AddWithValue("comment", transaction.Comment);
        command.Parameters.AddWithValue("transactionTimeUtc", transaction.TransactionTimeUtc);

        command.ExecuteNonQuery();
    }
}


