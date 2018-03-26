#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using Microsoft.EntityFrameworkCore;
using Sonata.ComponentModel.DataAnnotations;
using Sonata.Core.Extensions;
using Sonata.Data.Entity.Mapping;
using Sonata.Data.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sonata.Data.Entity
{
	/// <summary>
	/// Represents a Stored Procedure in the database.
	/// </summary>
	/// <remarks>The return type objects must have a default constructor.</remarks>
	public class StoredProcedure
	{
		#region Constants

		public const string DefaultSchema = "dbo";

		#endregion

		#region Members

		private string _name;
		private string _schema;

		/// <summary>
		/// The list of data types that this stored procedure returns as result sets. Order is important.
		/// </summary>
		internal List<Type> ReturnedTypes = new List<Type>();

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the table the stored procedure.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="value"/> is empty or whitespace.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public string Name
		{
			get => _name;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be empty or whitespace.", "value");

				_name = value;
			}
		}

		/// <summary>
		/// Gets or sets the schema of the stored procedure.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="value"/> is empty or whitespace.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public string Schema
		{
			get => _schema;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("value can not be empty or whitespace.", "value");

				_schema = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="DbContext"/> toward which all database queries will be run.
		/// </summary>
		internal DbContext DbContext { get; set; }

		/// <summary>
		/// Gets the full name (<see cref="Schema"/> plus <see cref="Name"/>) of the stored procedure.
		/// </summary>
		internal string Fullname => !String.IsNullOrWhiteSpace(Schema) && !String.IsNullOrWhiteSpace(Name) ?
			String.Format("{0}.{1}", Schema, Name) :
			String.IsNullOrWhiteSpace(Schema) ?
				Name :
				Schema;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="StoredProcedure"/> class.
		/// </summary>
		/// <param name="dbContext">The <see cref="DbContext"/> toward which all database queries will be run.</param>
		/// <exception cref="ArgumentNullException"><paramref name="dbContext"/> is NULL.</exception>
		/// <remarks>The return type objects must have a default constructor.</remarks>
		public StoredProcedure(DbContext dbContext)
		{
			DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		#endregion

		#region Methods

		/// <summary>
		/// Executes the current <see cref="StoredProcedure"/>.
		/// </summary>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		public ResultSets CallStoredProcedure(params Type[] returnedTypes)
		{
			return CallStoredProcedure(null, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure"/>.
		/// </summary>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Name"/> is empty or whitespace.</exception>
		public ResultSets CallStoredProcedure(int? commandTimeout, params Type[] returnedTypes)
		{
			if (String.IsNullOrWhiteSpace(_name))
				throw new InvalidOperationException("Stored procedure name is not defined.");

			if (returnedTypes != null)
				ReturnedTypes.AddRange(returnedTypes);

			return DbContext.CallStoredProcedure(this, commandTimeout);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure"/>.
		/// </summary>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		public async Task<ResultSets> CallStoredProcedureAsync(params Type[] returnedTypes)
		{
			return await CallStoredProcedureAsync(CancellationToken.None, null, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure"/>.
		/// </summary>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		public async Task<ResultSets> CallStoredProcedureAsync(CancellationToken token, params Type[] returnedTypes)
		{
			return await CallStoredProcedureAsync(token, null, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure"/>.
		/// </summary>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		public async Task<ResultSets> CallStoredProcedureAsync(int? commandTimeout, params Type[] returnedTypes)
		{
			return await CallStoredProcedureAsync(CancellationToken.None, commandTimeout, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure"/>.
		/// </summary>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Name"/> is empty or whitespace.</exception>
		public async Task<ResultSets> CallStoredProcedureAsync(CancellationToken token, int? commandTimeout, params Type[] returnedTypes)
		{
			if (String.IsNullOrWhiteSpace(_name))
				throw new InvalidOperationException("Stored procedure name is not defined.");

			if (returnedTypes != null)
				ReturnedTypes.AddRange(returnedTypes);

			return await DbContext.CallStoredProcedureAsync(this, token, commandTimeout);
		}

		/// <summary>
		/// Defined the name of the current stored procedure.
		/// </summary>
		/// <param name="name">The name of the current stored procedure.</param>
		/// <returns>The current <see cref="StoredProcedure"/> instance. Allow to chain configuration with the fluent API.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public StoredProcedure HasName(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException("name can not be empty or whitespace.", "name");

			Name = name;
			return this;
		}

		/// <summary>
		/// Defined the schema of the current stored procedure.
		/// </summary>
		/// <param name="schema">The schema used for the current stored procedure.</param>
		/// <returns>The current <see cref="StoredProcedure"/> instance. Allow to chain configuration with the fluent API.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="schema"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="schema"/> is empty or whitespace.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public StoredProcedure HasSchema(string schema)
		{
			if (schema == null)
				throw new ArgumentNullException(nameof(schema));
			if (String.IsNullOrWhiteSpace(schema))
				throw new ArgumentException("schema can not be empty or whitespace.", "schema");

			Schema = schema;
			return this;
		}

		/// <summary>
		/// Sets the data types of resultsets returned by the stored procedure. Order is important.
		/// </summary>
		/// <param name="types">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>The current <see cref="StoredProcedure"/> instance. Allow to chain configuration with the fluent API.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="types"/> is NULL.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public StoredProcedure ReturnsTypes(params Type[] types)
		{
			if (types == null)
				throw new ArgumentNullException(nameof(types));

			ReturnedTypes.AddRange(types);
			return this;
		}

		#endregion
	}

	/// <inheritdoc />
	/// <summary>
	/// Generic version of <see cref="T:Sonata.Data.Entity.StoredProcedure" />, allowing to take input parameters. 
	/// </summary>
	/// <typeparam name="T">The type containing all input parameters of the <see cref="T:Sonata.Data.Entity.StoredProcedure" />.</typeparam>
	public class StoredProcedure<T> : StoredProcedure
	{
		#region Members

		/// <summary>
		/// Contains a mapping of property names to parameter names. We do this since this mapping is complex; 
		/// i.e.: the default parameter name may be overridden by the Name attribute.
		/// </summary>
		internal Dictionary<String, String> MappedParams = new Dictionary<string, string>();

		#endregion

		#region Constructors

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sonata.Data.Entity.StoredProcedure`1" /> class.
		/// </summary>
		/// <param name="dbContext">The <see cref="!:DbContext" /> toward which all database queries will be run.</param>
		/// <remarks>This constructor will be called by <see cref="!:DbContext.InitializeStoredProcs" />.</remarks>
		public StoredProcedure(DbContext dbContext)
			: base(dbContext)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Executes the current <see cref="StoredProcedure{T}"/>.
		/// </summary>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <remarks><see cref="!:DbContext.InitializeStoredProcs"/> has to be called prior to using this method.</remarks>
		public ResultSets CallStoredProcedure(T inputParameters, params Type[] returnedTypes)
		{
			return CallStoredProcedure(null, inputParameters, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure{T}"/>.
		/// </summary>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <remarks><see cref="!:DbContext.InitializeStoredProcs"/> has to be called prior to using this method.</remarks>
		public ResultSets CallStoredProcedure(int? commandTimeout, T inputParameters, params Type[] returnedTypes)
		{
			// set up default return types if none provided
			if (returnedTypes == null)
				returnedTypes = new Type[] { };

			// Set up the stored proc parameters
			if (String.IsNullOrEmpty(Name))
				SetupStoredProcedure(returnedTypes);

			return DbContext.CallStoredProcedure(this, commandTimeout, inputParameters);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure{T}"/>.
		/// </summary>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <remarks><see cref="!:DbContext.InitializeStoredProcs"/> has to be called prior to using this method.</remarks>
		public async Task<ResultSets> CallStoredProcedureAsync(T inputParameters, params Type[] returnedTypes)
		{
			return await CallStoredProcedureAsync(CancellationToken.None, null, inputParameters, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure{T}"/>.
		/// </summary>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <remarks><see cref="!:DbContext.InitializeStoredProcs"/> has to be called prior to using this method.</remarks>
		public async Task<ResultSets> CallStoredProcedureAsync(int? commandTimeout, T inputParameters, params Type[] returnedTypes)
		{
			return await CallStoredProcedureAsync(CancellationToken.None, commandTimeout, inputParameters, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure{T}"/>.
		/// </summary>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>A list of lists containing result data from the stored procedure.</returns>
		/// <remarks><see cref="!:DbContext.InitializeStoredProcs"/> has to be called prior to using this method.</remarks>
		public async Task<ResultSets> CallStoredProcedureAsync(CancellationToken token, T inputParameters, params Type[] returnedTypes)
		{
			return await CallStoredProcedureAsync(token, null, inputParameters, returnedTypes);
		}

		/// <summary>
		/// Executes the current <see cref="StoredProcedure{T}"/>.
		/// </summary>
		/// <param name="token">Cancellation token (optional).</param>
		/// <param name="commandTimeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
		/// <param name="inputParameters">An instance of an <see cref="Object"/> containing data to be sent to the stored procedure (INPUT and OUTPUT parameters).</param>
		/// <param name="returnedTypes">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public async Task<ResultSets> CallStoredProcedureAsync(CancellationToken token, int? commandTimeout, T inputParameters, params Type[] returnedTypes)
		{
			if (returnedTypes == null)
				returnedTypes = new Type[] { };

			if (String.IsNullOrWhiteSpace(Name))
				SetupStoredProcedure(returnedTypes);

			return await DbContext.CallStoredProcedureAsync(this, token, commandTimeout, inputParameters);
		}

		/// <summary>
		/// Defined the name of the current stored procedure.
		/// </summary>
		/// <param name="name">The name of the current stored procedure.</param>
		/// <returns>The current <see cref="StoredProcedure"/> instance. Allow to chain configuration with the fluent API.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public new StoredProcedure<T> HasName(String name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException("name can not be empty or whitespace.", "name");

			base.HasName(name);
			return this;
		}

		/// <summary>
		/// Defined the schema of the current stored procedure.
		/// </summary>
		/// <param name="schema">The schema used for the current stored procedure.</param>
		/// <returns>The current <see cref="StoredProcedure"/> instance. Allow to chain configuration with the fluent API.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="schema"/> is NULL.</exception>
		/// <exception cref="ArgumentException"><paramref name="schema"/> is empty or whitespace.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public new StoredProcedure<T> HasSchema(String schema)
		{
			if (schema == null)
				throw new ArgumentNullException(nameof(schema));
			if (String.IsNullOrWhiteSpace(schema))
				throw new ArgumentException("schema can not be empty or whitespace.", "schema");

			base.HasSchema(schema);
			return this;
		}

		/// <summary>
		/// Sets the data types of resultsets returned by the stored procedure. Order is important.
		/// </summary>
		/// <param name="types">The data types of resultsets returned by the stored procedure. Order is important.</param>
		/// <returns>The current <see cref="StoredProcedure"/> instance. Allow to chain configuration with the fluent API.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="types"/> is NULL.</exception>
		/// <remarks>Use this property when using the Fluent API.</remarks>
		public new StoredProcedure<T> ReturnsTypes(params Type[] types)
		{
			if (types == null)
				throw new ArgumentNullException(nameof(types));

			base.ReturnsTypes(types);
			return this;
		}

		/// <summary>
		/// Stores output parameter values back into the <paramref name="inputParameters"/> object.
		/// </summary>
		/// <param name="inputSqlParameters">The list of valorized input parameters passed to the stored procedure (the .NET properties casted to <see cref="SqlParameter"/>).</param>
		/// <param name="inputParameters">The list of input parameters passed to the stored procedure (the .NET properties) (INPUT and OUTPUT parameters).</param>
		internal void ProcessOutputParms(IEnumerable<SqlParameter> inputSqlParameters, T inputParameters)
		{
			//	Gget the list of mapped properties for this type.
			var mappedProperties = typeof(T).GetMappedProperties().ToList();

			//	We want to write data back to properties for every non-input only parameter.
			foreach (var inputSqlParameter in inputSqlParameters.Where(p => p.Direction != ParameterDirection.Input))
			{
				var mappedPropertyName = MappedParams.Where(p => p.Key == inputSqlParameter.ParameterName).Select(p => p.Value).First();

				//	Extract the matching property and set its value.
				var parameterMappedProperty = mappedProperties.FirstOrDefault(p => p.Name == mappedPropertyName);
				if (parameterMappedProperty == null)
					continue;

				if (inputSqlParameter.Value == DBNull.Value)
				{
					if (!parameterMappedProperty.PropertyType.IsGenericType && parameterMappedProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
						throw new InvalidOperationException(
							String.Format("Can not set a NULL value to a non-nullable property. Property name: {0}", parameterMappedProperty.Name));
				}
				else
				{
					parameterMappedProperty.SetValue(inputParameters, inputSqlParameter.Value, null);
				}
			}
		}

		/// <summary>
		/// Convert parameters from type T to an <see cref="IEnumerable{SqlParameter}"/>.
		/// </summary>
		/// <param name="inputParameters">The parameters  to convert (INPUT and OUTPUT parameters).</param>
		/// <returns>An <see cref="IEnumerable{SqlParameter}"/> corresponding to the <paramref name="inputParameters"/>.</returns>
		/// <exception cref="InvalidCastException">The property type is not of type <see cref="IEnumerable{T}"/> althought the <see cref="SqlDbType"/> of its <see cref="StoredProcedureParameterAttribute"/> is of type <see cref="SqlDbType.Structured"/>.</exception>
		internal IEnumerable<SqlParameter> Parameters(T inputParameters)
		{
			//	Clear the parameter to property mapping since we'll be recreating this
			MappedParams.Clear();

			//	List of parameters we'll be returning
			var sqlParameters = new List<SqlParameter>();

			//	Properties that we're converting to parameters are everything without a NotMapped attribute
			foreach (var mappedProperty in typeof(T).GetMappedProperties())
			{
				#region Process Attributes

				//	Create a parameter and store default name - property name
				var holder = new SqlParameter { ParameterName = mappedProperty.Name };

				//	Override of parameter name by attribute
				if (mappedProperty.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() is StoredProcedureParameterAttribute storedProcedureParameterAttribute)
				{
					holder.ParameterName = storedProcedureParameterAttribute.ParameterName;
					holder.Direction = storedProcedureParameterAttribute.Direction;			// Default is input
					holder.Size = storedProcedureParameterAttribute.Size;
					holder.SqlDbType = storedProcedureParameterAttribute.Type;
					holder.TypeName = storedProcedureParameterAttribute.TypeName;
					holder.Precision = storedProcedureParameterAttribute.Precision;
					holder.Scale = storedProcedureParameterAttribute.Scale;

					if (holder.SqlDbType == SqlDbType.NVarChar
						|| holder.SqlDbType == SqlDbType.VarChar)
					{
						holder.Size = holder.Size == 0 ? int.MaxValue : holder.Size;
					}
				}

				#endregion

				#region Save parameter value

				//	Store table values, scalar value or null
				var value = mappedProperty.GetValue(inputParameters, null);
				if (value == null)
				{
					//	Set database null marker for null value
					holder.Value = DBNull.Value;
				}
				else if (holder.SqlDbType == SqlDbType.Structured)
				{
					//	Type must be IEnumerable type
					if (!(value is IEnumerable))
						throw new InvalidCastException(String.Format("{0} must be an IEnumerable Type", mappedProperty.Name));

					//	Get the type underlying the IEnumerable
					var basetype = value.GetType().GetEnumerableUnderlyingType();

					//	Get the table valued parameter table type name
					var userDefinedTableTypeAttribute = mappedProperty.GetCustomAttributes(typeof(UserDefinedTableTypeAttribute), false).FirstOrDefault() as UserDefinedTableTypeAttribute;
					if (userDefinedTableTypeAttribute == null && basetype != null)
						userDefinedTableTypeAttribute = basetype.GetCustomAttributes(typeof(UserDefinedTableTypeAttribute)).FirstOrDefault() as UserDefinedTableTypeAttribute;

					holder.TypeName = (userDefinedTableTypeAttribute != null) ? userDefinedTableTypeAttribute.Schema : DefaultSchema;
					holder.TypeName += ".";
					holder.TypeName += (userDefinedTableTypeAttribute != null) ? userDefinedTableTypeAttribute.Name : mappedProperty.Name;

					//	Generate table valued parameter
					holder.Value = ((IList)value).TableValuedParameter();
				}
				else
				{
					//	Process normal scalar value
					holder.Value = value;
				}

				#endregion

				//	Save the mapping between the parameter name and property name, since the parameter name can be overridden
				MappedParams.Add(holder.ParameterName, mappedProperty.Name);
				sqlParameters.Add(holder);
			}

			return sqlParameters;
		}

		/// <summary>
		/// Sets the schema and procedure name paramters from attributes and provided input type, and stores the indicated return types for handling output from the stored procedure call.
		/// </summary>
		/// <param name="returnedTypes">The list of <see cref="Type"/> that can be returned by the stored procedure.</param>
		private void SetupStoredProcedure(IEnumerable<Type> returnedTypes)
		{
			//	Set default schema if not set via attributes on the property in DbContext.
			Schema = DefaultSchema;

			//	Allow override by attribute on the input type object
			if (typeof(T).GetCustomAttributes(typeof(UserDefinedTableTypeAttribute)).FirstOrDefault() is UserDefinedTableTypeAttribute schemaAttribute)
				Schema = schemaAttribute.Schema;

			//	Set procedure name if it was not set on the property in DbContext
			if (String.IsNullOrWhiteSpace(Name))
			{
				Name = typeof(T).Name;

				//	Allow override by attribute
				if (typeof(T).GetCustomAttributes(typeof(StoredProcedureAttribute)).FirstOrDefault() is StoredProcedureAttribute procedureNameAttribute)
					Name = procedureNameAttribute.Name;
			}

			ReturnedTypes.AddRange(returnedTypes);
		}

		#endregion
	}
}