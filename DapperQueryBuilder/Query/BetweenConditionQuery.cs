using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Query
{
    public class BetweenConditionQuery : ConditionQuery
    {
        public object FromValue { get; set; }
        public object ToValue { get; set; }
        public BetweenConditionQuery(string conidtion, string columnName, object fromValue, object toValue) : base(conidtion, columnName, null, null)
        {
            FromValue = fromValue;
            ToValue = toValue;
        }
    }
}
