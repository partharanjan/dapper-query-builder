using Dapper;
using DapperQueryBuilder.Helper;
using DapperQueryBuilder.Query;
using DapperQueryBuilder.Result;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DapperQueryBuilder
{
    public abstract class QueryBuilder<TEntity> : IQueryBuilder<TEntity> where TEntity : class
    {
        #region -> For Private
        private Dictionary<string, object> _updateColumns;
        private LinqHelper<TEntity> _linqHelper;
        private TEntity _entity;
        #endregion

        #region -> For Abstract
        public abstract object GetCustomAttributeValue(Attribute attribute);
        #endregion

        #region -> For Properties
        public List<ConditionQuery> Conditions { get; set; }
        public List<string> SelectColumns { get; set; }
        public string EntityName { get; private set; }
        public bool EnableSnakeCase { get; set; }
        public OrderByQuery OrderBy { get; set; }
        public List<string> GroupBy { get; set; }
        public string ExtraSql { get; set; }
        #endregion

        public QueryBuilder()
        {
            Clear();
            // generate entity name
            GenerateEntityName();
        }

        public void Clear()
        {
            _updateColumns = new Dictionary<string, object>();
            _linqHelper = new LinqHelper<TEntity>();
            //
            Conditions = new List<ConditionQuery>();
            SelectColumns = new List<string>();
            GroupBy = new List<string>();
        }

        public QueryBuilder<TEntity> Where(string columnName, string operatorName, object value)
        {
            Conditions.Add(new ConditionQuery(null, columnName, operatorName, value));
            return this;
        }

        public QueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> expression)
        {
            Conditions.Add(_linqHelper.GetQuery(expression));
            return this;
        }

        public QueryBuilder<TEntity> Where(string columnName, string operatorName, object value, string condition = "AND")
        {
            Conditions.Add(new ConditionQuery(condition, columnName, operatorName, value));
            return this;
        }

        public QueryBuilder<TEntity> WhereBetween(string columnName, object fromValue, object toValue, string condition = "AND")
        {
            Conditions.Add(new BetweenConditionQuery(condition, columnName, fromValue, toValue));
            return this;
        }

        public QueryBuilder<TEntity> WhereIn<T>(string columnName, List<T> items, string condition = "AND")
        {
            Conditions.Add(new InConditionQuery(condition, columnName, "=", items.ToArray()));
            return this;
        }

        public QueryBuilder<TEntity> Set<TField>(Expression<Func<TEntity, TField>> field, TField value)
        {
            var memberExpression = field.Body as MemberExpression;
            if (!_updateColumns.ContainsKey(memberExpression.Member.Name))
            {
                _updateColumns.Add(memberExpression.Member.Name, value);
            }
            return this;
        }

        public TEntity Set<TModel>(TModel model) where TModel : class
        {
            Type type = typeof(TEntity);
            _entity = (TEntity)Activator.CreateInstance(type);
            List<PropertyInfo> entityProperties = type.GetProperties().AsList();
            List<PropertyInfo> modelProperties = model.GetType().GetProperties().AsList();
            entityProperties.ForEach(propertyInfo =>
            {
                var customAttributes = propertyInfo.GetCustomAttributes().ToList();
                if(customAttributes.Count>0)
                {
                    var value = GetCustomAttributeValue(customAttributes[0]);
                    propertyInfo.SetValue(_entity, value);
                }
                else
                {
                    var modelPropertyInfo = modelProperties.Find(m => m.Name == propertyInfo.Name);
                    if (modelPropertyInfo != null)
                    {
                        propertyInfo.SetValue(_entity, modelPropertyInfo.GetValue(model));
                    }
                }
                
            });
            return _entity;
        }

        public QueryBuilderResult Build(BuildType buildType)
        {
            QueryBuilderResult result = null;
            switch (buildType)
            {
                case BuildType.Insert: { return BuildInsert(); }
                case BuildType.Update: { return BuildUpdate(); }
                case BuildType.Delete: { return BuildDelete(); }
                case BuildType.Select: { return BuildSelect(); }
            }
            return result;
        }

        #region -> Private

        private void GenerateEntityName()
        {
            Type obj = typeof(TEntity);
            TableAttribute tableAttribute = obj.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                EntityName = tableAttribute.Name;
            }
            else
            {
                throw new Exception("No entity name found.");
            }
        }

        private void BuildCondition(StringBuilder stringBuilder, DynamicParameters parameters)
        {
            if (Conditions != null && Conditions.Count > 0)
            {
                stringBuilder.Append(" where ");
                int index = -1;

                Dictionary<string, object> dirPramas = new Dictionary<string, object>();
                Conditions.ForEach(item =>
                {
                    index++;
                    item.Condition = index == 0 ? null : (string.IsNullOrEmpty(item.Condition) ? "AND" : item.Condition);
                    // add parameters
                    Type type = item.GetType();
                    if (type == typeof(ConditionQuery))
                    {
                        #region -> For Condition
                        if (item.Value != null)
                        {
                            if (!dirPramas.ContainsKey(item.Column))
                            {
                                dirPramas.Add(item.Column, item.Value);
                                stringBuilder.Append($" {item.Condition} {item.Column} {item.Operator} @{item.Column} ");
                            }
                        }
                        else
                        {
                            string nullExpression = item.Operator == "=" ? "IS NULL" : "IS NOT NULL";
                            stringBuilder.Append($" {item.Condition} {item.Column} {nullExpression}");
                        }
                        #endregion
                    }
                    else if (type == typeof(InConditionQuery))
                    {
                        #region -> For In
                        if (dirPramas.ContainsKey(item.Column))
                        {
                            dirPramas.Add(item.Column, item.Value);
                            stringBuilder.Append($" {item.Condition} {item.Column} {item.Operator} any(@{item.Column}) ");
                        }
                        #endregion
                    }
                    else if (type == typeof(BetweenConditionQuery))
                    {
                        #region -> For Between
                        var betweenItem = (BetweenConditionQuery)item;
                        string columnName = item.Column;
                        string from_column_name = $"{columnName}_from_value";
                        string to_column_name = $"{columnName}_to_value";
                        if (!dirPramas.ContainsKey(from_column_name) && !dirPramas.ContainsKey(to_column_name))
                        {
                            dirPramas.Add(from_column_name, betweenItem.FromValue);
                            dirPramas.Add(to_column_name, betweenItem.ToValue);
                            stringBuilder.Append($" {item.Condition} {columnName} between @{from_column_name} AND @{to_column_name} ");
                        }
                        #endregion
                    }
                });

                if (dirPramas.Count > 0)
                {
                    foreach (var item in dirPramas)
                    {
                        string columnName = item.Key;
                        if (EnableSnakeCase) { columnName = columnName.ToSnakeCase(); }
                        parameters.Add(columnName, item.Value);
                    }
                }
            }
        }

        private QueryBuilderResult BuildSelect()
        {
            QueryBuilderResult result = new QueryBuilderResult();
            StringBuilder sqlBuilder = new StringBuilder();

            #region -> Generate Select SQL
            if (SelectColumns != null && SelectColumns.Count > 0)
            {
                // select specific
                sqlBuilder.Append($"select {string.Join(",", SelectColumns)} from {EntityName}");
            }
            else
            {
                // select all columns
                sqlBuilder.Append($"select * from {EntityName}");
            }
            #endregion

            #region -> Condition SQL
            BuildCondition(sqlBuilder, result.Parameters);
            #endregion

            #region -> Order By
            if (OrderBy != null)
            {
                sqlBuilder.Append($" order by {OrderBy.Column} {OrderBy.OrderBy} ");
            }
            #endregion

            #region -> For Group By
            if (GroupBy!=null && GroupBy.Count>0)
            {
                sqlBuilder.Append($" group by {string.Join(",", GroupBy)} ");
            }
            #endregion

            #region -> Extra SQL
            if (!string.IsNullOrEmpty(ExtraSql))
            {
                sqlBuilder.Append($" {ExtraSql} ");
            }
            #endregion

            #region -> Generate
            result.Sql = sqlBuilder.ToString().Trim();
            if (EnableSnakeCase)
            {
                result.Sql = result.Sql.ToSnakeCase();
            }
            #endregion

            return result;
        }

        private QueryBuilderResult BuildUpdate()
        {
            QueryBuilderResult result = new QueryBuilderResult();
            StringBuilder sqlBuilder = new StringBuilder();

            sqlBuilder.Append($"update {EntityName} ");

            #region -> For Set
            if (_updateColumns.Count > 0)
            {
                List<string> setConditions = new List<string>();
                foreach (var value in _updateColumns)
                {
                    string columnName = value.Key;
                    if (EnableSnakeCase)
                    {
                        columnName = columnName.ToSnakeCase();
                    }
                    setConditions.Add($"{columnName}=@{columnName}");
                    result.Parameters.Add(columnName, value.Value);
                }
                // set value
                sqlBuilder.Append(" SET " + string.Join(",", setConditions));
            }

            #endregion

            #region -> Condition SQL
            BuildCondition(sqlBuilder, result.Parameters);
            #endregion

            #region -> Extra SQL
            if (!string.IsNullOrEmpty(ExtraSql))
            {
                sqlBuilder.Append($" {ExtraSql} ");
            }
            #endregion

            #region -> Generate
            result.Sql = sqlBuilder.ToString().Trim();
            if (EnableSnakeCase)
            {
                result.Sql = result.Sql.ToSnakeCase();
            }
            #endregion

            return result;
        }

        private QueryBuilderResult BuildDelete()
        {
            QueryBuilderResult result = new QueryBuilderResult();
            StringBuilder sqlBuilder = new StringBuilder();

            sqlBuilder.Append($"delete from {EntityName} ");

            #region -> Condition SQL
            BuildCondition(sqlBuilder, result.Parameters);
            #endregion

            #region -> Generate
            result.Sql = sqlBuilder.ToString().Trim();
            if (EnableSnakeCase)
            {
                result.Sql = result.Sql.ToSnakeCase();
            }
            #endregion

            return result;
        }

        private QueryBuilderResult BuildInsert()
        {
            if (_entity != null)
            {
                var values = new Dictionary<string, object>();
                Type type = _entity.GetType();
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo propertyInfo in properties)
                {
                    values.Add(propertyInfo.Name, propertyInfo.GetValue(_entity));
                }
                if (values.Count > 0)
                {
                    // get columns and values
                    var columnsNames = values.Keys.Select(m => m).ToList();
                    // generate sql
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"insert into {EntityName} ");
                    // append column names
                    stringBuilder.Append($" ({string.Join(',', columnsNames)}) ");
                    // append column values as dynamic parameters
                    stringBuilder.Append($" values ({string.Join(",", columnsNames.Select(m => "@" + m))})");

                    DynamicParameters parameters = new DynamicParameters();
                    foreach (var value in values)
                    {
                        parameters.Add($"@{value.Key.ToSnakeCase()}", value.Value);
                    }
                    return new QueryBuilderResult()
                    {
                        Sql = stringBuilder.ToString(),
                        Parameters = parameters
                    };
                }
            }
            return null;
        }

        #endregion
    }
}
