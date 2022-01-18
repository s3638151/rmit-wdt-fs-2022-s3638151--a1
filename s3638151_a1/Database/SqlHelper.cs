using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3638151_a1.Database
{
    public class SqlHelper : IData
    {
        private readonly string _connectionString;

        public SqlHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool Add(string sql, SqlParameter[] values)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(values);

            int rows = command.ExecuteNonQuery();

            return rows > 0;
        }
    }
}
