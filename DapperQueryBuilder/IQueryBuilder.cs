using DapperQueryBuilder.Result;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DapperQueryBuilder
{
    public interface IQueryBuilder<TEntity> where TEntity : class
    {
        QueryBuilder<TEntity> Where(string columnName, string operatorName, object value);

        QueryBuilder<TEntity> Where(string columnName, string operatorName, object value, string condition = "AND");

        QueryBuilder<TEntity> WhereBetween(string columnName, object fromValue, object toValue, string condition = "AND");

        QueryBuilder<TEntity> WhereIn<T>(string columnName, List<T> items, string condition = "AND");

        QueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> expression);

        QueryBuilder<TEntity> Set<TField>(Expression<Func<TEntity, TField>> field, TField value);

        TEntity Set<TModel>(TModel model) where TModel : class;

        TEntity Set<TModel>(TModel model, Func<Attribute, object> customAttributeFunc) where TModel : class;

        QueryBuilderResult Build(BuildType buildType);
    }
}
