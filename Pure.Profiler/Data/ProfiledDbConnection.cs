
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Pure.Profiler.Data
{
    /// <summary>
    /// A <see cref="IDbConnection"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbConnection : DbConnection
    {
        //private readonly IDbConnection _connection;
        private  DbConnection _connection;
        private  Func<IDbProfiler> _getDbProfiler;

        public DbConnection WrappedConnection { get { return _connection; } }

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbConnection(IDbConnection connection, IDbProfiler dbProfiler)
            : this(connection, () => dbProfiler)
        {
        }

        /// <summary>
        /// Initializes a <see cref="ProfiledDbConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to be profiled.</param>
        /// <param name="getDbProfiler">Gets the <see cref="IDbProfiler"/>.</param>
        public ProfiledDbConnection(IDbConnection connection, Func<IDbProfiler> getDbProfiler)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (getDbProfiler == null)
            {
                throw new ArgumentNullException("getDbProfiler");
            }

            if (connection is ProfiledDbConnection)
                throw new ArgumentException("connection to be profiled should not be a instance of ProfiledDbConnection!");
            
            _connection = connection as DbConnection;
            if (_connection == null)
            {
                throw new ArgumentNullException("connection is not DbConnection");
            }
            if (_connection != null)
            {
                _connection.StateChange += StateChangeHandler;
            }
            _getDbProfiler = getDbProfiler;
        }

        #endregion

        #region DbConnection Members

        /// <summary>
        /// Starts a database transaction. 
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction. </param>
        /// <returns>An object representing the new transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var transaction = _connection.BeginTransaction(isolationLevel);
            return new ProfiledDbTransaction(transaction, this);
        }

        /// <summary>
        /// Changes the current database for an open connection. 
        /// </summary>
        /// <param name="databaseName">Specifies the name of the database for the connection to use.</param>
        public override void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection. 
        /// </summary>
        public override void Close()
        {
            _connection.Close();
        }

        /// <summary>
        /// Gets or sets the string used to open the connection. 
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return _connection.ConnectionString;
            }
            set
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.ConnectionString = value;
                }
            }
        }

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection. 
        /// </summary>
        /// <returns></returns>
        protected override DbCommand CreateDbCommand()
        {
            var command = _connection.CreateCommand();
            return new ProfiledDbCommand(command, _getDbProfiler);
        }

        /// <summary>
        /// Gets the name of the database server to which to connect. 
        /// </summary>
        public override string DataSource
        {
            get
            {
                if (_connection != null)
                {
                    return _connection.DataSource;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the name of the current database after a connection is opened, or the database name specified in the connection string before the connection is opened. 
        /// </summary>
        public override string Database
        {
            get { return _connection.Database; }
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString. 
        /// </summary>
        public override void Open()
        {
            _connection.Open();
        }

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected. 
        /// </summary>
        public override string ServerVersion
        {
            get
            {
                if (_connection != null)
                {
                    return _connection.ServerVersion;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a string that describes the state of the connection. 
        /// </summary>
        public override ConnectionState State
        {
            get { return _connection.State; }
        }

        /// <summary>
        /// Gets whether or not can raise events.
        /// </summary>
        protected override bool CanRaiseEvents
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the time to wait while establishing a connection before terminating the attempt and generating an error. 
        /// </summary>
        public override int ConnectionTimeout
        {
            get
            {
                return _connection.ConnectionTimeout;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbConnection"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.StateChange -= StateChangeHandler;
                }

                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Dispose();
                }
            }
            _connection = null;
            _getDbProfiler = null;

            base.Dispose(disposing);
        }
		
		 /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return _connection.OpenAsync(cancellationToken);
        }

        /// <summary>
        /// Enlists in the specified transaction. 
        /// </summary>
        /// <param name="transaction"></param>
        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            if (_connection != null)
            {
                _connection.EnlistTransaction(transaction);
            }
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/>. 
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchema()
        {
            if (_connection != null)
            {
                return _connection.GetSchema();
            }

            return null;
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/> using the specified string for the schema name. 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public override DataTable GetSchema(string collectionName)
        {
            if (_connection != null)
            {
                return _connection.GetSchema(collectionName);
            }

            return null;
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/> using the specified string for the schema name and the specified string array for the restriction values. 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="restrictionValues"></param>
        /// <returns></returns>
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            if (_connection != null)
            {
                return _connection.GetSchema(collectionName, restrictionValues);
            }

            return null;
        }
 
        #endregion

        #region Private Methods

        private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArgs)
        {
            OnStateChange(stateChangeEventArgs);
        }

        #endregion
    }
}
