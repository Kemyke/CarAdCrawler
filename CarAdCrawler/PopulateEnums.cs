using CarAdCrawler.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CarAdCrawler
{
    public class PopulateEnums
    {
        private const string checkTableScript = @"select object_id('Enum_{0}') ";
        private const string checkLiteralScript = @"select Name from Enum_{0} where Id = {1}";

        private const string createScript = @"CREATE TABLE dbo.Enum_{0}
                                                (
                                                    Id int NOT NULL,
                                                    Name nvarchar(MAX) NOT NULL

                                                    CONSTRAINT pk_{0}Id PRIMARY KEY(Id)
                                                )";

        private const string insertScript = @"INSERT INTO dbo.Enum_{0}(Id, Name) VALUES ('{1}', '{2}')";

        public void PopulateEnum(Type enumType, string connStr)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(string.Format(checkTableScript, enumType.Name), conn);
                if (cmd.ExecuteScalar() == DBNull.Value)
                {
                    cmd = new SqlCommand(string.Format(createScript, enumType.Name), conn);
                    cmd.ExecuteNonQuery();
                }

                foreach (var name in Enum.GetNames(enumType))
                {
                    int id = (int)Enum.Parse(enumType, name);
                    cmd = new SqlCommand(string.Format(checkLiteralScript, enumType.Name, id), conn);
                    if (cmd.ExecuteScalar() == null)
                    {
                        cmd = new SqlCommand(string.Format(insertScript, enumType.Name, id, name), conn);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
