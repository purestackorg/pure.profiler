
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pure.Profiler.Data
{
    /// <summary>
    /// Represents a profiled <see cref="IDataReader"/>
    /// which will call <see cref="IDbProfiler"/>.DataReaderFinished() automatically
    /// when the data reader is closed.
    /// </summary>
    public class ProfiledDbDataReader : DbDataReader
    {
        public string Id { get; private set; }
        //private readonly IDataReader _dataReader;
        //private readonly DbDataReader _dataReader;
        private readonly DbDataReader _dataReader;
        private readonly IDbProfiler _dbProfiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbDataReader"/>.
        /// </summary>
        /// <param name="dataReader">The <see cref="DbDataReader"/> to be profiled.</param>
        /// <param name="dbProfiler">
        ///     The <see cref="IDbProfiler"/> which profiles the <see cref="DbDataReader"/>
        /// </param>
        public ProfiledDbDataReader(IDataReader dataReader, IDbProfiler dbProfiler)
        {
            if (dataReader == null)
            {
                throw new ArgumentNullException("dataReader");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }
            Id = Guid.NewGuid().ToString();
            //_dataReader = dataReader;
           // _dataReader = dataReader;
            _dataReader = dataReader as DbDataReader ;
              
            _dbProfiler = dbProfiler;
        }

        #endregion

        #region Override Equals so that ProfiledDataReader could checks Equals by its dataReader

        /// <summary>
        /// Gets hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _dataReader.GetHashCode();
        }

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            var profilingDataReader = obj as ProfiledDbDataReader;
            if (!ReferenceEquals(profilingDataReader, null))
            {
                return ReferenceEquals(profilingDataReader._dataReader, _dataReader);
            }

            var dataReader = obj as DbDataReader;
            if (dataReader != null)
            {
                return ReferenceEquals(dataReader, _dataReader);
            }

            return false;
        }

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>Return true if equals.</returns>
        public static bool operator ==(ProfiledDbDataReader a, ProfiledDbDataReader b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null)
                || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>Return true if equals.</returns>
        public static bool operator ==(ProfiledDbDataReader a, DbDataReader b)
        {
            if (ReferenceEquals(a, null)
                || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>Return true if equals.</returns>
        public static bool operator ==(DbDataReader a, ProfiledDbDataReader b)
        {
            if (ReferenceEquals(a, null)
                || ReferenceEquals(b, null))
            {
                return false;
            }

            return b.Equals(a);
        }

        /// <summary>
        /// Not equals.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>Return true if not equals.</returns>
        public static bool operator !=(ProfiledDbDataReader a, ProfiledDbDataReader b)
        {
            if (ReferenceEquals(a, b))
            {
                return false;
            }

            if ((ReferenceEquals(a, null)
                || ReferenceEquals(b, null)))
            {
                return true;
            }

            return !a.Equals(b);
        }

        /// <summary>
        /// Not equals.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>Return true if not equals.</returns>
        public static bool operator !=(ProfiledDbDataReader a, DbDataReader b)
        {
            if ((ReferenceEquals(a, null)
                || ReferenceEquals(b, null)))
            {
                return true;
            }

            return !a.Equals(b);
        }

        /// <summary>
        /// Not equals.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>Return true if not equals.</returns>
        public static bool operator !=(DbDataReader a, ProfiledDbDataReader b)
        {
            if ((ReferenceEquals(a, null)
                || ReferenceEquals(b, null)))
            {
                return true;
            }

            return !b.Equals(a);
        }

        #endregion

        #region DbDataReader Members

       

        /// <summary>
        /// Gets the depth.
        /// </summary>
        public override int Depth
        {
            get { return _dataReader.Depth; }
        }

        /// <summary>
        /// Gets the field count.
        /// </summary>
        public override int FieldCount
        {
            get { return _dataReader.FieldCount; }
        }

        /// <summary>
        /// Gets boolean.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override bool GetBoolean(int ordinal)
        {
            return _dataReader.GetBoolean(ordinal);
        }

        /// <summary>
        /// Gets byte.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override byte GetByte(int ordinal)
        {
            return _dataReader.GetByte(ordinal);
        }

        /// <summary>
        /// Gets bytes.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return _dataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Gets char.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override char GetChar(int ordinal)
        {
            return _dataReader.GetChar(ordinal);
        }

        /// <summary>
        /// Gets chars.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return _dataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Gets the name of data type.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override string GetDataTypeName(int ordinal)
        {
            return _dataReader.GetDataTypeName(ordinal);
        }

        /// <summary>
        /// Gets datetime.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override DateTime GetDateTime(int ordinal)
        {
            return _dataReader.GetDateTime(ordinal);
        }

        /// <summary>
        /// Gets decimal.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override decimal GetDecimal(int ordinal)
        {
            return _dataReader.GetDecimal(ordinal);
        }

        /// <summary>
        /// Gets double.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override double GetDouble(int ordinal)
        {
            return _dataReader.GetDouble(ordinal);
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public override System.Collections.IEnumerator GetEnumerator()
        {
            if (_dataReader != null)
            {
                return _dataReader.GetEnumerator();
            }

            return null;// new DbEnumerator(_dataReader, true);
        }

        /// <summary>
        /// Gets the type of field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override Type GetFieldType(int ordinal)
        {
            return _dataReader.GetFieldType(ordinal);
        }

        /// <summary>
        /// Gets float.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override float GetFloat(int ordinal)
        {
            return _dataReader.GetFloat(ordinal);
        }

        /// <summary>
        /// Gets guid.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override Guid GetGuid(int ordinal)
        {
            return _dataReader.GetGuid(ordinal);
        }

        /// <summary>
        /// Gets short.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override short GetInt16(int ordinal)
        {
            return _dataReader.GetInt16(ordinal);
        }

        /// <summary>
        /// Gets int.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override int GetInt32(int ordinal)
        {
            return _dataReader.GetInt32(ordinal);
        }

        /// <summary>
        /// Gets long.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override long GetInt64(int ordinal)
        {
            return _dataReader.GetInt64(ordinal);
        }

        /// <summary>
        /// Gets name of field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override string GetName(int ordinal)
        {
            return _dataReader.GetName(ordinal);
        }

        /// <summary>
        /// Gets ordinal by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override int GetOrdinal(string name)
        {
            return _dataReader.GetOrdinal(name);
        }

        /// <summary>
        /// Gets schema table.
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTable()
        {
            return _dataReader.GetSchemaTable();
        }

        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return _dataReader != null ? _dataReader.GetProviderSpecificFieldType(ordinal) : null;
        }

        public override object GetProviderSpecificValue(int ordinal)
        {
            return _dataReader != null ? _dataReader.GetProviderSpecificValue(ordinal) : 0;
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return _dataReader != null ? _dataReader.GetProviderSpecificValues(values) : 0;
        }

        /// <summary>
        /// Gets string.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override string GetString(int ordinal)
        {
            return _dataReader.GetString(ordinal);
        }

        /// <summary>
        /// Gets value.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override object GetValue(int ordinal)
        {
            return _dataReader.GetValue(ordinal);
        }

        /// <summary>
        /// Gets values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public override int GetValues(object[] values)
        {
            return _dataReader.GetValues(values);
        }

        /// <summary>
        /// Has rows.
        /// </summary>
        public override bool HasRows
        {
            get
            {
                if (_dataReader != null)
                {
                    return _dataReader.HasRows;
                }

                return true;
            }
        }

        /// <summary>
        /// Whether or not the reader is closed.
        /// </summary>
        public override bool IsClosed
        {
            get { return _dataReader.IsClosed; }
        }

        /// <summary>
        /// Whether or not is db null.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override bool IsDBNull(int ordinal)
        {
            return _dataReader.IsDBNull(ordinal);
        }

        /// <summary>
        /// Whether or not has next result.
        /// </summary>
        /// <returns></returns>
        public override bool NextResult()
        {
            return _dataReader.NextResult();
        }
        public int RowCount { get; set; }
        public override bool Read()
        {
            var flag = _dataReader.Read();
            if (flag)
            {
                RowCount++;
            }

            return flag;
        }
        /// <summary>
        /// Whether or not has next row.
        /// </summary>
        /// <returns></returns>
        //public override bool Read()
        //{
        //    return _dataReader.Read();
        //}

        /// <summary>
        /// Gets # of records affected.
        /// </summary>
        public override int RecordsAffected
        {
            get { return _dataReader.RecordsAffected; }
        }
        public override int VisibleFieldCount
        {
            get { return _dataReader != null? _dataReader.VisibleFieldCount : 0; }
        }

        /// <summary>
        /// Gets value by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object this[string name]
        {
            get { return _dataReader[name]; }
        }

        /// <summary>
        /// Gets value by ordinal.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override object this[int ordinal]
        {
            get { return _dataReader[ordinal]; }
        }


        /// <summary>
        /// Gets field value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override T GetFieldValue<T>(int ordinal)
        {
            return _dataReader.GetFieldValue<T>(ordinal);
        }

        /// <summary>
        /// Gets field value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return _dataReader.GetFieldValueAsync<T>(ordinal, cancellationToken);
        }

      

        /// <summary>
        /// Gets stream.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override Stream GetStream(int ordinal)
        {
            return _dataReader.GetStream(ordinal);
        }

        /// <summary>
        /// Gets text reader.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override TextReader GetTextReader(int ordinal)
        {
            return _dataReader.GetTextReader(ordinal);
        }

        /// <summary>
        /// Is DBNull.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return _dataReader.IsDBNullAsync(ordinal, cancellationToken);
        }

        /// <summary>
        /// Gets next result.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return _dataReader.NextResultAsync(cancellationToken);
        }

        /// <summary>
        /// Reads next row.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return _dataReader.ReadAsync(cancellationToken);
        }


        //public override void Close()
        //{
        //    // reader can be null when we're not profiling, but we've inherited from ProfiledDbCommand and are returning a
        //    // an unwrapped reader from the base command
        //    if (_dataReader != null)
        //        _dataReader.Close();

        //    if (_dbProfiler != null)
        //    {
        //        _dbProfiler.DataReaderFinished(this);
        //    }

        //    base.Close();

        //}
        /// <summary>
        /// Closes the reader.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (_dataReader != null)
                    _dataReader.Close();

                if (_dbProfiler != null)
                {
                    _dbProfiler.DataReaderFinished(this);
                }
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
