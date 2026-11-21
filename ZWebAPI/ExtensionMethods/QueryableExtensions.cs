// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using ZWebAPI.Enums;
using ZWebAPI.Interfaces;
using ZWebAPI.Models;
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
        /// <param name="customFilter">The custom filter.</param>
        /// <returns>The catalog result model.</returns>
        public static CatalogResultModel<TKey> GetCatalog<TEntity, TKey>(this IQueryable<TEntity> query, ICatalogParameters parameters, Expression<Func<TEntity, TKey>> valueSelector, Expression<Func<TEntity, string?>> displaySelector, Expression<Func<CatalogEntryModel<TKey>, bool>>? customFilter = null)
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
                if (customFilter is not null)
                {
                    catalogQuery = catalogQuery
                        .Where(customFilter);
                }
                else
                {
                    catalogQuery = catalogQuery
                        .Where(x => x.Display != null && EF.Functions.Like(x.Display.ToLower(), $"%{parameters.Criteria.ToLower()}%"));
                }
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
        /// Sorts a query by the fields in <see cref="IListParameters.Sort"/>.
        /// </summary>
        /// <remarks>
        /// Each key names a public property of <typeparamref name="TEntity"/> (case-insensitive) and
        /// each value is <c>asc</c> or <c>desc</c>; anything other than <c>desc</c>/<c>descending</c>
        /// sorts ascending. Keys apply in the order sent. Unknown properties and properties that are
        /// not simple values (collections, nested objects) are ignored, so the client cannot reach
        /// anything outside the list model. When at least one field applies, the <c>ID</c> property,
        /// if any, is added last so pages stay stable. With no sort, the query is returned as it is,
        /// keeping any order the caller set.
        /// </remarks>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The sorted query.</returns>
        public static IQueryable<TEntity> ApplySort<TEntity>(this IQueryable<TEntity> query, IListParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (parameters?.Sort is not { Count: > 0 } sort)
            {
                return query;
            }

            IOrderedQueryable<TEntity>? ordered = null;
            HashSet<string> sortedProperties = new(StringComparer.Ordinal);

            foreach (KeyValuePair<string, string> field in sort)
            {
                PropertyInfo? property = FindSortableProperty<TEntity>(field.Key);
                if (property is null || !sortedProperties.Add(property.Name))
                {
                    continue;
                }

                ordered = OrderByProperty(ordered ?? query, property, IsDescending(field.Value), thenBy: ordered is not null);
            }

            if (ordered is null)
            {
                return query;
            }

            PropertyInfo? key = FindSortableProperty<TEntity>("ID");
            if (key is not null && sortedProperties.Add(key.Name))
            {
                ordered = OrderByProperty(ordered, key, descending: false, thenBy: true);
            }

            return ordered;
        }

        /// <summary>
        /// Get a range from a query, capped at <see cref="ListPaging.MaxPageSize"/> rows.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters. <see cref="IListParameters.EndRow"/> is the number of rows to take; zero or less means the cap.</param>
        /// <returns>Return the range results.</returns>
        public static IQueryable<TEntity> GetRange<TEntity>(this IQueryable<TEntity> query, IListParameters parameters)
        {
            return query.GetRange(parameters, ListPaging.MaxPageSize);
        }

        /// <summary>
        /// Get a range from a query, capped at <paramref name="maxPageSize"/> rows.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters. <see cref="IListParameters.EndRow"/> is the number of rows to take; zero or less means the cap.</param>
        /// <param name="maxPageSize">The most rows to return.</param>
        /// <returns>Return the range results.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxPageSize"/> is zero or less.</exception>
        public static IQueryable<TEntity> GetRange<TEntity>(this IQueryable<TEntity> query, IListParameters parameters, int maxPageSize)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxPageSize);

            if (parameters?.StartRow > 0)
            {
                query = query.Skip(parameters.StartRow);
            }

            int requested = parameters?.EndRow ?? 0;

            return query.Take(requested > 0 ? Math.Min(requested, maxPageSize) : maxPageSize);
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

        private static PropertyInfo? FindSortableProperty<TEntity>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            PropertyInfo? property;
            try
            {
                property = typeof(TEntity).GetProperty(name.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
            catch (AmbiguousMatchException)
            {
                // Two properties differ only by case; refuse rather than guess.
                return null;
            }

            if (property is null || !property.CanRead || property.GetIndexParameters().Length > 0)
            {
                return null;
            }

            return IsSortableType(property.PropertyType) ? property : null;
        }

        private static bool IsDescending(string? direction)
        {
            return string.Equals(direction?.Trim(), "desc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(direction?.Trim(), "descending", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSortableType(Type type)
        {
            Type underlying = Nullable.GetUnderlyingType(type) ?? type;

            return underlying.IsPrimitive
                || underlying.IsEnum
                || underlying == typeof(string)
                || underlying == typeof(decimal)
                || underlying == typeof(DateTime)
                || underlying == typeof(DateTimeOffset)
                || underlying == typeof(DateOnly)
                || underlying == typeof(TimeOnly)
                || underlying == typeof(TimeSpan)
                || underlying == typeof(Guid);
        }

        private static IOrderedQueryable<TEntity> OrderByProperty<TEntity>(IQueryable<TEntity> query, PropertyInfo property, bool descending, bool thenBy)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
            LambdaExpression keySelector = Expression.Lambda(Expression.Property(parameter, property), parameter);

            string methodName = (thenBy, descending) switch
            {
                (false, false) => nameof(Queryable.OrderBy),
                (false, true) => nameof(Queryable.OrderByDescending),
                (true, false) => nameof(Queryable.ThenBy),
                (true, true) => nameof(Queryable.ThenByDescending),
            };

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                methodName,
                [typeof(TEntity), property.PropertyType],
                query.Expression,
                Expression.Quote(keySelector));

            return (IOrderedQueryable<TEntity>)query.Provider.CreateQuery<TEntity>(call);
        }
        #endregion
    }
}