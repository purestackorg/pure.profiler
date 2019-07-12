
using System.Data;
using System.Data.Common;

namespace Pure.Profiler.Data
{
    /// <summary>
    /// A wrapper of <see cref="IDbTransaction"/> which supports DB profiling.
    /// </summary>
    internal class ProfiledDbTransaction : DbTransaction
    {
        private  DbTransaction _transaction;
        private DbConnection _dbConnection;

        /// <summary>
        /// Gets the wrapped <see cref="DbTransaction"/>
        public DbTransaction WrappedTransaction { get { return _transaction; } }

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to be profiled.</param>
        /// <param name="connection">The <see cref="DbConnection"/>.</param>
        public ProfiledDbTransaction(DbTransaction transaction, DbConnection connection)
        {
            _transaction = transaction;
            _dbConnection = connection ?? transaction.Connection;
        }

        #endregion

        #region DbTransaction Members

        /// <summary>
        /// Commits the database transaction. 
        /// </summary>
        public override void Commit()
        {
            _transaction.Commit();
        }

        /// <summary>
        /// Returns the <see cref="DbConnection"/> object associated with the transaction. 
        /// </summary>
        protected override DbConnection DbConnection
        {
            get { return _dbConnection; }
        }

        /// <summary>
        /// Returns <see cref="IsolationLevel"/> for this transaction. 
        /// </summary>
        public override IsolationLevel IsolationLevel
        {
            get { return _transaction.IsolationLevel; }
        }

        /// <summary>
        /// Rolls back a transaction from a pending state. 
        /// </summary>
        public override void Rollback()
        {
            _transaction.Rollback();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbTransaction"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
           
            if (disposing && _transaction != null)
            {
                _transaction.Dispose();
            }
            _transaction = null;
            if (Configuration.ConfigurationHelper.LoadPureProfilerConfigurationSection().EnableDbPool == false)
            {

                _dbConnection = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }

}
