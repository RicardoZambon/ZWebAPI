// Ignore Spelling: Queryable

using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Exceptions;
using ZWebAPI.Interfaces;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods to materialize a filtered/sorted <see cref="IQueryable{T}"/> into either a paginated <see cref="ListResult{T}"/> envelope or an <see cref="ExportResult"/>.
    /// </summary>
    public static class QueryableListExtensions
    {
        #region Public methods
        /// <summary>
        /// Materializes the source query into an <see cref="ExportResult"/>, applying filters and sort from <paramref name="parameters"/>, skipping pagination and enforcing the export row cap.
        /// </summary>
        /// <typeparam name="TSrc">The entity type produced by the source query.</typeparam>
        /// <typeparam name="TDest">The list model type exposed to the exporter.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="mapperConfig">The AutoMapper configuration provider used to project <typeparamref name="TSrc"/> into <typeparamref name="TDest"/>.</param>
        /// <param name="listExporter">The exporter that turns the projected rows into the final file payload.</param>
        /// <param name="format">The desired file format.</param>
        /// <param name="fileBaseName">The file name without extension.</param>
        /// <returns>An <see cref="ExportResult"/> containing the file bytes, MIME content type and full file name.</returns>
        /// <exception cref="ArgumentNullException">When any required argument is <c>null</c>.</exception>
        /// <exception cref="ExportRowLimitExceededException">When the filtered row count exceeds <see cref="ExportConstants.MaxExportRows"/>.</exception>
        public static async Task<ExportResult> ToExportResultAsync<TSrc, TDest>(
            this IQueryable<TSrc> query,
            IListParameters parameters,
            AutoMapper.IConfigurationProvider mapperConfig,
            IListExporter listExporter,
            ExportFormat format,
            string fileBaseName)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentNullException.ThrowIfNull(mapperConfig);
            ArgumentNullException.ThrowIfNull(listExporter);

            parameters.StartRow = 0;
            parameters.EndRow = int.MaxValue;

            IQueryable<TSrc> filtered = query.GetRange(parameters);

            int totalRows = await CountAsync(filtered);
            if (totalRows > ExportConstants.MaxExportRows)
            {
                throw new ExportRowLimitExceededException(ExportConstants.MaxExportRows);
            }

            List<TDest> items = await ToListAsync(filtered.ProjectTo<TDest>(mapperConfig));

            return listExporter.Export(items, format, fileBaseName);
        }

        /// <summary>
        /// Materializes an already projected query into an <see cref="ExportResult"/>.
        /// </summary>
        /// <typeparam name="TDest">The list model type exposed to the exporter.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="listExporter">The exporter that turns the projected rows into the final file payload.</param>
        /// <param name="format">The desired file format.</param>
        /// <param name="fileBaseName">The file name without extension.</param>
        /// <returns>An <see cref="ExportResult"/> containing the file bytes, MIME content type and full file name.</returns>
        /// <exception cref="ArgumentNullException">When any required argument is <c>null</c>.</exception>
        /// <exception cref="ExportRowLimitExceededException">When the filtered row count exceeds <see cref="ExportConstants.MaxExportRows"/>.</exception>
        public static async Task<ExportResult> ToExportResultAsync<TDest>(
            this IQueryable<TDest> query,
            IListParameters parameters,
            IListExporter listExporter,
            ExportFormat format,
            string fileBaseName)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentNullException.ThrowIfNull(listExporter);

            parameters.StartRow = 0;
            parameters.EndRow = int.MaxValue;

            IQueryable<TDest> filtered = query.GetRange(parameters);

            int totalRows = await CountAsync(filtered);
            if (totalRows > ExportConstants.MaxExportRows)
            {
                throw new ExportRowLimitExceededException(ExportConstants.MaxExportRows);
            }

            List<TDest> items = await ToListAsync(filtered);

            return listExporter.Export(items, format, fileBaseName);
        }

        /// <summary>
        /// Materializes the source query into a <see cref="ListResult{TDest}"/> envelope.
        /// </summary>
        /// <typeparam name="TSrc">The entity type produced by the source query.</typeparam>
        /// <typeparam name="TDest">The list model type returned to the client.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="mapperConfig">The AutoMapper configuration provider used to project <typeparamref name="TSrc"/> into <typeparamref name="TDest"/>.</param>
        /// <returns>A <see cref="ListResult{TDest}"/> with the paginated items and the total unpaginated row count.</returns>
        /// <exception cref="ArgumentNullException">When any required argument is <c>null</c>.</exception>
        public static async Task<ListResult<TDest>> ToListResultAsync<TSrc, TDest>(
            this IQueryable<TSrc> query,
            IListParameters parameters,
            AutoMapper.IConfigurationProvider mapperConfig)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentNullException.ThrowIfNull(mapperConfig);

            int totalRows = await CountAsync(query);

            List<TDest> items = await ToListAsync(query
                .GetRange(parameters)
                .ProjectTo<TDest>(mapperConfig));

            return ListResult<TDest>.From(items, totalRows);
        }

        /// <summary>
        /// Materializes an already projected query into a <see cref="ListResult{TDest}"/> envelope.
        /// </summary>
        /// <typeparam name="TDest">The list model type returned to the client.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A <see cref="ListResult{TDest}"/> with the paginated items and the total unpaginated row count.</returns>
        /// <exception cref="ArgumentNullException">When any required argument is <c>null</c>.</exception>
        public static async Task<ListResult<TDest>> ToListResultAsync<TDest>(
            this IQueryable<TDest> query,
            IListParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(parameters);

            int totalRows = await CountAsync(query);

            List<TDest> items = await ToListAsync(query.GetRange(parameters));

            return ListResult<TDest>.From(items, totalRows);
        }
        #endregion

        #region Private methods
        private static Task<int> CountAsync<T>(IQueryable<T> query)
        {
            if (query.Provider is IAsyncQueryProvider)
            {
                return EntityFrameworkQueryableExtensions.CountAsync(query);
            }

            return Task.FromResult(query.Count());
        }

        private static Task<List<T>> ToListAsync<T>(IQueryable<T> query)
        {
            if (query.Provider is IAsyncQueryProvider)
            {
                return EntityFrameworkQueryableExtensions.ToListAsync(query);
            }

            return Task.FromResult(query.ToList());
        }
        #endregion
    }
}
