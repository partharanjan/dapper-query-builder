using DapperQueryBuilder.Result;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static DapperQueryBuilder.Query.LikeConditionQuery;

namespace DapperQueryBuilder
{
    public interface IQueryBuilder<TEntity> where TEntity : class
    {
        QueryBuilder<TEntity> Where<TField>(Expression<Func<TEntity, TField>> field, string operatorName, object value, string condition = "AND");

        QueryBuilder<TEntity> WhereBetween<TField>(Expression<Func<TEntity, TField>> field, TField fromValue, TField toValue, string condition = "AND");

        QueryBuilder<TEntity> WhereIn<TField>(Expression<Func<TEntity, TField>> field, List<TField> items, string condition = "AND");

        QueryBuilder<TEntity> Like<TField>(Expression<Func<TEntity, TField>> field, LikeConditionType likeCondition, string value, string condition = "AND");

        QueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> expression);

        QueryBuilder<TEntity> Set<TField>(Expression<Func<TEntity, TField>> field, TField value);

        TEntity Set<TModel>(TModel model) where TModel : class;

        TEntity Set<TModel>(TModel model, Func<Attribute, object> customAttributeFunc) where TModel : class;

        QueryBuilderResult Build(BuildType buildType);
    }
}
