#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sonata.ComponentModel.DataAnnotations;
using Sonata.Data.Entity.Mapping;
using Sonata.Data.Extensions;

namespace Sonata.Data.Entity
{
	public static class DbContextExtension
	{
		/// <summary>
		/// Executes the specified <paramref name="storedProcedure"/>, passing in a list of input parameters.
		/// </summary>
		/// <param name="instance">Database Context to use for the call.</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="storedProcedure">The <see cref="StoredProcedure"/> to execute.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL or <paramref name="storedProcedure"/> is NULL.</exception>
		public static ResultSets CallStoredProcedure(this DbContext instance, StoredProcedure storedProcedure, int? commandTimeout, IEnumerable<DbParameter> inputParameters = null)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (storedProcedure == null)
				throw new ArgumentNullException(nameof(storedProcedure));

			var results = instance.ReadFromStoredProcedure(storedProcedure.Fullname, inputParameters, commandTimeout, storedProcedure.ReturnedTypes.ToArray());
			return results ?? new ResultSets();
		}

		/// <summary>
		/// Executes the specified <paramref name="storedProcedure"/>, passing in a list of input parameters.
		/// </summary>
		/// <param name="instance">Database Context to use for the call.</param>
		/// <param name="storedProcedure">The <see cref="StoredProcedure"/> to execute.</param>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL or <paramref name="storedProcedure"/> is NULL.</exception>
		public static async Task<ResultSets> CallStoredProcedureAsync(this DbContext instance, StoredProcedure storedProcedure, CancellationToken token, int? commandTimeout, IEnumerable<DbParameter> inputParameters = null)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (storedProcedure == null)
				throw new ArgumentNullException(nameof(storedProcedure));

