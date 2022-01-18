using System.Data;
using Microsoft.Data.SqlClient;
using s3638151_a1.Models;
using s3638151_a1.Utilities;

namespace s3638151_a1.Database;

public class CustomerData
{
	private readonly string _connectionString;

    public List<Customer> Customers { get; }

	public CustomerData(string connectionString)
	{
        _connectionString = connectionString;

        using var connection = new SqlConnection(_connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "select * from Customer";

        var accountData = new AccountData(_connectionString);
        var loginData = new LoginData(_connectionString);

        Customers = command.GetDataTable().Select().Select(x => new Customer
        {
            CustomerID = x.Field<int>("CustomerID"),
            //CustomerID = (int)x["CustomerID"],
            //Name = (string)x["Name"],
            Name = x.Field<string>("Name"),
            Address = x.Field<string>("Address"),
            City = x.Field<string>("City"),
            PostCode = x.Field<string>("PostCode"),
            //Pets = petManager.GetPets(x.Field<int>("PersonID"))
            Accounts = accountData.GetAccounts(x.Field<int>("CustomerID")),
            Login = loginData.GetLogins(x.Field<int>("CustomerID"))

        }).ToList();
    }

    public void InsertCustomer(Customer customer)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
                "insert into Customer (CustomerID, Name, Address, City, PostCode) values (@customerID, @name, @address, @city, @postCode)";
        command.Parameters.AddWithValue("customerID", customer.CustomerID);
        command.Parameters.AddWithValue("name", customer.Name);
        command.Parameters.AddWithValue("address", customer.Address.GetObjectOrDbNull());
        command.Parameters.AddWithValue("city", customer.City.GetObjectOrDbNull());
        command.Parameters.AddWithValue("postCode", customer.PostCode.GetObjectOrDbNull());

        command.ExecuteNonQuery();
        
    
	}
}

