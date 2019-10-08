using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Result
{
    public enum BuildType { Insert, Update, Delete, Select };

    public class QueryBuilderResult
    {
        public string Sql { get; set; }
        public DynamicParameters Parameters { get; set; }

        public QueryBuilderResult()
        {
            Sql = string.Empty;
            Parameters = new DynamicParameters();
        }
    }
}
