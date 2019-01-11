

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using Pure.Profiler.Timings;
using System.Linq;
namespace Pure.Profiler.Data
{
    /// <summary>
    /// Represents a DB timing of a <see cref="IDbCommand"/> execution.
    /// </summary>
    public sealed class DbTiming : Timing
    {
        private readonly IProfiler _profiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="DbTiming"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the timing to when stops.
        /// </param>
        /// <param name="executeType">The <see cref="DbExecuteType"/> of the <see cref="IDbCommand"/> being executed &amp; profiled.</param>
        /// <param name="command">The <see cref="IDbCommand"/> being executed &amp; profiled.</param>
        public DbTiming(
            IProfiler profiler, DbExecuteType executeType, IDbCommand command, object result)
            : base(profiler, "db", ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId, command == null ? null : command.CommandText, null)
        {
            if (profiler == null) throw new ArgumentNullException("profiler");
            if (command == null) throw new ArgumentNullException("command");

            _profiler = profiler;
            StartMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds;
            Sort = profiler.Elapsed.Ticks;
            Data = new Dictionary<string, string>();
            string rawSql = command.CommandText;
            Data["executeType"] = executeType.ToString().ToLowerInvariant();
            Data["executeResult"] = result != null ? result.ToString() : "";
            if (command.Parameters == null || command.Parameters.Count == 0)
            {
                Data["rawSQL"] = rawSql;
                return; 
            }

            Data["parameters"] = SerializeParameters(command.Parameters);
            Data["rawSQL"] = FormatToRawSQL(rawSql, command.Parameters);
           
            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the first fetch milliseconds of this DB operation.
        /// </summary>
        public void FirstFetch()
        {
            Data["readStart"] = ((long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Stops the current DB timing.
        /// </summary>
        public void Stop()
        {
            DurationMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds;
            if (!Data.ContainsKey("readStart"))
            {
                Data["readStart"] = DurationMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            _profiler.GetTimingSession().AddTiming(this);
        }

        #endregion

        #region Private Methods
        private static string GetPrefixChar()
        {
            string dbType = ProfilingSession.Configuration.PureProfilerConfiguration.DbType;
            string result = "";
            if (dbType.IndexOf("sqlserver", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "@";
            }
            else if (dbType.IndexOf("sqlce", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "@";
            }
            else if (dbType.IndexOf("oracle", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = ":";
            }
            else if (dbType.IndexOf("sqlite", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "@";
            }
            else if (dbType.IndexOf("mysql", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "@";
            }
            return result;
        }
        private static string FormatDateTime(string value)
        {
            string dbType = ProfilingSession.Configuration.PureProfilerConfiguration.DbType;
            string result = "";
            if (dbType.IndexOf("sqlserver", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "'" + value + "'";
            }
            else if (dbType.IndexOf("sqlce", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "'" + value + "'";
            }
            else if (dbType.IndexOf("oracle", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "to_date('" + value + "','yyyy-MM-dd HH24:mi:ss') ";
            }
            else if (dbType.IndexOf("sqlite", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "'" + value + "'";
            }
            else if (dbType.IndexOf("mysql", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                result = "'" + value + "'";
            }
            return result;
        }

        private static string FormatBool(string value)
        {
            string dbType = ProfilingSession.Configuration.PureProfilerConfiguration.DbType;
            string result = "";
            if (value == true.ToString())
            {
                result = "1";

            }
            else
            {
                result = "0";
            }
             
            return result;
        }
        private static string FormatToRawSQL(  string rawSql, IDataParameterCollection parameters)
        { 
            DbType[] STRINGDBTYPE = {DbType.AnsiString, DbType.Guid, DbType.String, DbType.AnsiStringFixedLength, DbType.StringFixedLength, 
                                        DbType.Object, DbType.Xml  };
            DbType[] DATEBTYPE = { DbType.Date, DbType.DateTime, DbType.DateTime2, DbType.Time, DbType.DateTimeOffset };
            DbType[] BOOLTYPE = { DbType.Boolean };
            string prefix = GetPrefixChar();
            string formatValue = "";
            foreach (IDataParameter parameter in parameters)
            {
                formatValue = parameter.Value == null || parameter.Value == DBNull.Value ? "NULL" : SerializeParameterValue(parameter.Value);
                if (!(parameter.Value == null || parameter.Value == DBNull.Value))
	{
                    if (STRINGDBTYPE.Any(p=>p == parameter.DbType))
                {
                    formatValue = "'" + formatValue + "'";
                }
                else if (DATEBTYPE.Any(p => p == parameter.DbType))
                {
                    formatValue = FormatDateTime(formatValue);
                }
                    else if (BOOLTYPE.Any(p => p == parameter.DbType))
                    {
                        formatValue = FormatBool(formatValue);
                    }
                else
                {
                    
                }
	}

                rawSql = rawSql.Replace(prefix + parameter.ParameterName, formatValue);
                
            }

            return rawSql;
        }
        private static string SerializeParameters(IDataParameterCollection parameters)
        {
            var sb = new StringBuilder();

            foreach (IDataParameter parameter in parameters)
            {
                sb.Append(parameter.ParameterName);
                sb.Append("(");
                sb.Append(parameter.DbType.ToString());
                sb.Append(", ");
                sb.Append(parameter.Direction.ToString());
                if (parameter.IsNullable)
                {
                    sb.Append(", nullable");
                }
                sb.Append("): ");
                sb.Append(parameter.Value == null || parameter.Value == DBNull.Value ? "NULL" : SerializeParameterValue(parameter.Value));
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        private static string SerializeParameterValue(object value)
        {
            var table = value as DataTable;
            if (table == null)
            {
                return value.ToString();
            }

            if (string.IsNullOrEmpty(table.TableName))
            {
                table.TableName = "Table"; // ensure table name to avoid serialization error
            }

            // for DataTable, serialize and only take top 500 content
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                table.WriteXml(sw);
            }

            var text = sb.ToString();
            return text.Length > 500 ? text.Substring(0, 497) + "..." : text;
        }

        #endregion
    }
}
