using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

using PLM.DBA.Common.BO;

namespace PLM.DBA.Core.Common
{
	public interface IDBACommand
	{
		IEnumerable<TResult> ExecuteReaderQuery<TResult>(CommandParameter parameters) where TResult : class, new();
        Task<IEnumerable<TResult>> ExecuteReaderQueryAsync<TResult>(CommandParameter parameters) where TResult : class, new();
        Task<IEnumerable<TResult>> ExecuteReaderQueryAsync<TResult>(CommandParameter parameters, Func<SqlDataReader, Task<IEnumerable<TResult>>> action) where TResult : class, new();
        IEnumerable<TResult> ExecuteReaderQuery<TResult>(CommandParameter parameters, Func<SqlDataReader, IEnumerable<TResult>> action) where TResult : class, new();
		int ExecuteNonQuery(CommandParameter parameters);
        Task<int> ExecuteNonQueryAsync(CommandParameter parameters);
        IEnumerable<TOutParamsResult> ExecuteNonQuery<TOutParamsResult>(CommandParameter parameters) where TOutParamsResult : class, new();
		IEnumerable<TResult> ExecuteNonQuery<TResult>(CommandParameter parameters, Func<SqlParameterCollection, IEnumerable<TResult>> action) where TResult : class, new();
	}
}