			var results = await instance.ReadFromStoredProcedureAsync(storedProcedure.Fullname, token, inputParameters, commandTimeout, storedProcedure.ReturnedTypes.ToArray());
			return results ?? new ResultSets();
		}

		/// <summary>
		/// Executes the specified <paramref name="storedProcedure"/>.
		/// </summary>
		/// <typeparam name="T">Type of object containing the input parameters data.</typeparam>
		/// <param name="instance">Database Context to use for the call.</param>
		/// <param name="storedProcedure">The <see cref="StoredProcedure{T}"/> to execute.</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL or <paramref name="storedProcedure"/> is NULL.</exception>
		public static ResultSets CallStoredProcedure<T>(this DbContext instance, StoredProcedure<T> storedProcedure, int? commandTimeout, T inputParameters)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (storedProcedure == null)
				throw new ArgumentNullException(nameof(storedProcedure));

			var sqlInputParameters = storedProcedure.Parameters(inputParameters).ToList();
			var results = instance.ReadFromStoredProcedure(storedProcedure.Fullname, sqlInputParameters, commandTimeout, storedProcedure.ReturnedTypes.ToArray());
			storedProcedure.ProcessOutputParms(sqlInputParameters, inputParameters);

			return results ?? new ResultSets();
		}

		/// <summary>
		/// Executes the specified <paramref name="storedProcedure"/>.
		/// </summary>
		/// <typeparam name="T">Type of object containing the input parameters data.</typeparam>
		/// <param name="instance">Database Context to use for the call.</param>
		/// <param name="storedProcedure">The <see cref="StoredProcedure{T}"/> to execute.</param>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL or <paramref name="storedProcedure"/> is NULL.</exception>
		public static async Task<ResultSets> CallStoredProcedureAsync<T>(this DbContext instance, StoredProcedure<T> storedProcedure, CancellationToken token, int? commandTimeout)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (storedProcedure == null)
				throw new ArgumentNullException(nameof(storedProcedure));

			var results = await instance.ReadFromStoredProcedureAsync(storedProcedure.Fullname, token, null, commandTimeout, storedProcedure.ReturnedTypes.ToArray());
			return results ?? new ResultSets();
		}

		/// <summary>
		/// Executes the specified <paramref name="storedProcedure"/>.
		/// </summary>
		/// <typeparam name="T">Type of object containing the input parameters data.</typeparam>
		/// <param name="instance">Database Context to use for the call.</param>
		/// <param name="storedProcedure">The <see cref="StoredProcedure{T}"/> to execute.</param>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL or <paramref name="storedProcedure"/> is NULL.</exception>
		public static async Task<ResultSets> CallStoredProcedureAsync<T>(this DbContext instance, StoredProcedure<T> storedProcedure, CancellationToken token, int? commandTimeout, T inputParameters)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (storedProcedure == null)
				throw new ArgumentNullException(nameof(storedProcedure));

			var sqlInputParameters = storedProcedure.Parameters(inputParameters).ToList();
			var results = await instance.ReadFromStoredProcedureAsync(storedProcedure.Fullname, token, sqlInputParameters, commandTimeout, storedProcedure.ReturnedTypes.ToArray());
			storedProcedure.ProcessOutputParms(sqlInputParameters, inputParameters);

			return results ?? new ResultSets();
		}

		/// <summary>
		/// Locates and initializes all the stored procedure properties in this <see cref="DbContext"/>. This should be called in the <see cref="DbContext"/> constructor.
		/// </summary>
		/// <param name="instance">The <see cref="DbContext"/> containing stored procedure to initialize.</param>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL.</exception>
		public static void InitializeStoredProcedures(this DbContext instance)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));

			var contextType = instance.GetType();
			foreach (var storedProcedure in contextType.GetProperties().Where(p => p.PropertyType.Name.Contains("StoredProcedure")))
			{
				//	Create StoredProcedure object and save it in DbContext property
				var constructorInfo = storedProcedure.PropertyType.GetConstructor(new[] { contextType });
				if (constructorInfo == null)
					continue;

				var m = constructorInfo.Invoke(new object[] { instance });
				storedProcedure.SetValue(instance, m);

				//	See if there is a StoredProcedureAttribute on this property to get a possible Name and/or ReturnTypes
				if (storedProcedure.GetCustomAttributes(typeof(StoredProcedureAttribute), false).FirstOrDefault() is StoredProcedureAttribute storedProcedureAttribute)
				{
					if (!String.IsNullOrWhiteSpace(storedProcedureAttribute.Name))
						((StoredProcedure)m).HasName(storedProcedureAttribute.Name);

					if (storedProcedureAttribute.ReturnTypes != null)
						((StoredProcedure)m).ReturnsTypes(storedProcedureAttribute.ReturnTypes);
				}

				//	See if there is a Schema attribute on this property
				if (storedProcedure.GetCustomAttributes(typeof(UserDefinedTableTypeAttribute), false).FirstOrDefault() is UserDefinedTableTypeAttribute userDefinedTableTypeAttribute && !String.IsNullOrWhiteSpace(userDefinedTableTypeAttribute.Schema))
					((StoredProcedure)m).HasSchema(userDefinedTableTypeAttribute.Schema);
			}
		}

		/// <summary>
		/// Call a stored procedure and get the results back. 
		/// </summary>
		/// <param name="instance">Database Context to use for the call.</param>
		/// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL or <paramref name="storedProcedureName"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="storedProcedureName"/> is empty or whitespace.</exception>
		/// <exception cref="Exception">An <see cref="Exception"/> occured while reading the result from the stored procedure.</exception>
		internal static ResultSets ReadFromStoredProcedure(this DbContext instance, string storedProcedureName, IEnumerable<DbParameter> inputParameters = null, int? commandTimeout = null, params Type[] returnedTypes)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (storedProcedureName == null)
				throw new ArgumentNullException(nameof(storedProcedureName));
			if (String.IsNullOrWhiteSpace(storedProcedureName))
				throw new ArgumentException(nameof(storedProcedureName));

			var isConnectionOpen = false;
			var results = new ResultSets();
			var currentType = (returnedTypes == null) ? new Type[0].GetEnumerator() : returnedTypes.GetEnumerator();
			var connection = instance.Database.GetDbConnection();

			try
			{
				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
					isConnectionOpen = true;
				}

				using (var sqlCommand = connection.CreateCommand())
				{
					sqlCommand.CommandText = storedProcedureName;
					sqlCommand.CommandType = CommandType.StoredProcedure;
					sqlCommand.CommandTimeout = commandTimeout ?? sqlCommand.CommandTimeout;

					//	Add input parameters
					if (inputParameters != null)
					{
						foreach (var inputParameter in inputParameters)
							sqlCommand.Parameters.Add(inputParameter);
					}

					//	Execute the stored procedure
					var sqlDataReader = sqlCommand.ExecuteReader();

					//	Get the type we're expecting for the first result. If no types specified, ignore all results.
					if (currentType.MoveNext())
					{
						#region Process results

						//	Repeat this loop for each result set returned by the stored procedure for which we have a result type specified.
						do
						{
							var mappedProperties = ((Type)currentType.Current).GetMappedProperties().ToList();	//	Get properties to save for the current destination type.
							var currentResult = new List<object>();												//	Create a destination for our results.

							while (sqlDataReader.Read())
							{
								//	Create an object to hold this result.
								var constructorInfo = ((Type)currentType.Current).GetConstructor(Type.EmptyTypes);
								if (constructorInfo == null)
									continue;

								var item = constructorInfo.Invoke(new object[0]);
								sqlDataReader.ReadRecord(item, mappedProperties.ToArray());			//	Copy data elements by parameter name from result to destination object.
								currentResult.Add(item);											//	Add newly populated item to our output list.
							}

							//	Add this result set to our return list.
							results.Add(currentResult);
						}
						while (sqlDataReader.NextResult() && currentType.MoveNext());

						#endregion
					}

					//	If we opened the reader, then close up the reader, we're done saving results.
					if (isConnectionOpen)
						sqlDataReader.Close();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("Error reading from stored procedure {0}: {1}", storedProcedureName, ex.Message), ex);
			}
			finally
			{
				connection.Close();
			}

			return results;
		}

		/// <summary>
		/// Call a stored procedure and get the results back. 
		/// </summary>
		/// <param name="instance">Database Context to use for the call.</param>
		/// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="instance"/> is NULL or <paramref name="storedProcedureName"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="storedProcedureName"/> is empty or whitespace.</exception>
		/// <exception cref="Exception">An <see cref="Exception"/> occured while reading the result from the stored procedure.</exception>
		internal static async Task<ResultSets> ReadFromStoredProcedureAsync(this DbContext instance, string storedProcedureName, CancellationToken token, IEnumerable<DbParameter> inputParameters = null, int? commandTimeout = null, params Type[] returnedTypes)
		{
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (storedProcedureName == null)
				throw new ArgumentNullException(nameof(storedProcedureName));
			if (String.IsNullOrWhiteSpace(storedProcedureName))
				throw new ArgumentException(nameof(storedProcedureName));

			var isConnectionOpen = false;
			var results = new ResultSets();
			var currentType = (returnedTypes == null) ? new Type[0].GetEnumerator() : returnedTypes.GetEnumerator();
			var connection = instance.Database.GetDbConnection();

			try
			{
				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
					isConnectionOpen = true;
				}

				using (var sqlCommand = connection.CreateCommand())
				{
					sqlCommand.CommandText = storedProcedureName;
					sqlCommand.CommandType = CommandType.StoredProcedure;
					sqlCommand.CommandTimeout = commandTimeout ?? sqlCommand.CommandTimeout;

					//	Add input parameters
					if (inputParameters != null)
					{
						foreach (var inputParameter in inputParameters)
							sqlCommand.Parameters.Add(inputParameter);
					}

					//	Execute the stored procedure
					var sqlDataReader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess, token);

					//	Get the type we're expecting for the first result. If no types specified, ignore all results.
					if (currentType.MoveNext())
					{
						#region Process results

						//	Repeat this loop for each result set returned by the stored procedure for which we have a result type specified.
						do
						{
							var mappedProperties = ((Type)currentType.Current).GetMappedProperties().ToList();	//	Get properties to save for the current destination type.
							var currentResult = new List<object>();												//	Create a destination for our results.

							while (sqlDataReader.Read())
							{
								//	Create an object to hold this result.
								var constructorInfo = ((Type)currentType.Current).GetConstructor(Type.EmptyTypes);
								if (constructorInfo == null)
									continue;

								var item = constructorInfo.Invoke(new object[0]);
								sqlDataReader.ReadRecord(item, mappedProperties.ToArray());			//	Copy data elements by parameter name from result to destination object.
								currentResult.Add(item);											//	Add newly populated item to our output list.
							}

							//	Add this result set to our return list.
							results.Add(currentResult);
						}
						while (sqlDataReader.NextResult() && currentType.MoveNext());

						#endregion
					}

					//	If we opened the reader, then close up the reader, we're done saving results.
					if (isConnectionOpen)
						sqlDataReader.Close();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("Error reading from stored procedure {0}: {1}", storedProcedureName, ex.Message), ex);
			}
			finally
			{
				connection.Close();
			}

			return results;
		}
	}
}