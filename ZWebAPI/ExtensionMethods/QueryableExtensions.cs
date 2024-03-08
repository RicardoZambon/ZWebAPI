// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using ZWebAPI.Enums;
using ZWebAPI.Interfaces;
using ZWebAPI.Models.Catalog;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="System.Linq.IQueryable{T}"/>.
    /// </summary>
    public static class QueryableExtensions
    {
        #region Public methods
        /// <summary>
        /// Gets the catalog result model fom a query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="valueSelector">The value selector.</param>
        /// <returns>The catalog result model.</returns>
        public static CatalogResultModel<TKey> GetCatalog<TEntity, TKey>(this IQueryable<TEntity> query, ICatalogParameters parameters, Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, string?>> valueSelector)
            where TEntity : class
            where TKey : struct
        {
            CatalogResultModel<TKey> result = new();

            IQueryable<KeyValuePair<TKey, string?>> catalogQuery = query.ConvertToKeyValuePairs(keySelector, valueSelector);

            if (!string.IsNullOrEmpty(parameters.Criteria))
            {
                catalogQuery = catalogQuery.Where(x =>
                    x.Value != null
                    && EF.Functions.Like(x.Value.ToLower(), $"%{parameters.Criteria.ToLower()}%")
                );
            }

            if (parameters.MaxResults > 0 && catalogQuery.Count() > parameters.MaxResults)
            {
                result.ShouldUseCriteria = true;
            }
            else
            {
                result.Entries = catalogQuery
                    .Select(x => new CatalogEntryModel<TKey>()
                    {
                        Display = x.Value,
                        Value = x.Key,
                    });
            }

            return result;
        }

        /// <summary>
        /// Get a range from a query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Return the range results.</returns>
        public static IQueryable<TEntity> GetRange<TEntity>(this IQueryable<TEntity> query, IListParameters parameters)
        {
            if (parameters?.StartRow > 0)
            {
                query = query.Skip(parameters.StartRow);
            }

            if (parameters?.EndRow > 0)
            {
                query = query.Take(parameters.EndRow);
            }

            return query;
        }

        /// <summary>
        /// Tries to filter the query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="property">The property.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns>Return the query filtered.</returns>
        public static IQueryable<TEntity> TryFilter<TEntity>(this IQueryable<TEntity> query, IListParameters parameters, string property, FilterTypes filterType)
        {
            return query.TryFilter(parameters, property, property, filterType);
        }

        /// <summary>
        /// Tries to filter the query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="property">The property.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns>Return the query filtered.</returns>
        /// <exception cref="System.Exception">The property {typeProperty} was not found in the type {(filterProperty?.PropertyType ?? typeof(TEntity)).Name}.</exception>
        public static IQueryable<TEntity> TryFilter<TEntity>(this IQueryable<TEntity> query, IListParameters parameters, string property, string parameterName, FilterTypes filterType)
        {
            PropertyInfo? filterProperty = null;

            string[] properties = property.Split(".");
            foreach (string typeProperty in properties)
            {
                filterProperty = (filterProperty?.PropertyType ?? typeof(TEntity)).GetProperty(typeProperty);

                if (filterProperty is null)
                {
                    throw new Exception($"The property {typeProperty} was not found in the type {(filterProperty?.PropertyType ?? typeof(TEntity)).Name}.");
                }
            }

            if (parameters.HasFilter(parameterName) && filterProperty != null)
            {
                object? filterValue = parameters.GetFilterValue(parameterName, filterProperty.PropertyType);

                Expression<Func<TEntity, bool>>? where = null;
                switch (filterType)
                {
                    case FilterTypes.Like:
                        where = BuildLikeExpression<TEntity>(property, filterValue);
                        break;
                    case FilterTypes.Equals:
                        where = BuildEqualsExpression<TEntity>(property, filterValue);
                        break;
                    case FilterTypes.LessThan:
                        where = BuildLessThanExpression<TEntity>(property, filterValue);
                        break;
                    case FilterTypes.LessThanOrEqual:
                        where = BuildLessThanOrEqualExpression<TEntity>(property, filterValue);
                        break;
                    case FilterTypes.GreaterThan:
                        where = BuildGreaterThanExpression<TEntity>(property, filterValue);
                        break;
                    case FilterTypes.GreatherThanOrEqual:
                        where = BuildGreaterThanOrEqualExpression<TEntity>(property, filterValue);
                        break;
                }

                if (where != null)
                {
                    return query.Where(where);
                }
            }
            return query;
        }
        #endregion

        #region Private methods
        private static Expression<Func<TEntity, bool>>? BuildEqualsExpression<TEntity>(string propertyName, object? propertyValue)
        {
            ParameterExpression parameter = GetParameter<TEntity>();

            if (GetProperty(parameter, propertyName) is MemberExpression property)
            {
                ConstantExpression value = Expression.Constant(propertyValue);

                return BuildLambda<TEntity>(Expression.Equal(property, value), parameter);
            }
            return null;
        }

        private static Expression<Func<TEntity, bool>>? BuildGreaterThanExpression<TEntity>(string propertyName, object? propertyValue)
        {
            ParameterExpression parameter = GetParameter<TEntity>();

            if (GetProperty(parameter, propertyName) is MemberExpression property)
            {
                ConstantExpression value = Expression.Constant(propertyValue);

                return BuildLambda<TEntity>(Expression.GreaterThan(property, value), parameter);
            }
            return null;
        }

        private static Expression<Func<TEntity, bool>>? BuildGreaterThanOrEqualExpression<TEntity>(string propertyName, object? propertyValue)
        {
            ParameterExpression parameter = GetParameter<TEntity>();

            if (GetProperty(parameter, propertyName) is MemberExpression property)
            {
                ConstantExpression value = Expression.Constant(propertyValue);

                return BuildLambda<TEntity>(Expression.GreaterThanOrEqual(property, value), parameter);
            }
            return null;
        }

        private static Expression<Func<TEntity, bool>>? BuildLambda<TEntity>(Expression body, ParameterExpression parameter)
            => Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        private static Expression<Func<TEntity, bool>>? BuildLessThanExpression<TEntity>(string propertyName, object? propertyValue)
        {
            ParameterExpression parameter = GetParameter<TEntity>();

            if (GetProperty(parameter, propertyName) is MemberExpression property)
            {
                ConstantExpression value = Expression.Constant(propertyValue);

                return BuildLambda<TEntity>(Expression.LessThan(property, value), parameter);
            }
            return null;
        }

        private static Expression<Func<TEntity, bool>>? BuildLessThanOrEqualExpression<TEntity>(string propertyName, object? propertyValue)
        {
            ParameterExpression parameter = GetParameter<TEntity>();

            if (GetProperty(parameter, propertyName) is MemberExpression property)
            {
                ConstantExpression value = Expression.Constant(propertyValue);

                return BuildLambda<TEntity>(Expression.LessThanOrEqual(property, value), parameter);
            }
            return null;
        }

        private static Expression<Func<TEntity, bool>>? BuildLikeExpression<TEntity>(string propertyName, object? propertyValue)
        {
            if (typeof(DbFunctionsExtensions).GetMethods().FirstOrDefault(x => x.Name == "Like" && x.GetParameters().Length == 3) is MethodInfo method)
            {
                ParameterExpression parameter = GetParameter<TEntity>();

                if (GetProperty(parameter, propertyName) is MemberExpression property)
                {
                    ConstantExpression value = Expression.Constant($"%{propertyValue?.ToString() ?? string.Empty}%");

                    MethodCallExpression methodCall = Expression.Call(method, Expression.Constant(EF.Functions), property, value);

                    return BuildLambda<TEntity>(methodCall, parameter);
                }
            }
            return null;
        }

        private static IQueryable<KeyValuePair<TKey, string?>> ConvertToKeyValuePairs<TEntity, TKey>(this IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, string?>> valueSelector)
            where TEntity : class
            where TKey : struct
        {
            return query.Select(x => new KeyValuePair<TKey, string?>(
                keySelector.Compile().Invoke(x),
                valueSelector.Compile().Invoke(x)
            ));
        }

        private static ParameterExpression GetParameter<TEntity>()
            => Expression.Parameter(typeof(TEntity), typeof(TEntity).Name.ToLower());

        private static MemberExpression? GetProperty(Expression parameter, string propertyName)
        {
            MemberExpression? expression = null;

            string[] properties = propertyName.Split(".");
            foreach (string property in properties)
            {
                expression = Expression.Property(expression ?? parameter, property);
            }

            return expression;
        }
        #endregion
    }
}