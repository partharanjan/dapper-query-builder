using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Query
{
    public class InConditionQuery : ConditionQuery
    {
        public InConditionQuery(string conidtion, string columnName, string operatorName, object value) : base(conidtion, columnName, operatorName, value)
        {

        }
    }
}
