
using System;
using System.Data;
using System.Data.Common;

namespace Pure.Profiler.Data
{
    /// <summary>
    /// A <see cref="IDbDataAdapter"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbDataAdapter : DbDataAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes an <see cref="ProfiledDbDataAdapter"/>.
        /// </summary>
        /// <param name="dataAdapter">The <see cref="IDbDataAdapter"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbDataAdapter(DbDataAdapter dataAdapter, IDbProfiler dbProfiler)
        {
            if (dataAdapter == null)
            {
                throw new ArgumentNullException("dataAdapter");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }
            
            if (dataAdapter.SelectCommand != null)
            {
                var profiledSelectCommand = dataAdapter.SelectCommand as ProfiledDbCommand;
                if (profiledSelectCommand != null)
                {
                    SelectCommand = profiledSelectCommand;
                }
                else
                {
                    SelectCommand = new ProfiledDbCommand(dataAdapter.SelectCommand, dbProfiler);
                }
            }

            if (dataAdapter.InsertCommand != null)
            {
                var profiledInsertCommand = dataAdapter.InsertCommand as ProfiledDbCommand;
                if (profiledInsertCommand != null)
                {
                    InsertCommand = profiledInsertCommand;
                }
                else
                {
                    InsertCommand = new ProfiledDbCommand(dataAdapter.InsertCommand, dbProfiler);
                }
            }

            if (dataAdapter.UpdateCommand != null)
            {
                var profiledUpdateCommand = dataAdapter.UpdateCommand as ProfiledDbCommand;
                if (profiledUpdateCommand != null)
                {
                    UpdateCommand = profiledUpdateCommand;
                }
                else
                {
                    UpdateCommand = new ProfiledDbCommand(dataAdapter.UpdateCommand, dbProfiler);
                }
            }

            if (dataAdapter.DeleteCommand != null)
            {
                var profiledDeleteCommand = dataAdapter.DeleteCommand as ProfiledDbCommand;
                if (profiledDeleteCommand != null)
                {
                    DeleteCommand = profiledDeleteCommand;
                }
                else
                {
                    DeleteCommand = new ProfiledDbCommand(dataAdapter.DeleteCommand, dbProfiler);
                }
            }
        }

        #endregion
    }
}
