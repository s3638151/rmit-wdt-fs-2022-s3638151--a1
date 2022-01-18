using System.Data;
using Microsoft.Data.SqlClient;
using s3638151_a1.Models;
using s3638151_a1.Utilities;

namespace s3638151_a1.Database;

public class LoginData
{
    private readonly string _connectionString;
    IData idata = null;

    public LoginData(string connectionString)
    {
        _connectionString = connectionString;
    }

    public LoginData(string connectionString, IData data)
    {
        _connectionString = connectionString;
        idata = data;
    }

    public Logins GetLogins(int customerID)
    {
        Logins login = new Logins();
        using var connection = new SqlConnection(_connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "select * from Login where CustomerID = @customerID";
        command.Parameters.AddWithValue("customerID", customerID);

        List<Logins> logins = command.GetDataTable().Select().Select(x => new Logins
        {
            LoginID = x.Field<string>("LoginID"),
            CustomerID = customerID,
            PasswordHash = x.Field<string>("PasswordHash"),

        }).ToList();

        if (logins.Count > 0)
        {
            login = logins[0];
        }

        return login;
    }

    //Run the loginID command to query login information
    public Logins GetLogin(string loginID)
    {
        Logins login = new Logins();
        using var connection = new SqlConnection(_connectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "select * from Login where LoginID = @LoginID";
        command.Parameters.AddWithValue("LoginID", loginID);

        List<Logins> logins = command.GetDataTable().Select().Select(x => new Logins
        {
            LoginID = loginID,
            CustomerID = x.Field<int>("CustomerID"),
            PasswordHash = x.Field<string>("PasswordHash"),

        }).ToList();

        if (logins.Count > 0)
        {
            login = logins[0];
        }

        return login;
    }

    //public void InsertLogin(Logins login)
    //{
    //    using var connection = new SqlConnection(_connectionString);
    //    connection.Open();

    //    using var command = connection.CreateCommand();
    //    command.CommandText =
    //        @"insert into Login (LoginID, CustomerID, PasswordHash)
    //        values (@loginID, @customerID, @passwordHash)";
    //    command.Parameters.AddWithValue("loginID", login.LoginID);
    //    command.Parameters.AddWithValue("customerID", login.CustomerID);
    //    command.Parameters.AddWithValue("passwordHash", login.PasswordHash);

    //    command.ExecuteNonQuery();
    //}

    //Dependency injection mode reduces code iteration
    public bool InsertLogin(Logins login)
    {
        string sql = @"insert into Login (LoginID, CustomerID, PasswordHash)
            values (@loginID, @customerID, @passwordHash)";
        List<SqlParameter> values = new List<SqlParameter>();
        values.Add(new SqlParameter("loginID", login.LoginID));
        values.Add(new SqlParameter("customerID", login.CustomerID));
        values.Add(new SqlParameter("passwordHash", login.PasswordHash));

        return idata.Add(sql, values.ToArray());
    }
}


