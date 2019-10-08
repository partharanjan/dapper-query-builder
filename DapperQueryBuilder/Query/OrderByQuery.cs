using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Query
{
    public class OrderByQuery
    {
        public string Column { get; set; }
        public string OrderBy { get; set; }

        public OrderByQuery(string column, string orderBy = "asc")
        {
            Column = column;
            OrderBy = orderBy;
        }
    }
}
