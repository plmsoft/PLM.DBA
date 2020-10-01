using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using PLM.DBA.Common.BO;

using PLM.DBA.Core.Common.Const;

namespace PLM.DBA.Core.Common
{
	public class DBACommand : IDBACommand
	{
		private readonly string _connectionString;

		public DBACommand(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int ExecuteNonQuery(CommandParameter parameters)
		{
			using(var con = new SqlConnection(_connectionString))
			{
				using(var cmd = new SqlCommand(parameters.Command, con))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					if(parameters.Parameters != null)
						cmd.Parameters.AddRange(parameters.Parameters.ToArray());

					con.Open();

					int c = cmd.ExecuteNonQuery();

					if (parameters.Parameters != null)
					{
						var outNewIdParam = parameters.Parameters.FirstOrDefault(d => d.Direction == System.Data.ParameterDirection.Output && d.ParameterName.Equals(Consts.OutputParameter.OutNewIdParam, StringComparison.OrdinalIgnoreCase));
						if (outNewIdParam != null)
							parameters.NewId = outNewIdParam.Value;
					}

					return c;
				}
			}
		}

        public async Task<int> ExecuteNonQueryAsync(CommandParameter parameters)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(parameters.Command, con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (parameters.Parameters != null)
                        cmd.Parameters.AddRange(parameters.Parameters.ToArray());

                    con.Open();

                    int c = await cmd.ExecuteNonQueryAsync();

					if (parameters.Parameters != null)
					{
						var outNewIdParam = parameters.Parameters.FirstOrDefault(d => d.Direction == System.Data.ParameterDirection.Output && d.ParameterName.Equals(Consts.OutputParameter.OutNewIdParam, StringComparison.OrdinalIgnoreCase));
						var outCountParam = parameters.Parameters.FirstOrDefault(d => d.Direction == System.Data.ParameterDirection.Output && d.ParameterName.Equals(Consts.OutputParameter.OutCountParam, StringComparison.OrdinalIgnoreCase));
						parameters.OutParameterValue = parameters.Parameters.Where(d => d.Direction == System.Data.ParameterDirection.Output).Select(d => d.Value).ToArray();

						if (outNewIdParam != null)
							parameters.NewId = outNewIdParam.Value;

						if (outCountParam != null)
							parameters.Count = outCountParam.Value;
					}

					return c;
                }
            }
        }

        public IEnumerable<TOutParamsResult> ExecuteNonQuery<TOutParamsResult>(CommandParameter parameters) where TOutParamsResult : class, new()
		{
			var result = new ConcurrentQueue<TOutParamsResult>();

			using (var con = new SqlConnection(_connectionString))
			{
				using (var cmd = new SqlCommand(parameters.Command, con))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					if (parameters.Parameters != null)
						cmd.Parameters.AddRange(parameters.Parameters.ToArray());

					con.Open();

					cmd.ExecuteNonQuery();

					var outParamItems = parameters.Parameters.Where(d => d.Direction == System.Data.ParameterDirection.Output).ToArray();

					if (outParamItems.Length > 0)
					{
						var props = typeof(TOutParamsResult).GetProperties();
						
						foreach (var outItem in outParamItems)
						{
							var item = new TOutParamsResult();

							var propItem = props.FirstOrDefault(d => d.Name.Equals(outItem.ParameterName, StringComparison.OrdinalIgnoreCase));
							if (propItem != null)
							{
								propItem.SetValue(item, outItem.Value, null);

								result.Enqueue(item);
							}

						}

					}
				}
			}

			return result;
		}

		public IEnumerable<TResult> ExecuteNonQuery<TResult>(CommandParameter parameters, Func<SqlParameterCollection, IEnumerable<TResult>> action) where TResult : class, new()
		{
			using (var con = new SqlConnection(_connectionString))
			{
				using (var cmd = new SqlCommand(parameters.Command, con))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					if (parameters.Parameters != null)
						cmd.Parameters.AddRange(parameters.Parameters.ToArray());

					con.Open();

					cmd.ExecuteNonQuery();
					var result = action(cmd.Parameters);
					return result;
				}
			}
		}

        public IEnumerable<TResult> ExecuteReaderQuery<TResult>(CommandParameter parameters) where TResult : class, new()
		{
			var result = new ConcurrentQueue<TResult>();

			using (var con = new SqlConnection(_connectionString))
			{
				using (var cmd = new SqlCommand(parameters.Command, con))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					if (parameters.Parameters != null)
						cmd.Parameters.AddRange(parameters.Parameters.ToArray());

					con.Open();

					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
					{
                        var props = typeof(TResult).GetProperties();

						while (reader.Read())
						{
							var item = new TResult();

							for(var i=0; i < reader.FieldCount; i++)
							{
								var propItem = props.FirstOrDefault(d => d.Name.Equals(reader.GetName(i), StringComparison.OrdinalIgnoreCase));
								if (propItem != null)
								{
									var propItemType = propItem.PropertyType;

									object value = null;

									if (!reader.IsDBNull(i))
										value = reader.GetValue(reader.GetOrdinal(propItem.Name));
									else
									{
										if (propItemType.IsValueType)
										{
											if (propItemType.Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
												value = DateTime.MinValue;
											else
												if (propItemType.Name.Equals("Decimal", StringComparison.OrdinalIgnoreCase))
													value = new Decimal(0.00);
												else
													if (propItemType.Name.Equals("Double", StringComparison.OrdinalIgnoreCase))
														value = Double.Parse("0.00");
													else
														value = -1;
										}
									}

									propItem.SetValue(item, value, null);
								}
							}

							result.Enqueue(item);
						}

						reader.Close();

                        if (parameters.Parameters != null &&  parameters.Parameters.FirstOrDefault(d => d.Direction == System.Data.ParameterDirection.Output && d.ParameterName.Equals(Consts.OutputParameter.OutCountParam, StringComparison.OrdinalIgnoreCase)) != null)
                        {
                            var outCountParam = cmd.Parameters[Consts.OutputParameter.OutCountParam];
                            if (outCountParam != null)
                                parameters.Count = outCountParam.Value;
                        }
                    }
                }
			}

			return result;
		}

		public IEnumerable<TResult> ExecuteReaderQuery<TResult>(CommandParameter parameters, Func<SqlDataReader, IEnumerable<TResult>> action) where TResult : class, new()
		{
			using (var con = new SqlConnection(_connectionString))
			{
				using (var cmd = new SqlCommand(parameters.Command, con))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					if (parameters.Parameters != null)
						cmd.Parameters.AddRange(parameters.Parameters.ToArray());

					con.Open();

					using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
					{
						var result = action(reader);
						reader.Close();
						return result;
					}
				}
			}
		}

        public async Task<IEnumerable<TResult>> ExecuteReaderQueryAsync<TResult>(CommandParameter parameters) where TResult : class, new()
        {
            var result = new ConcurrentQueue<TResult>();

            using (var con = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(parameters.Command, con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (parameters.Parameters != null)
                        cmd.Parameters.AddRange(parameters.Parameters.ToArray());

                    con.Open();

                    using (var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection))
                    {
                        var props = typeof(TResult).GetProperties();

                        while (await reader.ReadAsync())
                        {
                            var item = new TResult();

                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                var propItem = props.FirstOrDefault(d => d.Name.Equals(reader.GetName(i), StringComparison.OrdinalIgnoreCase));
                                if (propItem != null)
                                {
                                    var propItemType = propItem.PropertyType;

                                    object value = null;

                                    if (!reader.IsDBNull(i))
                                        value = reader.GetValue(reader.GetOrdinal(propItem.Name));
                                    else
                                    {
                                        if (propItemType.IsValueType)
                                        {
                                            if (propItemType.Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                                                value = DateTime.MinValue;
                                            else if (propItemType.Name.Equals("Decimal", StringComparison.OrdinalIgnoreCase))
                                                    value = new Decimal(0.00);
                                            else if (propItemType.Name.Equals("Double", StringComparison.OrdinalIgnoreCase))
                                                    value = Double.Parse("0.00");
                                            else if (propItemType.Name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase))
                                                    value = DateTimeOffset.MinValue;
                                            else if (propItemType.Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                                                value = false;
                                            else if (propItemType.Name.Equals("Bool", StringComparison.OrdinalIgnoreCase))
                                                value = false;
                                            else
                                                value = -1;
                                        }
                                    }

                                    propItem.SetValue(item, value, null);
                                }
                            }

                            result.Enqueue(item);
                        }

                        reader.Close();

                        if (parameters.Parameters != null &&  parameters.Parameters.FirstOrDefault(d => d.Direction == System.Data.ParameterDirection.Output && d.ParameterName.Equals(Consts.OutputParameter.OutCountParam, StringComparison.OrdinalIgnoreCase)) != null)
                        {
                            var outCountParam = cmd.Parameters[Consts.OutputParameter.OutCountParam];
                            if (outCountParam != null)
                                parameters.Count = outCountParam.Value;
                        }
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<TResult>> ExecuteReaderQueryAsync<TResult>(CommandParameter parameters, Func<SqlDataReader, Task<IEnumerable<TResult>>> action) where TResult : class, new()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(parameters.Command, con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

					if (parameters.Parameters != null)
                        cmd.Parameters.AddRange(parameters.Parameters.ToArray());

                    con.Open();

                    using (var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection))
                    {
                        var result = await action(reader);
                        reader.Close();

                        if (parameters.Parameters != null && parameters.Parameters.FirstOrDefault(d => d.Direction == System.Data.ParameterDirection.Output && d.ParameterName.Equals(Consts.OutputParameter.OutCountParam, StringComparison.OrdinalIgnoreCase)) != null)
                        {
                            var outCountParam = cmd.Parameters[Consts.OutputParameter.OutCountParam];
                            if (outCountParam != null)
                                parameters.Count = outCountParam.Value;
                        }

                        return result;
                    }
                }
            }
        }
    }
}
