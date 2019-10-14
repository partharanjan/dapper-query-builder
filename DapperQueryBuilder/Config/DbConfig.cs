using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Config
{
    public class DbConfig
    {
        public enum DbType { Sql, Postgres };

        public DbType Type { get; private set; }

        public DbConfig(DbType dbType)
        {
            Type = dbType;
        }
    }
}
