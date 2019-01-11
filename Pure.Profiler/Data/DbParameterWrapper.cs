

using System.Data;
using System.Data.Common;

namespace Pure.Profiler.Data
{
    internal sealed class DbParameterWrapper : DbParameter
    {
        private readonly IDbDataParameter _parameter;
        private readonly DbParameter _dbParameter;

        #region Constructors

        public DbParameterWrapper(IDbDataParameter parameter)
        {
            _parameter = parameter;
            _dbParameter = parameter as DbParameter;
        }

        #endregion

        #region DbParameter Members

        public override DbType DbType
        {
            get
            {
                return _parameter.DbType;
            }
            set
            {
                _parameter.DbType = value;
            }
        }

        public override ParameterDirection Direction
        {
            get
            {
                return _parameter.Direction;
            }
            set
            {
                _parameter.Direction = value;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return _parameter.IsNullable;
            }
            set
            {
                if (_dbParameter != null)
                {
                    _dbParameter.IsNullable = value;
                }
            }
        }

        public override string ParameterName
        {
            get
            {
                return _parameter.ParameterName;
            }
            set
            {
                _parameter.ParameterName = value;
            }
        }

        public override void ResetDbType()
        {
            if (_dbParameter != null)
            {
                _dbParameter.ResetDbType();
            }
        }

        public override int Size
        {
            get
            {
                return _parameter.Size;
            }
            set
            {
                _parameter.Size = value;
            }
        }

        public override string SourceColumn
        {
            get
            {
                return _parameter.SourceColumn;
            }
            set
            {
                _parameter.SourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                if (_dbParameter != null)
                {
                    return _dbParameter.SourceColumnNullMapping;
                }

                return false;
            }
            set
            {
                if (_dbParameter != null)
                {
                    _dbParameter.SourceColumnNullMapping = value;
                }
            }
        }

        public override DataRowVersion SourceVersion
        {
            get
            {
                return _dbParameter.SourceVersion;
            }
            set
            {
                _dbParameter.SourceVersion = value;
            }
        }

        public override object Value
        {
            get
            {
                return _dbParameter.Value;
            }
            set
            {
                _dbParameter.Value = value;
            }
        }

        #endregion
    }
}
