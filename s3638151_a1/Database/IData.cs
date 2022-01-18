using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3638151_a1.Database
{
    public interface IData
    {
        bool Add(string sql, SqlParameter[] values);
    }
}
