
using Pure.Profiler.Configuration;
using Pure.Profiler.Timings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace Pure.Profiler.Data
{
    /// <summary>
    /// A <see cref="DbCommand"/> wrapper which supports DB profiling.
    /// </summary>  
    public class ProfiledDbCommand : DbCommand, ICloneable
    {
        public   DbCommand _command;
        private  Func<IDbProfiler> _getDbProfiler;
        private bool _bindByName;
        private static Link<Type, Action<IDbCommand, bool>> bindByNameCache;

        #region Properties

        /// <summary>
        /// Gets or sets the tags of the <see cref="DbTiming"/> which will be created internally.
        /// </summary>
        public TagCollection Tags { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public ProfiledDbCommand(DbCommand command, IDbProfiler dbProfiler, IEnumerable<string> tags = null)
            : this(command, dbProfiler, tags == null ? null : new TagCollection(tags))
        {
        }

        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public ProfiledDbCommand(DbCommand command, IDbProfiler dbProfiler, TagCollection tags)
            : this(command, () => dbProfiler, tags)
        {
        }

        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="getDbProfiler">Gets the <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>        
        public ProfiledDbCommand(DbCommand command, Func<IDbProfiler> getDbProfiler, IEnumerable<string> tags = null)
            : this(command, getDbProfiler, tags == null ? null : new TagCollection(tags))
        {
        }

        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="getDbProfiler">Gets the <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public ProfiledDbCommand(DbCommand command, Func<IDbProfiler> getDbProfiler, TagCollection tags)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (getDbProfiler == null)
            {
                throw new ArgumentNullException("getDbProfiler");
            }

            _command = command;
            _getDbProfiler = getDbProfiler;

            Tags = tags;
        }

        #endregion

        #region DbCommand Members

        /// <summary>
        /// Attempts to cancels the execution of a <see cref="DbCommand"/>.
        /// </summary>
        public override void Cancel()
        {
            _command.Cancel();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source. 
        /// </summary>
        public override string CommandText
        {
            get
            {
                return _command.CommandText;
            }
            set
            {
                _command.CommandText = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error. 
        /// </summary>
        public override int CommandTimeout
        {
            get
            {
                return _command.CommandTimeout;
            }
            set
            {
                _command.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Indicates or specifies how the <see cref="CommandText"/> property is interpreted. 
        /// </summary>
        public override CommandType CommandType
        {
            get
            {
                return _command.CommandType;
            }
            set
            {
                _command.CommandType = value;
            }
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DbParameter"/> object. 
        /// </summary>
        /// <returns>Returns the created <see cref="DbParameter"/>.</returns>
        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        /// <summary>
        /// Gets or sets the <see cref="DbConnection"/> used by this DbCommand. 
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                return _command.Connection;
            }
            set
            {
                if (value is ProfiledDbConnection)
                    _command.Connection = (value as ProfiledDbConnection).WrappedConnection;
                else
                    _command.Connection = value;
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="DbParameter"/> objects. 
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                return _command.Parameters;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DbTransaction"/> within which this <see cref="DbCommand"/> object executes. 
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return _command.Transaction;
            }
            set
            {
                if (value is ProfiledDbTransaction)
                    _command.Transaction = (value as ProfiledDbTransaction).WrappedTransaction;
                else
                    _command.Transaction = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command object should be visible in a customized interface control. 
        /// </summary>
        public override bool DesignTimeVisible
        {
            get { return _command.DesignTimeVisible; }
            set { _command.DesignTimeVisible = value; }
        }

        /// <summary>
        /// Gets the internal command.
        /// </summary>
        public DbCommand InternalCommand => _command;
        /// <summary>
        /// Gets or sets a value indicating whether or not to bind by name.
        /// If the underlying command supports BindByName, this sets/clears the underlying
        /// implementation accordingly. This is required to support OracleCommand from Dapper
        /// </summary>
        public bool BindByName
        {
            get => _bindByName;
            set
            {
                if (_bindByName != value)
                {
                    if (_command != null)
                    {
                        GetBindByName(_command.GetType())?.Invoke(_command, value);
                    }

                    _bindByName = value;
                }
            }
        }
        /// <summary>
        /// Get the binding name.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <returns>The <see cref="Action"/>.</returns>
        private static Action<IDbCommand, bool> GetBindByName(Type commandType)
        {
            if (commandType == null) return null; // GIGO
            if (Link<Type, Action<IDbCommand, bool>>.TryGet(bindByNameCache, commandType, out var action))
            {
                return action;
            }

            var prop = commandType
#if NETSTANDARD1_5
                .GetTypeInfo()
#endif
                .GetProperty("BindByName", BindingFlags.Public | BindingFlags.Instance);
            action = null;
            ParameterInfo[] indexers;
            MethodInfo setter;
            if (prop?.CanWrite == true && prop.PropertyType == typeof(bool)
                && ((indexers = prop.GetIndexParameters()) == null || indexers.Length == 0)
                && (setter = prop.GetSetMethod()) != null)
            {
                var method = new DynamicMethod(commandType.Name + "_BindByName", null, new[] { typeof(IDbCommand), typeof(bool) });
                var il = method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, commandType);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(OpCodes.Callvirt, setter, null);
                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand, bool>)method.CreateDelegate(typeof(Action<IDbCommand, bool>));
            }

            // cache it            
            Link<Type, Action<IDbCommand, bool>>.TryAdd(ref bindByNameCache, commandType, ref action);
            return action;
        }

        /// <summary>
        /// Executes the command text against the connection. 
        /// </summary>
        /// <param name="behavior">The <see cref="CommandBehavior"/>.</param>
        /// <returns></returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return _command.ExecuteReader();

            try
            {
               

                DbDataReader reader = null;
                dbProfiler.ExecuteDbCommand(
                    DbExecuteType.Reader
                    , _command
                    , () => {
                        reader = _command.ExecuteReader(behavior);

                        return reader;
                    } 
                    , Tags);

                var profiledReader = reader as ProfiledDbDataReader;
                if (profiledReader != null)
                {
                    return profiledReader;
                }

                return new ProfiledDbDataReader(reader, dbProfiler);
            }
            catch (Exception ex)
            {
                if (Tags == null)
                {
                    Tags = new TagCollection();

                }
                Tags.Add(ProfilingSession.FailOnErrorMark);
                DbTiming dbTiming = new DbTiming(ProfilingSession.Current.Profiler, DbExecuteType.Reader, _command, 0) { Tags = Tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();

               throw new PureProfilerException("ProfiledDbCommand", ex);;
            }

        }

      
        //IDataReader IDbCommand.ExecuteReader()
        //{
        //    return _command.ExecuteReader(CommandBehavior.Default);
        //}

        //IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        //{
        //    var dbProfiler = _getDbProfiler();
        //    if (dbProfiler == null) return _command.ExecuteReader();

        //    try
        //    {

        //        IDataReader reader = null;
        //        reader = dbProfiler.ExecuteDbCommand(
        //            DbExecuteType.Reader
        //            , _command
        //            , () => {
        //                reader = _dbCommand.ExecuteReader(behavior);

        //                return reader;
        //            }
        //            , Tags);

        //        var profiledReader = reader as ProfiledDbDataReader;
        //        if (profiledReader != null)
        //        {
        //            return profiledReader;
        //        }

        //        return new ProfiledDbDataReader(reader, dbProfiler);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (Tags == null)
        //        {
        //            Tags = new TagCollection();

        //        }
        //        Tags.Add(ProfilingSession.FailOnErrorMark);
        //        DbTiming dbTiming = new DbTiming(ProfilingSession.Current.Profiler, DbExecuteType.Reader, _command, 0) { Tags = Tags };
        //        // if not executing reader, stop the sql timing right after execute()
        //        dbTiming.Stop();

        //       throw new PureProfilerException("ProfiledDbCommand", ex);;
        //    }

        //}

        /// <summary>
        /// Executes a SQL statement against a connection object. 
        /// </summary>
        /// <returns>Returns The number of rows affected. </returns>
        public override int ExecuteNonQuery()
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return _command.ExecuteNonQuery();

            try
            {
                int affected = 0;
                dbProfiler.ExecuteDbCommand(
                    DbExecuteType.NonQuery, _command, () => { affected = _command.ExecuteNonQuery(); return affected; }, Tags);
                return affected;
            }
            catch (Exception ex)
            {
                if (Tags == null)
                {
                    Tags = new TagCollection();

                }
                Tags.Add(ProfilingSession.FailOnErrorMark);
                DbTiming dbTiming = new DbTiming(ProfilingSession.Current.Profiler, DbExecuteType.NonQuery, _command, 0) { Tags = Tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();

               throw new PureProfilerException("ProfiledDbCommand", ex);;
            }

        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored. 
        /// </summary>
        /// <returns>The first column of the first row in the result set. </returns>
        public override object ExecuteScalar()
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return _command.ExecuteScalar();

            try
            {
                object returnValue = null;
                dbProfiler.ExecuteDbCommand(
                    DbExecuteType.Scalar, _command, () => { returnValue = _command.ExecuteScalar(); return returnValue; }, Tags);
                return returnValue;
            }
            catch (Exception ex)
            {
                if (Tags == null)
                {
                    Tags = new TagCollection();

                }
                Tags.Add(ProfilingSession.FailOnErrorMark);
                DbTiming dbTiming = new DbTiming(ProfilingSession.Current.Profiler, DbExecuteType.Scalar, _command, null) { Tags = Tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();

               throw new PureProfilerException("ProfiledDbCommand", ex);;
            }

        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public override void Prepare()
        {
            _command.Prepare();
        }

        /// <summary>
        /// Gets or sets how command results are applied to a row.
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return _command.UpdatedRowSource;
            }
            set
            {
                _command.UpdatedRowSource = value;
            }
        }

        /// <summary>
        /// Gets whether or not can raise events.
        /// </summary>
        //protected override bool CanRaiseEvents
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbCommand"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_command != null)
                    _command.Dispose();
            }
           
            _command = null; 
            base.Dispose(disposing);
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return await _command.ExecuteReaderAsync(behavior, cancellationToken);

            try
            {



                DbDataReader reader = null;
                var result = await dbProfiler.ExecuteDbCommandAsync(
                    DbExecuteType.Reader
                    , _command
                    , async () => {
                        reader = await _command.ExecuteReaderAsync(behavior, cancellationToken);
                        return reader;
                    }
                    , Tags);
                ¡¡
                

                var profiledReader = reader as ProfiledDbDataReader;
                if (profiledReader != null)
                {
                    return profiledReader;
                }

                return new ProfiledDbDataReader(reader, dbProfiler);
 ¡¡
            }
            catch (Exception ex)
            {
                if (Tags == null)
                {
                    Tags = new TagCollection();

                }
                Tags.Add(ProfilingSession.FailOnErrorMark);
                DbTiming dbTiming = new DbTiming(ProfilingSession.Current.Profiler, DbExecuteType.Reader, _command, 0) { Tags = Tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();

               throw new PureProfilerException("ProfiledDbCommand", ex);;
            }

        


        }


        /// <summary>
        /// Executes NonQuery.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {


            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return (int)(await _command.ExecuteNonQueryAsync(cancellationToken));

            try
            {
                int affected = 0;
              
                await dbProfiler.ExecuteDbCommandAsync(
               DbExecuteType.NonQuery, _command, async () => { affected = await _command.ExecuteNonQueryAsync(cancellationToken); return affected; }, Tags);

                return affected;
            }
            catch (Exception ex)
            {
                if (Tags == null)
                {
                    Tags = new TagCollection();

                }
                Tags.Add(ProfilingSession.FailOnErrorMark);
                DbTiming dbTiming = new DbTiming(ProfilingSession.Current.Profiler, DbExecuteType.NonQuery, _command, 0) { Tags = Tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();

                throw new PureProfilerException("ProfiledDbCommand", ex);
            }

            //var dbProfiler = _getDbProfiler();
            //if (dbProfiler == null) return (int)(await _command.ExecuteNonQueryAsync(cancellationToken));

            //return (int)await dbProfiler.ExecuteDbCommandAsync(
            //    DbExecuteType.NonQuery, _command, async () => { return await _command.ExecuteNonQueryAsync(cancellationToken); }, Tags);
        }

        /// <summary>
        /// Executes Scalar.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {

            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return await _command.ExecuteScalarAsync(cancellationToken);

            try
            {
                object returnValue = null;
                  await dbProfiler.ExecuteDbCommandAsync(
              DbExecuteType.Scalar, _command, async () => { returnValue = await _command.ExecuteScalarAsync(cancellationToken); return returnValue; }, Tags);
                 
                return returnValue;
            }
            catch (Exception ex)
            {
                if (Tags == null)
                {
                    Tags = new TagCollection();

                }
                Tags.Add(ProfilingSession.FailOnErrorMark);
                DbTiming dbTiming = new DbTiming(ProfilingSession.Current.Profiler, DbExecuteType.Scalar, _command, null) { Tags = Tags };
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();

                throw new PureProfilerException("ProfiledDbCommand", ex);
            }


            //var dbProfiler = _getDbProfiler();
            //if (dbProfiler == null) return await _command.ExecuteScalarAsync(cancellationToken);

            //return await dbProfiler.ExecuteDbCommandAsync(
            //    DbExecuteType.Scalar, _command, async () => { return await _command.ExecuteScalarAsync(cancellationToken); }, Tags);
        }


        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var cmdCloneable = _command as ICloneable;
            var cmdClone = cmdCloneable == null ? _command : cmdCloneable.Clone() as DbCommand;

            return new ProfiledDbCommand(cmdClone, _getDbProfiler, Tags) { Connection = cmdClone.Connection };
        }

        #endregion
    }
}
