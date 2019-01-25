

using Pure.Profiler.Timings;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pure.Profiler.Data
{
    /// <summary>
    /// Represents a generic DB profiler for profiling execution of <see cref="IDbCommand"/>.
    /// </summary>
    public interface IDbProfiler
    {
        /// <summary>
        /// Executes &amp; profiles the execution of the specified <see cref="DbCommand"/>.
        /// </summary>
        /// <param name="executeType">The <see cref="DbExecuteType"/>.</param>
        /// <param name="command">The <see cref="DbCommand"/> to be executed &amp; profiled.</param>
        /// <param name="execute">
        ///     The execute handler, 
        ///     which should return the <see cref="DbDataReader"/> instance if it is an ExecuteReader operation.
        ///     If it is not ExecuteReader, it should return null.
        /// </param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        IDataReader ExecuteDbCommand(DbExecuteType executeType, DbCommand command, Func<object> execute, TagCollection tags);
        
        /// <summary>
        /// Executes &amp; profiles the execution of the specified <see cref="DbCommand"/> asynchronously.
        /// </summary>
        /// <param name="executeType">The <see cref="DbExecuteType"/>.</param>
        /// <param name="command">The <see cref="DbCommand"/> to be executed &amp; profiled.</param>
        /// <param name="execute">
        ///     The execute handler, 
        ///     which should return a scalar value.
        /// </param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        Task<object> ExecuteDbCommandAsync(DbExecuteType executeType, DbCommand command, Func<Task<object>> execute, TagCollection tags);

        /// <summary>
        /// Notifies the profiler that the data reader has finished reading
        /// so that the DB timing attached to the data reading could be stopped.
        /// </summary>
        /// <param name="dataReader">The <see cref="DbDataReader"/>.</param>
        void DataReaderFinished(IDataReader dataReader);
    }
}
