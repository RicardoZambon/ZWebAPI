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
        /// <param name="valueSelector">The value selector.</param>
        /// <param name="displaySelector">The display selector.</param>
        /// <returns>The catalog result model.</returns>
        public static CatalogResultModel<TKey> GetCatalog<TEntity, TKey>(this IQueryable<TEntity> query, ICatalogParameters parameters, Expression<Func<TEntity, TKey>> valueSelector, Expression<Func<TEntity, string?>> displaySelector)
            where TEntity : class
            where TKey : struct
        {
            // Builds the SELECT query dynamically.
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            InvocationExpression valueExpression = Expression.Invoke(valueSelector, parameter);
            InvocationExpression displayExpression = Expression.Invoke(displaySelector, parameter);

            NewExpression newExpression = Expression.New(typeof(CatalogEntryModel<TKey>));
            List<MemberBinding> bindings = new() {
                Expression.Bind(typeof(CatalogEntryModel<TKey>).GetProperty(nameof(CatalogEntryModel<TKey>.Value))!, valueExpression),
                Expression.Bind(typeof(CatalogEntryModel<TKey>).GetProperty(nameof(CatalogEntryModel<TKey>.Display))!, displayExpression),
            };

            MemberInitExpression memberInit = Expression.MemberInit(newExpression, bindings);

            Expression<Func<TEntity, CatalogEntryModel<TKey>>> lambda = Expression.Lambda<Func<TEntity, CatalogEntryModel<TKey>>>(memberInit, parameter);

            // Execute the query.
            IQueryable<CatalogEntryModel<TKey>> catalogQuery = query.Select(lambda);
            bool hasCriteria = false;

            // When needed, apply criteria.
            if (!string.IsNullOrWhiteSpace(parameters.Criteria))
            {
                hasCriteria = true;
                catalogQuery = catalogQuery
                    .Where(x => x.Display != null && EF.Functions.Like(x.Display.ToLower(), $"%{parameters.Criteria.ToLower()}%"));
            }

            // Build the result class.
            CatalogResultModel<TKey> result = new();

            if (parameters.MaxResults > 0 && catalogQuery.Count() > parameters.MaxResults)
            {
                if (!hasCriteria)
                {
                    result.ShouldUseCriteria = true;
                }
                else
                {
                    catalogQuery = catalogQuery.Take(parameters.MaxResults);
                }
            }

            if (!result.ShouldUseCriteria)
            {
                result.Entries = catalogQuery.OrderBy(x => x.Display);
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
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns>Return the query filtered.</returns>
        public static IQueryable<TEntity> TryFilter<TEntity, TValue>(this IQueryable<TEntity> query, ISummaryParameters parameters, Expression<Func<TEntity, TValue>> selector, FilterTypes filterType)
        {
            MemberExpression? memberExpression = GetMemberExpression(selector);
            if (memberExpression is null)
            {
                return query;
            }

            string parameterName = GetParameterName(memberExpression);
            if (!parameters.HasFilter(parameterName))
            {
                return query;
            }

            object? filterValue = parameters.GetFilterValue(parameterName, typeof(TValue));

            return RunFilter(query, selector.Parameters.First(), memberExpression, filterValue, filterType);
        }

        /// <summary>
        /// Tries the filter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns>Return the query filtered.</returns>
        public static IQueryable<TEntity> TryFilter<TEntity, TValue>(this IQueryable<TEntity> query, ISummaryParameters parameters, Expression<Func<TEntity, TValue>> selector, string parameterName, FilterTypes filterType)
        {
            if (!parameters.HasFilter(parameterName))
            {
                return query;
            }

            MemberExpression? memberExpression = GetMemberExpression(selector);
            if (memberExpression is null)
            {
                return query;
            }

            object? filterValue = parameters.GetFilterValue(parameterName, typeof(TValue));

            return RunFilter(query, selector.Parameters.First(), memberExpression, filterValue, filterType);
        }

        /// <summary>
        /// Tries to filter the query with arbitrary value.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns>Return the query filtered.</returns>
        public static IQueryable<TEntity> TryFilterWithValue<TEntity, TValue>(this IQueryable<TEntity> query, Expression<Func<TEntity, TValue>> selector, object? filterValue, FilterTypes filterType)
        {
            MemberExpression? memberExpression = GetMemberExpression(selector);

            if (memberExpression is null)
            {
                return query;
            }

            return RunFilter(query, selector.Parameters.First(), memberExpression, filterValue, filterType);
        }
        #endregion

        #region Private methods
        private static Expression<Func<TEntity, bool>>? BuildLambda<TEntity>(Expression body, ParameterExpression parameter)
            => Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        private static MemberExpression? GetMemberExpression<TEntity, TValue>(Expression<Func<TEntity, TValue>> selector)
        {
            MemberExpression? memberExpression = null;

            if (selector.Body is MemberExpression selectorMemberExpression)
            {
                memberExpression = selectorMemberExpression;
            }
            else if (selector.Body is UnaryExpression selectorUnaryExpression && selectorUnaryExpression.Operand is MemberExpression)
            {
                memberExpression = (MemberExpression)selectorUnaryExpression.Operand;
            }

            return memberExpression;
        }

        private static string GetParameterName(MemberExpression? memberExpression)
        {
            Stack<string> members = new();
            while (memberExpression != null)
            {
                members.Push(memberExpression.Member.Name);
                memberExpression = memberExpression.Expression as MemberExpression;
            }
            return string.Join(string.Empty, members);
        }

        private static IQueryable<TEntity> RunFilter<TEntity>(this IQueryable<TEntity> query, ParameterExpression parameter, MemberExpression memberExpression, object? filterValue, FilterTypes filterType)
        {
            if (filterType == FilterTypes.Like)
            {
                filterValue = $"%{filterValue}%";
            }

            UnaryExpression value = Expression.Convert(Expression.Constant(filterValue), memberExpression.Type);

            Expression<Func<TEntity, bool>>? where = null;
            switch (filterType)
            {
                case FilterTypes.Like:
                    if (typeof(DbFunctionsExtensions).GetMethods().FirstOrDefault(x => x.Name == "Like" && x.GetParameters().Length == 3) is MethodInfo method)
                    {
                        where = BuildLambda<TEntity>(Expression.Call(method, Expression.Constant(EF.Functions), memberExpression, value), parameter);
                    }
                    break;
                case FilterTypes.Equals:
                    where = BuildLambda<TEntity>(Expression.Equal(memberExpression, value), parameter);
                    break;
                case FilterTypes.LessThan:
                    where = BuildLambda<TEntity>(Expression.LessThan(memberExpression, value), parameter);
                    break;
                case FilterTypes.LessThanOrEqual:
                    where = BuildLambda<TEntity>(Expression.LessThanOrEqual(memberExpression, value), parameter);
                    break;
                case FilterTypes.GreaterThan:
                    where = BuildLambda<TEntity>(Expression.GreaterThan(memberExpression, value), parameter);
                    break;
                case FilterTypes.GreatherThanOrEqual:
                    where = BuildLambda<TEntity>(Expression.GreaterThanOrEqual(memberExpression, value), parameter);
                    break;
            }

            if (where is not null)
            {
                return query.Where(where);
            }

            return query;
        }
        #endregion
    }
}