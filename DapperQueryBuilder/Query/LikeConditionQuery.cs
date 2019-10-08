using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder.Query
{
    public class LikeConditionQuery : ConditionQuery
    {
        public enum LikeConditionType { StartsWith,Contains,EndsWith };

        public LikeConditionType LikeType { get; set; }

        public LikeConditionQuery(string conidtion, string columnName, LikeConditionType likeType, object value) : base(conidtion, columnName, null, value)
        {
            LikeType = likeType;
        }
    }
}
