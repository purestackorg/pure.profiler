
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Pure.Profiler.Timings;

namespace Pure.Profiler.Data
{
    /// <summary>
    /// The default <see cref="IDbProfiler"/> implementation.
    /// </summary>
    public class DbProfiler : IDbProfiler
    {
        private readonly IProfiler _profiler;
        //private readonly ConcurrentDictionary<string, DbTiming> _inProgressDataReaders;
        private readonly ConcurrentDictionary<IDataReader, DbTiming> _inProgressDataReaders;

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DbProfiler"/>.
        /// </summary>
        /// <param name="profiler">The profiler.</param>
        public DbProfiler(IProfiler profiler)
        {
            if (profiler == null)
            {
                throw new ArgumentNullException("profiler");
            }

            _profiler = profiler;
            //_inProgressDataReaders = new ConcurrentDictionary<string, DbTiming>();
            _inProgressDataReaders = new ConcurrentDictionary<IDataReader, DbTiming>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes &amp; profiles the execution of the specified <see cref="IDbCommand"/>.
        /// </summary>
        /// <param name="executeType">The <see cref="DbExecuteType"/>.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to be executed &amp; profiled.</param>
        /// <param name="execute">
        ///     The execute handler, 
        ///     which should return the <see cref="IDataReader"/> instance if it is an ExecuteReader operation.
        ///     If it is not ExecuteReader, it should return null.
        /// </param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public virtual IDataReader ExecuteDbCommand(DbExecuteType executeType, DbCommand command, Func<object> execute, TagCollection tags)
        {
            if (execute == null)
            {
                return null;
            }

            if (command == null)
            {
                execute();
                return null;
            }
            object result = "";
            DbTiming dbTiming = null;

            var data = execute();

            var dataReader = data as IDataReader;
            if (dataReader == null)
            {
                if (data != null)
                {
                    if (executeType == DbExecuteType.NonQuery)
                    {
                        result = Convert.ToInt32(data);
                    }
                    else if (executeType == DbExecuteType.Scalar)
                    {
                        result = (data);
                    }
                }


                dbTiming = new DbTiming(_profiler, executeType, command, result) { Tags = tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();
                return null;
            }

            //int readerCount = 0;
            //while (dataReader.Read())
            //{
            //    readerCount++;
            //}

            //result = dataReader;

            dbTiming = new DbTiming(_profiler, executeType, command, result) { Tags = tags };
            dbTiming.FirstFetch();
            var reader = dataReader as ProfiledDbDataReader ??
                new ProfiledDbDataReader(dataReader, this);
            //var reader = new ProfiledDbDataReader(dataReader, this);
            _inProgressDataReaders[reader] = dbTiming;
            //_inProgressDataReaders[reader.Id] = dbTiming;

            return reader;
        }

        /// <summary>
        /// Notifies the profiler that the data reader has finished reading
        /// so that the DB timing attached to the data reading could be stopped.
        /// </summary>
        /// <param name="dataReader">The <see cref="IDataReader"/>.</param>
        public virtual void DataReaderFinished(IDataReader dataReader)
        {
            if (dataReader == null)
            {
                return;
            }

            var pdReader = dataReader as ProfiledDbDataReader;
            int rowCount = 0;
            if (pdReader != null)
            {
                rowCount = pdReader.RowCount;

                DbTiming dbTiming;
                //if (_inProgressDataReaders.TryRemove(pdReader.Id, out dbTiming))
                //{
                //    dbTiming.Data["executeResult"] = rowCount.ToString();
                //    dbTiming.Stop();
                //}
                if (_inProgressDataReaders.TryRemove(pdReader, out dbTiming))
                {
                    dbTiming.Data["executeResult"] = rowCount.ToString();
                    dbTiming.Stop();
                }

            }

            //DbTiming dbTiming;
            //if (_inProgressDataReaders.TryRemove(dataReader, out dbTiming)) 
            //{
            //    dbTiming.Data["executeResult"] = rowCount.ToString();
            //    dbTiming.Stop();
            //}
        }

 
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
        public async Task<object> ExecuteDbCommandAsync(DbExecuteType executeType, DbCommand command, Func<Task<object>> execute, TagCollection tags)
        {
            if (execute == null)
            {
                return null;
            }

            if (command == null)
            {
                var o =(await execute());
                return null;
            }
            object result = "";
            DbTiming dbTiming = null;

            var data = (await execute());

            var dataReader = data as IDataReader;
            if (dataReader == null)
            {
                if (data != null)
                {
                    if (executeType == DbExecuteType.NonQuery)
                    {
                        result = Convert.ToInt32(data);
                    }
                    else if (executeType == DbExecuteType.Scalar)
                    {
                        result = (data);
                    }
                }


                dbTiming = new DbTiming(_profiler, executeType, command, result) { Tags = tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();
                return null;
            }

            //int readerCount = 0;
            //while (dataReader.Read())
            //{
            //    readerCount++;
            //}

            //result = dataReader;

            dbTiming = new DbTiming(_profiler, executeType, command, result) { Tags = tags };
            dbTiming.FirstFetch();
            var reader = dataReader as ProfiledDbDataReader ??
                new ProfiledDbDataReader(dataReader, this);
            //var reader = new ProfiledDbDataReader(dataReader, this);
            _inProgressDataReaders[reader] = dbTiming;
            //_inProgressDataReaders[reader.Id] = dbTiming;

            return reader;

            //if (execute == null)
            //{
            //    return null;
            //}

            //if (command == null)
            //{
            //    return await execute();
            //}

            //var dbTiming = new DbTiming(_profiler, executeType, command) { Tags = tags };

            //if (executeType == DbExecuteType.Reader)
            //{
            //    // for ExecuteReader
            //    var dataReader = (await execute()) as DbDataReader;
            //    if (dataReader == null)
            //    {
            //        // if not executing reader, stop the sql timing right after execute()
            //        dbTiming.Stop();
            //        return null;
            //    }

            //    dbTiming.FirstFetch();
            //    var reader = dataReader as ProfiledDbDataReader ??
            //        new ProfiledDbDataReader(dataReader, this);
            //    _inProgressDataReaders[reader] = dbTiming;

            //    return reader;
            //}

            //// for ExecuteNonQuery and ExecuteScalar
            //try
            //{
            //    return await execute();
            //}
            //finally
            //{
            //    dbTiming.Stop();
            //}
        }
¡¡

        #endregion

        #region IDbProfiler Members

        //IDataReader IDbProfiler.ExecuteDbCommand(DbExecuteType executeType, IDbCommand command, Func<object> execute, TagCollection tags)
        //{
        //    return ExecuteDbCommand(executeType, command, execute, tags);
        //}

        void IDbProfiler.DataReaderFinished(IDataReader dataReader)
        //public virtual void DataReaderFinished(IDataReader dataReader)
        {
            DataReaderFinished(dataReader);
        }

    

        #endregion
    }
}
