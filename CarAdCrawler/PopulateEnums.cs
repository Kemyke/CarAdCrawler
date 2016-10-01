using CarAdCrawler.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CarAdCrawler
{
    public class PopulateEnums
    {
        private const string createSeq = @"DO
                                            $$
                                            BEGIN
                                                    CREATE SEQUENCE {0}_seq;
                                            EXCEPTION WHEN duplicate_table THEN
                                                    -- do nothing, it's already there
                                            END
                                            $$ LANGUAGE plpgsql;";
        private const string createScript = @"CREATE TABLE IF NOT EXISTS enum_{0}
                                                (
                                                    id integer PRIMARY KEY DEFAULT nextval('{0}_seq'),
                                                    name varchar(355) NOT NULL
                                                )";

        private const string insertScript = @"INSERT INTO enum_{0}(id, name) VALUES ('{1}', '{2}')";

        public void PopulateEnum(Type enumType, string connStr)
        {
            using (var conn = new NpgsqlConnection(connStr))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(string.Format(createSeq, enumType.Name.ToLower()), conn);
                cmd.ExecuteNonQuery();
                cmd = new NpgsqlCommand(string.Format(createScript, enumType.Name.ToLower()), conn);
                cmd.ExecuteNonQuery();

                foreach (var name in Enum.GetNames(enumType))
                {
                    int id = (int)Enum.Parse(enumType, name);
                    cmd = new NpgsqlCommand(string.Format(checkLiteralScript, enumType.Name, id), conn);
                    if (cmd.ExecuteScalar() == null)
                    {
                        cmd = new NpgsqlCommand(string.Format(insertScript, enumType.Name, id, name), conn);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
