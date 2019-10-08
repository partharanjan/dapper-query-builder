using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Query
{
    public class ConditionQuery
    {
        public string Column { get; set; }
        public string Condition { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }

        public ConditionQuery(string condition, string columnName, string operatorName, object value)
        {
            Column = columnName;
            Operator = operatorName;
            Value = value;
            Condition = condition;
        }
    }
}
