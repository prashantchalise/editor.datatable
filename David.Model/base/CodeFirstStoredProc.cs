using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Data.Common;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.SqlServer.Server;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace David.Model.ChaliseStoredProc
{
	/// <summary>
	/// Holds multiple Result Sets returned from a Stored Procedure call. 
	/// </summary>
	public class ResultsList : IEnumerable
	{
		// our internal object that is the list of results lists
		List<List<object>> thelist = new List<List<object>>();

		/// <summary>
		/// Add a results list to the results set
		/// </summary>
		/// <param name="list"></param>
		public void Add(List<object> list)
		{
			thelist.Add(list);
		}

		/// <summary>
		/// Return an enumerator over the internal list
		/// </summary>
		/// <returns>Enumerator over List<object> that make up the result sets </returns>
		public IEnumerator GetEnumerator()
		{
			return thelist.GetEnumerator();
		}

		/// <summary>
		/// Return the count of result sets
		/// </summary>
		public Int32 Count
		{
			get { return thelist.Count; }
		}

		/// <summary>
		/// Get the nth results list item
		/// </summary>
		/// <param name="index"></param>
		/// <returns>List of objects that make up the result set</returns>
		public List<object> this[int index]
		{
			get { return thelist[index]; }
		}

		/// <summary>
		/// Return the result set that contains a particular type and does a cast to that type.
		/// </summary>
		/// <typeparam name="T">Type that was listed in StoredProc object as a possible return type for the stored procedure</typeparam>
		/// <returns>List of T; if no results match, returns an empty list</returns>
		public List<T> ToList<T>()
		{
			// search each non-empty results list 
			foreach (List<object> list in thelist.Where(p => p.Count > 0).Select(p => p))
			{
				// compare types of the first element - this is why we filter for non-empty results
				if (typeof(T) == list[0].GetType())
				{
					// do cast to return type
					return list.Cast<T>().Select(p => p).ToList();
				}
			}

			// no matches? return empty list
			return new List<T>();
		}

		/// <summary>
		/// Return the result set that contains a particular type and does a cast to that type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>Array of T; if no results match, returns an empty array</returns>
		public T[] ToArray<T>()
		{
			// search each non-empty results list 
			foreach (List<object> list in thelist.Where(p => p.Count > 0).Select(p => p))
			{
				// compare types of the first element - this is why we filter for non-empty results
				if (typeof(T) == list[0].GetType())
				{
					// do cast to return type
					return list.Cast<T>().Select(p => p).ToArray();
				}
			}

			// no matches? return empty array
			return new T[0];
		}
	}

	/// <summary>
	/// Genericized version of StoredProc object, takes a .Net POCO object type for the parameters. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class StoredProc<T> : StoredProc
	{
		//-----------------------------------------------------------------------------------------
		// New Style Interface; more in line with EF style 
		//-----------------------------------------------------------------------------------------

		/// <summary>
		/// New Interface
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public ResultsList CallStoredProc(T data, params Type[] types)
		{
			return CallStoredProc(this.commandTimeout, data, types);
		}

		/// <summary>
		/// New Interface
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="CommandTimeout">Timeout value for this command execution</param>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public ResultsList CallStoredProc(int? CommandTimeout, T data, params Type[] types)
		{
			// protect ourselves from the old style of calling this 
			if (null == _context)
			{
				throw new Exception("Not Properly Initialized. Call InitializeStoredProcs in the DbContext constructor.");
			}

			// set up default return types if none provided
			if (null == types)
				types = new Type[] { };

			// Set up the stored proc parameters
			if (String.IsNullOrEmpty(procname))
			{
				SetupStoredProc(types);
			}

			return CallStoredProc(CommandTimeout, Transaction, data, types);
		}

		/// <summary>
		/// New Interface
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="CommandTimeout">Timeout value for this command execution</param>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public ResultsList CallStoredProc(int? CommandTimeout, DbTransaction transaction, T data, params Type[] types)
		{
			// protect ourselves from the old style of calling this 
			if (null == _context)
			{
				throw new Exception("Not Properly Initialized. Call InitializeStoredProcs in the DbContext constructor.");
			}

			// set up default return types if none provided
			if (null == types)
				types = new Type[] { };

			// Set up the stored proc parameters
			if (String.IsNullOrEmpty(procname))
			{
				SetupStoredProc(types);
			}

			commandTimeout = CommandTimeout;
			Transaction = transaction;

			return CodeFirstStoredProcs.CallStoredProc(_context, this, CommandTimeout, transaction, data);
		}

		/// <summary>
		/// New Interface, Asnyc
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public async Task<ResultsList> CallStoredProcAsync(T data, params Type[] types)
		{
			return await CallStoredProcAsync(this.cancellationToken, this.commandTimeout, this.Transaction, data, types);
		}

		/// <summary>
		/// New Interface, Asnyc
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="CommandTimeout">Timeout value for this execution</param>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public async Task<ResultsList> CallStoredProcAsync(int? CommandTimeout, T data, params Type[] types)
		{
			commandTimeout = CommandTimeout;
			return await CallStoredProcAsync(this.cancellationToken, CommandTimeout, this.Transaction, data, types);
		}

		/// <summary>
		/// New Interface, Asnyc
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public async Task<ResultsList> CallStoredProcAsync(CancellationToken token, T data, params Type[] types)
		{
			cancellationToken = token;
			return await CallStoredProcAsync(token, this.commandTimeout, this.Transaction, data, types);
		}

		/// <summary>
		/// New Interface, Asnyc
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public async Task<ResultsList> CallStoredProcAsync(DbTransaction transaction, T data, params Type[] types)
		{
			Transaction = transaction;
			return await CallStoredProcAsync(this.cancellationToken, this.commandTimeout, transaction, data, types);
		}

		/// <summary>
		/// New Interface, Asnyc
		/// Call the stored procedure; InitializeStoredProcs must be called prior to using this method
		/// </summary>
		/// <param name="token">Cancellation Token (optional) </param>
		/// <param name="CommandTimeout">Timout value for this command execution</param>
		/// <param name="data">POCO object containing data to be sent to the stored procedure</param>
		/// <param name="transaction">sql transaction in which to enroll the stored procedure call</param>
		/// <param name="types">Types of POCO objects that will be used to house any returned data</param>
		/// <returns>List of lists containing result data from stored proc</returns>
		public async Task<ResultsList> CallStoredProcAsync(CancellationToken token, int? CommandTimeout, DbTransaction transaction, T data, params Type[] types)
		{
			// protect ourselves from the old style of calling this 
			if (null == _context)
			{
				throw new Exception("Not Properly Initialized. Call InitializeStoredProcs in the DbContext constructor.");
			}

			if (null == types)
				types = new Type[] { };

			if (String.IsNullOrEmpty(procname))
			{
				SetupStoredProc(types);
			}

			cancellationToken = token;
			commandTimeout = CommandTimeout;
			Transaction = transaction;

			return await CodeFirstStoredProcs.CallStoredProcAsync(_context, this, token, CommandTimeout, transaction, data);
		}

		/// <summary>
		/// Constructor for new style interface. This is called by InitializeStoredProcs
		/// </summary>
		/// <param name="context">DbContext that this procedure call will use for database connectivity</param>
		public StoredProc(DbContext context)
			: base(context)
		{
			// save database context for processing
			_context = context;

			// initialize properties
			cancellationToken = CancellationToken.None;
			commandTimeout = null;
			Transaction = null;
		}

		//-----------------------------------------------------------------------------------------
		// Old Style Interface; kept for backwards compatibility
		//-----------------------------------------------------------------------------------------

		/// <summary>
		/// Constructor. Note that the return type objects must have a default constructor!
		/// </summary>
		/// <param name="types">Types returned by the stored procedure. Order is important!</param>
		public StoredProc(params Type[] types)
			: base()
		{
			schema = "dbo";

			// analyse return types
			SetupStoredProc(types);

			// initialize properties
			cancellationToken = CancellationToken.None;
			commandTimeout = null;
			Transaction = null;
		}

		/// <summary>
		/// Set the schema and proc name paramters from attributes and provided input type, and
		/// store the indicated return types for handling output from the stored proc call
		/// </summary>
		/// <param name="types">List of types that can be returned by the stored procedure</param>
		private void SetupStoredProc(Type[] types)
		{
			// set default schema if not set via attributes on the property in DbContext
			if (String.IsNullOrEmpty(schema))
			{
				schema = "dbo";

				// allow override by attribute on the input type object
				var schema_attr = typeof(T).GetAttribute<StoredProcAttributes.Schema>();
				if (null != schema_attr)
					schema = schema_attr.Value;
			}

			// set proc name if it was not set on the property in DbContext
			if (String.IsNullOrEmpty(procname))
			{
				// set default proc name
				procname = typeof(T).Name;

				// allow override by attribute
				var procname_attr = typeof(T).GetAttribute<StoredProcAttributes.Name>();
				if (null != procname_attr)
					procname = procname_attr.Value;
			}

			outputtypes.AddRange(types);
		}

		/// <summary>
		/// Contains a mapping of property names to parameter names. We do this since this mapping is complex; 
		/// i.e. the default parameter name may be overridden by the Name attribute
		/// </summary>
		internal Dictionary<String, String> MappedParams = new Dictionary<string, string>();

		/// <summary>
		/// Store output parameter values back into the data object
		/// </summary>
		/// <param name="parms">List of parameters</param>
		/// <param name="data">Source data object</param>
		internal void ProcessOutputParms(IEnumerable<SqlParameter> parms, T data)
		{
			// get the list of properties for this type
			PropertyInfo[] props = typeof(T).GetMappedProperties();

			// we want to write data back to properties for every non-input only parameter
			foreach (SqlParameter parm in parms
				.Where(p => p.Direction != ParameterDirection.Input)
				.Select(p => p))
			{
				// get the property name mapped to this parameter
				String propname = MappedParams.Where(p => p.Key == parm.ParameterName).Select(p => p.Value).First();

				// extract the matchingproperty and set its value
				PropertyInfo prop = props.Where(p => p.Name == propname).FirstOrDefault();

				// Store output parm value, handle null returns
				if (parm.Value.GetType() == typeof(System.DBNull))
					prop.SetValue(data, null, null);
				else
					prop.SetValue(data, parm.Value, null);
			}
		}

		/// <summary>
		/// Convert parameters from type T properties to SqlParameters
		/// </summary>
		/// <param name="data">Source data object</param>
		/// <returns></returns>
		internal IEnumerable<SqlParameter> Parameters(T data)
		{
			// clear the parameter to property mapping since we'll be recreating this
			MappedParams.Clear();

			// list of parameters we'll be returning
			List<SqlParameter> parms = new List<SqlParameter>();

			// properties that we're converting to parameters are everything without
			// a NotMapped attribute
			foreach (PropertyInfo p in typeof(T).GetMappedProperties())
			{
				//---------------------------------------------------------------------------------
				// process attributes
				//---------------------------------------------------------------------------------

				// create parameter and store default name - property name
				SqlParameter holder = new SqlParameter()
				{
					ParameterName = p.Name
				};

				// override of parameter name by attribute
				var name = p.GetAttribute<StoredProcAttributes.Name>();
				if (null != name)
					holder.ParameterName = name.Value;

				// save direction (default is input)
				var dir = p.GetAttribute<StoredProcAttributes.Direction>();
				if (null != dir)
					holder.Direction = dir.Value;

				// save size
				var size = p.GetAttribute<StoredProcAttributes.Size>();
				if (null != size)
					holder.Size = size.Value;

				// save database type of parameter
				var parmtype = p.GetAttribute<StoredProcAttributes.ParameterType>();
				if (null != parmtype)
					holder.SqlDbType = parmtype.Value;

				// save user-defined type name
				var typename = p.GetAttribute<StoredProcAttributes.TypeName>();
				if (null != typename)
					holder.TypeName = typename.Value;

				// save precision
				var precision = p.GetAttribute<StoredProcAttributes.Precision>();
				if (null != precision)
					holder.Precision = precision.Value;

				// save scale
				var scale = p.GetAttribute<StoredProcAttributes.Scale>();
				if (null != scale)
					holder.Scale = scale.Value;

				// get streaming 
				var stream = p.GetAttribute<StoredProcAttributes.StreamOutput>();

				// save the mapping between the parameter name and property name, since the parameter
				// name can be overridden
				MappedParams.Add(holder.ParameterName, p.Name);

				//---------------------------------------------------------------------------------
				// Save parameter value
				//---------------------------------------------------------------------------------

				// store table values, scalar value or null
				if (null == data)
				{
					holder.Value = DBNull.Value;
				}
				else
				{
					var value = p.GetValue(data, null);
					if (value == null)
					{
						// set database null marker for null value
						holder.Value = DBNull.Value;
					}
					else if (SqlDbType.Structured == holder.SqlDbType)
					{
						// catcher - tvp must be ienumerable type
						if (!(value is IEnumerable))
							throw new InvalidCastException(String.Format("{0} must be an IEnumerable Type", p.Name));

						// set a null sqlparameter object for empty tables
						if (0 == ((IList)value).Count)
						{
							holder.Value = DBNull.Value;
						}
						else
						{
							// ge the type underlying the IEnumerable
							Type basetype = CodeFirstStoredProcHelpers.GetUnderlyingType(value.GetType());

							// get the table valued parameter table type name
							var schema = p.GetAttribute<StoredProcAttributes.Schema>();
							if (null == schema && null != basetype)
								schema = basetype.GetAttribute<StoredProcAttributes.Schema>();

							var tvpname = p.GetAttribute<StoredProcAttributes.TableName>();
							if (null == tvpname && null != basetype)
								tvpname = basetype.GetAttribute<StoredProcAttributes.TableName>();

							holder.TypeName = (null != schema) ? schema.Value : "dbo";
							holder.TypeName += ".";
							holder.TypeName += (null != tvpname) ? tvpname.Value : p.Name;

							// generate table valued parameter
							holder.Value = CodeFirstStoredProcHelpers.TableValuedParameter((IList)value);
						}
					}
					else
					{
						// process normal scalar value
						holder.Value = value;
					}
				}

				// add parameter to list
				parms.Add(holder);
			}

			return parms;
		}

		/// <summary>
		/// Fluent API - assign owner (schema)
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public new StoredProc<T> HasOwner(String owner)
		{
			base.HasOwner(owner);
			return this;
		}

		/// <summary>
		/// Fluent API - assign procedure name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public new StoredProc<T> HasName(String name)
		{
			base.HasName(name);
			return this;
		}

		/// <summary>
		/// Fluent API - set the data types of resultsets returned by the stored procedure. 
		/// Order is important! Note that the return type objects must have a default constructor!
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public new StoredProc<T> ReturnsTypes(params Type[] types)
		{
			base.ReturnsTypes(types);
			return this;
		}

		/// <summary>
		/// Command time out  - limit the time spent on a transaction
		/// </summary>
		public new StoredProc<T> UseTimeout(int timeout)
		{
			commandTimeout = timeout;
			return this;
		}

		/// <summary>
		/// Cancellation Token, used to signal cancellation by a user or other process
		/// </summary>
		public new StoredProc<T> UseCancellationToken(CancellationToken token)
		{
			cancellationToken = token;
			return this;
		}

		/// <summary>
		/// Tranasaction to enroll the sqlcommand in; should not be required w/ EF 6, as enrollment 
		/// should be automatic by then.
		/// </summary>
		public new StoredProc<T> UseTransaction(SqlTransaction tran)
		{
			Transaction = tran;
			return this;
		}

		/// <summary>
		/// Tranasaction to enroll the sqlcommand in; should not be required w/ EF 6, as enrollment 
		/// should be automatic by then.
		/// </summary>
		public new StoredProc<T> UseTransaction(DbTransaction tran)
		{
			Transaction = tran;
			return this;
		}

		//public new StoredProc<T> UseTransaction(DbContextTransaction tran)
		//{
		//    return UseTransaction((DbTransaction)tran.UnderlyingTransaction);
		//}

	}

	/// <summary>
	/// Represents a Stored Procedure in the database. Note that the return type objects
	/// must have a default constructor!
	/// </summary>
	public class StoredProc
	{
		// store a db context
		internal DbContext _context { get; set; }

		/// <summary>
		/// Database owner of this object
		/// </summary>
		public String schema { get; set; }

		/// <summary>
		/// Name of the stored procedure
		/// </summary>
		public String procname { get; set; }

		/// <summary>
		/// Fluent API - assign owner (schema)
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public StoredProc HasOwner(String owner)
		{
			schema = owner;
			return this;
		}

		/// <summary>
		/// Fluent API - assign procedure name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public StoredProc HasName(String name)
		{
			procname = name;
			return this;
		}

		/// <summary>
		/// Fluent API - set the data types of resultsets returned by the stored procedure. 
		/// Order is important! Note that the return type objects must have a default constructor!
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public StoredProc ReturnsTypes(params Type[] types)
		{
			outputtypes.AddRange(types);
			return this;
		}

		/// <summary>
		/// Command Behavior for 
		/// </summary>
		public CommandBehavior commandBehavior { get; set; }

		/// <summary>
		/// Get the fully (schema plus owner) name of the stored procedure
		/// </summary>
		internal String fullname
		{
			get { return schema + "." + procname; }
		}

		/// <summary>
		/// Command time out  - limit the time spent on a transaction
		/// </summary>
		public int? commandTimeout { get; set; }
		public StoredProc UseTimeout(int timeout)
		{
			commandTimeout = timeout;
			return this;
		}

		/// <summary>
		/// Cancellation Token, used to signal cancellation by a user or other process
		/// </summary>
		public CancellationToken cancellationToken { get; set; }
		public StoredProc UseCancellationToken(CancellationToken token)
		{
			cancellationToken = token;
			return this;
		}

		/// <summary>
		/// Tranasaction to enroll the sqlcommand in; should not be required w/ EF 6, as enrollment 
		/// should be automatic by then.
		/// </summary>
		public StoredProc UseTransaction(SqlTransaction tran)
		{
			Transaction = tran;
			return this;
		}

		/// <summary>
		/// Tranasaction to enroll the sqlcommand in; required if using
		/// connection.BeginTransaction or database.BeginTransaction instead of TransactionScope.
		/// </summary>
		public DbTransaction Transaction { get; set; }

		/// <summary>
		/// Compatibility with previous versions - replace public variable
		/// with property that sets the Transaction value
		/// </summary>
		public SqlTransaction sqlTransaction
		{
			get
			{
				return (SqlTransaction)Transaction;
			}

			set
			{
				Transaction = (DbTransaction)value;
			}

		}

		public StoredProc UseTransaction(DbTransaction tran)
		{
			Transaction = tran;
			return this;
		}

		//public StoredProc UseTransaction(DbContextTransaction tran)
		//{
		//    return UseTransaction((DbTransaction)tran.UnderlyingTransaction);
		//}

		//-----------------------------------------------------------------------------------------
		// New style interface
		//-----------------------------------------------------------------------------------------
		public StoredProc(DbContext context)
		{
			// save database context for processing
			_context = context;

			// default values 
			cancellationToken = CancellationToken.None;
			commandTimeout = null;
			Transaction = null;
		}

		/// <summary>
		/// New Interface
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		public ResultsList CallStoredProc(params Type[] types)
		{
			return CallStoredProc(this.commandTimeout, types);
		}

		/// <summary>
		/// New Interface
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="CommandTimeout">Timeout value for this command execution</param>
		/// <param name="types"></param>
		/// <returns></returns>
		public ResultsList CallStoredProc(int? CommandTimeout, params Type[] types)
		{
			return CallStoredProc(CommandTimeout, this.Transaction, types);
		}

		/// <summary>
		/// New Interface
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="CommandTimeout">Timeout value for this command execution</param>
		/// <param name="types"></param>
		/// <returns></returns>
		public ResultsList CallStoredProc(int? CommandTimeout, DbTransaction transaction, params Type[] types)
		{
			// protect ourselves from the old style of calling this 
			if (null == _context)
			{
				throw new Exception("Not Properly Initialized. Call InitializeStoredProcs in the DbContext constructor.");
			}

			if (String.IsNullOrEmpty(procname))
			{
				throw new Exception("Not properly Initialized. Missing stored procedure name.");
			}

			if (null != types)
				outputtypes.AddRange(types);

			commandTimeout = CommandTimeout;
			Transaction = transaction;

			return CodeFirstStoredProcs.CallStoredProc(_context, this, CommandTimeout, this.commandBehavior, this.Transaction);
		}

		/// <summary>
		/// New Interface, Async
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="types">List of output types from the stored proc</param>
		/// <returns></returns>
		public async Task<ResultsList> CallStoredProcAsync(params Type[] types)
		{
			return await CallStoredProcAsync(this.cancellationToken, this.commandTimeout, this.Transaction, types);
		}

		/// <summary>
		/// New Interface, Async
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="token">Cancellation token (optional) </param>
		/// <param name="types">List of output types from the stored proc</param>
		/// <returns></returns>
		public async Task<ResultsList> CallStoredProcAsync(CancellationToken token, params Type[] types)
		{
			return await CallStoredProcAsync(token, this.commandTimeout, this.Transaction, types);
		}

		/// <summary>
		/// New Interface, Async
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="CommandTimeout">Timeout value for this command execution</param>
		/// <param name="types">List of output types from the stored proc</param>
		/// <returns></returns>
		public async Task<ResultsList> CallStoredProcAsync(int? CommandTimeout, params Type[] types)
		{
			return await CallStoredProcAsync(this.cancellationToken, CommandTimeout, this.Transaction, types);
		}

		/// <summary>
		/// New Interface, Async
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="CommandTimeout">Timeout value for this command execution</param>
		/// <param name="types">List of output types from the stored proc</param>
		/// <returns></returns>
		public async Task<ResultsList> CallStoredProcAsync(DbTransaction transaction, params Type[] types)
		{
			return await CallStoredProcAsync(this.cancellationToken, this.commandTimeout, transaction, types);
		}

		/// <summary>
		/// New Interface, Async
		/// Execute the stored proc. Note that the proc name must be set elsewhere, by HasName directly,
		/// either during setup using the fluent interface or in code prior to it's first use.
		/// </summary>
		/// <param name="token">Cancellation token (optional) </param>
		/// <param name="CommandTimeout">Timeout value for this execution</param>
		/// <param name="transaction">sql transaction in which to enroll the stored procedure call</param>
		/// <param name="types">List of output types from the stored proc</param>
		/// <returns></returns>
		public async Task<ResultsList> CallStoredProcAsync(CancellationToken token, int? CommandTimeout, DbTransaction transaction, params Type[] types)
		{
			// protect ourselves from the old style of calling this 
			if (null == _context)
			{
				throw new Exception("Not Properly Initialized. Call InitializeStoredProcs in the DbContext constructor.");
			}

			if (String.IsNullOrEmpty(procname))
			{
				throw new Exception("Not properly Initialized. Missing stored procedure name.");
			}

			if (null != types)
				outputtypes.AddRange(types);

			cancellationToken = token;
			commandTimeout = CommandTimeout;
			Transaction = transaction;

			return await CodeFirstStoredProcs.CallStoredProcAsync(_context, this, token, CommandTimeout, transaction);
		}

		//-----------------------------------------------------------------------------------------
		// Original constructors
		//-----------------------------------------------------------------------------------------

		public StoredProc()
		{
			schema = "dbo";
		}

		public StoredProc(String name)
		{
			schema = "dbo";
			procname = name;
		}

		public StoredProc(String name, params Type[] types)
		{
			schema = "dbo";
			procname = name;
			outputtypes.AddRange(types);
		}

		public StoredProc(String owner, String name, params Type[] types)
		{
			schema = owner;
			procname = name;
			outputtypes.AddRange(types);
		}

		/// <summary>
		/// List of data types that this stored procedure returns as result sets. 
		/// Order is important!
		/// </summary>
		internal List<Type> outputtypes = new List<Type>();

		/// <summary>
		/// Get an array of types returned
		/// </summary>
		internal Type[] returntypes
		{
			get { return outputtypes.ToArray(); }
		}
	}

	/// <summary>
	/// Contains extension methods to Code First database objects for Stored Procedure processing
	/// Updated to include support for streaming and for async
	/// </summary>
	public static class CodeFirstStoredProcs
	{
		/// <summary>
		/// New Interface 
		/// Locate and initialize all the stored proc properties in this DbContext. This should be 
		/// called in the DbContext constructor.
		/// </summary>
		/// <param name="context"></param>
		public static void InitializeStoredProcs(this DbContext context)
		{
			Type contexttype = context.GetType();
			foreach (PropertyInfo proc in contexttype.GetProperties()
				.Where(p => p.PropertyType.Name.Contains("StoredProc")))
			{
				// create StoredProc object and save in DbContext property
				object m = proc.PropertyType.GetConstructor(new Type[] { contexttype }).Invoke(new DbContext[] { context });
				proc.SetValue(context, m);

				// see if there is a Name attribute on this property
				var nameattr = proc.GetAttribute<StoredProcAttributes.Name>();
				if (null != nameattr)
				{
					((StoredProc)m).HasName(nameattr.Value);
				}

				// see if there is a Schema attribute on this property
				var schemaattr = proc.GetAttribute<StoredProcAttributes.Schema>();
				if (null != schemaattr)
				{
					((StoredProc)m).HasOwner(schemaattr.Value);
				}

				// see if there is a ReturnTypes attribute on this property
				var typesattr = proc.GetAttribute<StoredProcAttributes.ReturnTypes>();
				if (null != typesattr)
				{
					((StoredProc)m).ReturnsTypes(typesattr.Returns);
				}

			}
		}

		/// <summary>
		/// Generic Typed version of calling a stored procedure - original interface
		/// </summary>
		/// <typeparam name="T">Type of object containing the parameter data</typeparam>
		/// <param name="context">Database Context to use for the call</param>
		/// <param name="procedure">Generic Typed stored procedure object</param>
		/// <param name="data">The actual object containing the parameter data</param>
		/// <returns></returns>
		public static ResultsList CallStoredProc<T>(this DbContext context, StoredProc<T> procedure, T data)
		{
			IEnumerable<SqlParameter> parms = procedure.Parameters(data);
			ResultsList results = context.ReadFromStoredProc(procedure.fullname, parms, null, procedure.commandBehavior, procedure.Transaction, procedure.returntypes);
			procedure.ProcessOutputParms(parms, data);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Call a stored procedure, passing in the stored procedure object and a list of parameters - original interface
		/// </summary>
		/// <param name="context">Database context used for the call</param>
		/// <param name="procedure">Stored Procedure</param>
		/// <param name="parms">List of parameters</param>
		/// <returns></returns>
		public static ResultsList CallStoredProc(this DbContext context, StoredProc procedure, IEnumerable<SqlParameter> parms = null)
		{
			ResultsList results = context.ReadFromStoredProc(procedure.fullname, parms, null, procedure.commandBehavior, procedure.Transaction, procedure.returntypes);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Generic Typed version of calling a stored procedure; add timeout parameter
		/// </summary>
		/// <typeparam name="T">Type of object containing the parameter data</typeparam>
		/// <param name="context">Database Context to use for the call</param>
		/// <param name="procedure">Generic Typed stored procedure object</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="data">The actual object containing the parameter data</param>
		/// <returns></returns>
		public static ResultsList CallStoredProc<T>(this DbContext context, StoredProc<T> procedure, int? CommandTimeout, T data)
		{
			IEnumerable<SqlParameter> parms = procedure.Parameters(data);
			ResultsList results = context.ReadFromStoredProc(procedure.fullname, parms, CommandTimeout, procedure.commandBehavior, procedure.Transaction, procedure.returntypes);
			procedure.ProcessOutputParms(parms, data);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Generic Typed version of calling a stored procedure; add timeout parameter
		/// </summary>
		/// <typeparam name="T">Type of object containing the parameter data</typeparam>
		/// <param name="context">Database Context to use for the call</param>
		/// <param name="procedure">Generic Typed stored procedure object</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="transaction">Sql transaction in which to enroll the stored procedure call</param>
		/// <param name="data">The actual object containing the parameter data</param>
		/// <returns></returns>
		public static ResultsList CallStoredProc<T>(this DbContext context, StoredProc<T> procedure, int? CommandTimeout, DbTransaction transaction, T data)
		{
			IEnumerable<SqlParameter> parms = procedure.Parameters(data);
			ResultsList results = context.ReadFromStoredProc(procedure.fullname, parms, CommandTimeout, procedure.commandBehavior, transaction, procedure.returntypes);
			procedure.ProcessOutputParms(parms, data);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Call a stored procedure, passing in the stored procedure object and a list of parameters; add timeout parameter to call 
		/// </summary>
		/// <param name="context">Database context used for the call</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="procedure">Stored Procedure</param>
		/// <param name="parms">List of parameters</param>
		/// <returns></returns>
		public static ResultsList CallStoredProc(this DbContext context, StoredProc procedure, int? CommandTimeout, IEnumerable<SqlParameter> parms = null)
		{
			ResultsList results = context.ReadFromStoredProc(procedure.fullname, parms, CommandTimeout, procedure.commandBehavior, procedure.Transaction, procedure.returntypes);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Call a stored procedure, passing in the stored procedure object and a list of parameters; add timeout parameter to call 
		/// </summary>
		/// <param name="context">Database context used for the call</param>
		/// <param name="procedure">Stored Procedure</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="transaction">Sql transaction in which to enroll the stored procedure call</param>
		/// <param name="parms">List of parameters</param>
		/// <returns></returns>
		public static ResultsList CallStoredProc(this DbContext context, StoredProc procedure, int? CommandTimeout, CommandBehavior commandbehavior, DbTransaction transaction, IEnumerable<SqlParameter> parms = null)
		{
			ResultsList results = context.ReadFromStoredProc(procedure.fullname, parms, CommandTimeout, commandbehavior, transaction, procedure.returntypes);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// internal
		/// 
		/// Call a stored procedure and get results back. 
		/// </summary>
		/// <param name="context">Code First database context object</param>
		/// <param name="procname">Qualified name of proc to call</param>
		/// <param name="parms">List of ParameterHolder objects - input and output parameters</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="transaction">Sql transaction in which to enroll the stored procedure call</param>
		/// <param name="outputtypes">List of types to expect in return. Each type *must* have a default constructor.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		internal static ResultsList ReadFromStoredProc(this DbContext context,
			String procname,
			IEnumerable<SqlParameter> parms = null,
			int? CommandTimeout = null,
			CommandBehavior commandbehavior = CommandBehavior.Default,
			DbTransaction transaction = null,
			params Type[] outputtypes)
		{
			// create our output set object
			ResultsList results = new ResultsList();

			// ensure that we have a type list, even if it's empty
			IEnumerator currenttype = (null == outputtypes) ?
				new Type[0].GetEnumerator() :
				outputtypes.GetEnumerator();

			// track whether we opened the connection or not
			Boolean opened = false;

			// handle to the database connection object
			var connection = context.Database.Connection;
			try
			{
				// open the connect for use and create a command object
				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
					opened = true;
				}
				using (var cmd = connection.CreateCommand())
				{
					// set up command object and add parms
					SetupStoredProcCall(procname, parms, CommandTimeout, transaction, cmd);

					// Do It! This actually makes the database call
					var reader = cmd.ExecuteReader(commandbehavior);

					// get the type we're expecting for the first result. If no types specified,
					// ignore all results
					if (currenttype.MoveNext())
					{
						// process results - repeat this loop for each result set returned by the stored proc
						// for which we have a result type specified
						do
						{
							// get properties to save for the current destination type
							PropertyInfo[] props = ((Type)currenttype.Current).GetMappedProperties();

							// create a destination for our results
							List<object> current = new List<object>();

							// process the result set
							while (reader.Read())
							{
								// create an object to hold this result
								object item = ((Type)currenttype.Current).GetConstructor(System.Type.EmptyTypes).Invoke(new object[0]);

								// copy data elements by parameter name from result to destination object
								reader.ReadRecord(item, props);

								// add newly populated item to our output list
								current.Add(item);
							}

							// add this result set to our return list
							results.Add(current);
						}
						while (reader.NextResult() && currenttype.MoveNext());
					}
					// close up the reader, we're done saving results
					//reader.Close();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error reading from stored proc " + procname + ": " + ex.Message, ex);
			}
			finally
			{
				if (opened)
					connection.Close();
			}

			return results;
		}

		/// <summary>
		/// Set up the DBCommand object, adding in options and parameters
		/// </summary>
		/// <param name="procname">Name of the stored proc</param>
		/// <param name="parms">SQLParameters to pass. Override: no parm created for TVP that is DBNull</param>
		/// <param name="CommandTimeout">Set Command Timeout Override</param>
		/// <param name="transaction">Transaction in which to enroll this proc call</param>
		/// <param name="cmd">DBCommand object representing the command to the database</param>
		private static void SetupStoredProcCall(String procname, IEnumerable<SqlParameter> parms, int? CommandTimeout, DbTransaction transaction, DbCommand cmd)
		{
			// command to execute is our stored procedure
			cmd.Transaction = transaction;
			cmd.CommandText = procname;
			cmd.CommandType = System.Data.CommandType.StoredProcedure;

			// Assign command timeout value, if one was provided
			cmd.CommandTimeout = null == CommandTimeout ? cmd.CommandTimeout : (int)CommandTimeout;

			// move parameters to command object
			if (null != parms)
				foreach (SqlParameter p in parms)
				{
					// Don't send any parm for null table-valued parameters
					if (!(SqlDbType.Structured == p.SqlDbType && DBNull.Value == p.Value))
					{
						cmd.Parameters.Add(p);
					}
				}
		}


		/// <summary>
		/// Generic Typed version of calling a stored procedure - async interface
		/// </summary>
		/// <typeparam name="T">Type of object containing the parameter data</typeparam>
		/// <param name="context">Database Context to use for the call</param>
		/// <param name="procedure">Generic Typed stored procedure object</param>
		/// <param name="data">The actual object containing the parameter data</param>
		/// <returns></returns>
		public static async Task<ResultsList> CallStoredProcAsync<T>(this DbContext context, StoredProc<T> procedure, T data)
		{
			IEnumerable<SqlParameter> parms = procedure.Parameters(data);
			ResultsList results = await context.ReadFromStoredProcAsync(procedure.fullname, CancellationToken.None, parms, null, procedure.commandBehavior, procedure.Transaction, procedure.returntypes);
			procedure.ProcessOutputParms(parms, data);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Call a stored procedure, passing in the stored procedure object and a list of parameters - async interface
		/// </summary>
		/// <param name="context">Database context used for the call</param>
		/// <param name="procedure">Stored Procedure</param>
		/// <param name="parms">List of parameters</param>
		/// <returns></returns>
		public static async Task<ResultsList> CallStoredProcAsync(this DbContext context, StoredProc procedure, IEnumerable<SqlParameter> parms = null)
		{
			ResultsList results = await context.ReadFromStoredProcAsync(procedure.fullname, CancellationToken.None, parms, null, procedure.commandBehavior, procedure.Transaction, procedure.returntypes);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Generic Typed version of calling a stored procedure; add timeout parameter - async interface
		/// </summary>
		/// <typeparam name="T">Type of object containing the parameter data</typeparam>
		/// <param name="context">Database Context to use for the call</param>
		/// <param name="procedure">Generic Typed stored procedure object</param>
		/// <param name="token">Cancellation token for asyc process cancellation</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="transaction">Sql transaction in which to enroll the stored procedure call</param>
		/// <param name="data">The actual object containing the parameter data</param>
		/// <returns></returns>
		public static async Task<ResultsList> CallStoredProcAsync<T>(this DbContext context, StoredProc<T> procedure, CancellationToken token, int? CommandTimeout, DbTransaction transaction, T data)
		{
			IEnumerable<SqlParameter> parms = procedure.Parameters(data);
			ResultsList results = await context.ReadFromStoredProcAsync(procedure.fullname, token, parms, CommandTimeout, procedure.commandBehavior, transaction, procedure.returntypes);
			procedure.ProcessOutputParms(parms, data);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// Call a stored procedure, passing in the stored procedure object and a list of parameters; add timeout parameter to call - async interface
		/// </summary>
		/// <param name="context">Database context used for the call</param>
		/// <param name="procedure">Stored Procedure</param>
		/// <param name="token">Cancellation token for asyc process cancellation</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="transaction">Sql transaction in which to enroll the stored procedure call</param>
		/// <param name="parms">List of parameters</param>
		/// <returns></returns>
		public static async Task<ResultsList> CallStoredProcAsync(this DbContext context, StoredProc procedure, CancellationToken token, int? CommandTimeout, DbTransaction transaction, IEnumerable<SqlParameter> parms = null)
		{
			ResultsList results = await context.ReadFromStoredProcAsync(procedure.fullname, token, parms, CommandTimeout, procedure.commandBehavior, transaction, procedure.returntypes);
			return results ?? new ResultsList();
		}

		/// <summary>
		/// internal
		/// 
		/// Call a stored procedure and get results back. - async version
		/// </summary>
		/// <param name="context">Code First database context object</param>
		/// <param name="procname">Qualified name of proc to call</param>
		/// <param name="token">Cancellation token for asyc process cancellation</param>
		/// <param name="parms">List of ParameterHolder objects - input and output parameters</param>
		/// <param name="CommandTimeout">Timeout for stored procedure call</param>
		/// <param name="transaction">Sql transaction in which to enroll the stored procedure call</param>
		/// <param name="outputtypes">List of types to expect in return. Each type *must* have a default constructor.</param>
		/// <returns></returns>
		internal static async Task<ResultsList> ReadFromStoredProcAsync(this DbContext context,
			String procname,
			CancellationToken token,
			IEnumerable<SqlParameter> parms = null,
			int? CommandTimeout = null,
			CommandBehavior commandbehavior = CommandBehavior.Default,
			DbTransaction transaction = null,
			params Type[] outputtypes)
		{
			// create our output set object
			ResultsList results = new ResultsList();

			// ensure that we have a type list, even if it's empty
			IEnumerator currenttype = (null == outputtypes) ?
				new Type[0].GetEnumerator() :
				outputtypes.GetEnumerator();

			// handle to the database connection object
			var connection = context.Database.Connection;
			try
			{
				// open the connect for use and create a command object
				await connection.OpenAsync(token);
				using (var cmd = connection.CreateCommand())
				{
					// set up command object and add parms
					SetupStoredProcCall(procname, parms, CommandTimeout, transaction, cmd);

					// Do It! This actually makes the database call
					var reader = await cmd.ExecuteReaderAsync(commandbehavior, token);

					// get the type we're expecting for the first result. If no types specified,
					// ignore all results
					if (currenttype.MoveNext())
					{
						// process results - repeat this loop for each result set returned by the stored proc
						// for which we have a result type specified
						do
						{
							// get properties to save for the current destination type
							PropertyInfo[] props = ((Type)currenttype.Current).GetMappedProperties();
							Dictionary<String, PropertyInfo> propertymap = CodeFirstStoredProcHelpers.MapPropertiesByName(props);

							// create a destination for our results
							List<object> current = new List<object>();

							// process the result set
							while (await reader.ReadAsync(token))
							{
								// create an object to hold this result
								object item = ((Type)currenttype.Current).GetConstructor(System.Type.EmptyTypes).Invoke(new object[0]);

								// copy data elements by parameter name from result to destination object
								await reader.ReadRecordAsync(item, propertymap, token);

								// add newly populated item to our output list
								current.Add(item);
							}

							// add this result set to our return list
							results.Add(current);
						}
						while (await reader.NextResultAsync(token) && currenttype.MoveNext());
					}

					// close up the reader, we're done saving results
					//reader.Close();

				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error reading from stored proc " + procname + ": " + ex.Message, ex);
			}
			finally
			{
				connection.Close();
			}

			return results;
		}
	}

	/// <summary>
	/// Contains extension methods to Code First database objects for Stored Procedure processing
	/// Updated to include support for streaming and async
	/// </summary>
	internal static class CodeFirstStoredProcHelpers
	{

		/// <summary>
		/// Get the underlying class type for lists, etc. that implement IEnumerable<>.
		/// </summary>
		/// <param name="listtype"></param>
		/// <returns></returns>
		public static Type GetUnderlyingType(Type listtype)
		{
			Type basetype = null;
			foreach (Type i in listtype.GetInterfaces())
				if (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
					basetype = i.GetGenericArguments()[0];

			return basetype;
		}

		/// <summary>
		/// Get properties of a type that do not have the 'NotMapped' attribute
		/// </summary>
		/// <param name="t">Type to examine for properites</param>
		/// <returns>Array of properties that can be filled</returns>
		public static PropertyInfo[] GetMappedProperties(this Type t)
		{
			var props1 = t.GetProperties();
			var props2 = props1
				.Where(p => p.GetAttribute<NotMappedAttribute>() == null)
				.Select(p => p);
			return props2.ToArray();
		}

		/// <summary>
		/// Get an attribute for a type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T GetAttribute<T>(this Type type)
			where T : Attribute
		{
			//var attributes  = type.GetAttribute<T>();
			var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(T), false).FirstOrDefault();
			return (T)attributes;
		}

		/// <summary>
		/// Get an attribute for a property
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyinfo"></param>
		/// <returns></returns>
		public static T GetAttribute<T>(this PropertyInfo propertyinfo)
			where T : Attribute
		{
			var attributes = propertyinfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
			return (T)attributes;
		}

		/// <summary>
		/// Read data for the current result row from a reader into a destination object, by the name
		/// of the properties on the destination object.
		/// </summary>
		/// <param name="reader">data reader holding return data</param>
		/// <param name="t">object to populate</param>
		/// <param name="props">properties list to copy from result set row 'reader' to object 't'</param>
		/// <returns></returns>
		public static object ReadRecord(this DbDataReader reader, object t, PropertyInfo[] props)
		{
			String name = "";

			// copy mapped properties
			foreach (PropertyInfo p in props)
			{
				try
				{
					// default name is property name, override of parameter name by attribute
					var attr = p.GetAttribute<StoredProcAttributes.Name>();
					name = (null == attr) ? p.Name : attr.Value;

					// see if we're being asked to stream this property
					var stream = p.GetAttribute<StoredProcAttributes.StreamOutput>();
					if (null != stream)
					{
						// if yes, then write to a stream
						ReadFromStream(reader, t, name, p, stream);
					}
					else
					{
						// get the requested value from the returned dataset and handle null values
						var data = reader[name];
						if (data.GetType() == typeof(System.DBNull))
							p.SetValue(t, null, null);
						else
							p.SetValue(t, reader[name], null);
					}
				}
				catch (Exception ex)
				{
					if (ex.GetType() == typeof(IndexOutOfRangeException))
					{
						// if the result set doesn't have this value, intercept the exception
						// and set the property value to null / 0
						p.SetValue(t, null, null);
					}
					else
					{
						// tell the user *where* we had an exception
						Exception outer = new Exception(String.Format("Exception processing return column {0} in {1}",
							name, t.GetType().Name), ex);

						// something bad happened, pass on the exception
						throw outer;
					}
				}
			}

			return t;
		}

		/// <summary>
		/// map names of properties to the propertyinfo object, allow for attributes to override the name.
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static Dictionary<String, PropertyInfo> MapPropertiesByName(PropertyInfo[] props)
		{
			Dictionary<String, PropertyInfo> propertymap = new Dictionary<string, PropertyInfo>(props.Length);
			for (int i = 0; i < props.Length; i++)
			{
				PropertyInfo p = props[i];
				// default name is property name, override of parameter name by attribute
				var attr = p.GetAttribute<StoredProcAttributes.Name>();
				var name = (null == attr) ? p.Name : attr.Value;

				propertymap.Add(name, p);
			}

			return propertymap;
		}

		/// <summary>
		/// Read data for the current result row from a reader into a destination object, by the name
		/// of the properties on the destination object.
		/// </summary>
		/// <param name="reader">data reader holding return data</param>
		/// <param name="t">object to populate</param>
		/// <returns></returns>
		/// <param name="props">properties list to copy from result set row 'reader' to object 't'</param>
		/// <param name="token">Cancellation token for asyc process cancellation</param>
		public async static Task<object> ReadRecordAsync(this DbDataReader reader, object t, Dictionary<String, PropertyInfo> propertymap, CancellationToken token)
		{
			// copy mapped properties
			for (int i = 0; i < reader.FieldCount; i++)
			{
				// get this column name
				String name = reader.GetName(i);

				// if we don't have this property in our map, just skip it. Note: we're doing a currentculture w/ no case search for the key.
				String key = propertymap.Keys.Where(k => k.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
				if (String.IsNullOrEmpty(key))
				{
					continue;
				}

				// get the relevant property for this column
				PropertyInfo p = propertymap[key];

				// get the data from the reader into the target object
				try
				{
					// see if we're being asked to write this property to a stream
					var stream = p.GetAttribute<StoredProcAttributes.StreamOutput>();
					if (null != stream)
					{
						// if yes, wait on the stream processing
						await ReadFromStreamAsync(reader, t, name, p, stream, token);
					}
					else
					{
						// get the requested value from the returned dataset and handle null values
						//var data = reader[name];
						var data = await reader.GetFieldValueAsync<object>(i);
						if (data.GetType() == typeof(System.DBNull))
							p.SetValue(t, null, null);
						else
							p.SetValue(t, data, null);
					}
				}
				catch (Exception ex)
				{
					if (ex.GetType() == typeof(IndexOutOfRangeException))
					{
						// if the result set doesn't have this value, intercept the exception
						// and set the property value to null / 0
						p.SetValue(t, null, null);
					}
					else
					{
						// tell the user *where* we had an exception
						Exception outer = new Exception(String.Format("Exception processing return column {0} in {1}",
							name, t.GetType().Name), ex);

						// something bad happened, pass on the exception
						throw outer;
					}
				}
			}

			return t;
		}

		/// <summary>
		/// Read streamed data from SQL Server into a file or memory stream. If the target property for the data in object 't' is not
		/// a stream, then copy the data to an array or String.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="p"></param>
		/// <param name="stream"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		private static void ReadFromStream(DbDataReader reader, object t, String name, PropertyInfo p, StoredProcAttributes.StreamOutput stream)
		{
			// handle streamed values
			Stream tostream = CreateStream(stream, t);
			try
			{
				using (Stream fromstream = reader.GetStream(reader.GetOrdinal(name)))
				{
					fromstream.CopyTo(tostream);
				}

				// reset our stream position
				tostream.Seek(0, 0);

				// For array output, copy tostream to user's array and close stream since user will never see it
				if (p.PropertyType.Name.Contains("[]") || p.PropertyType.Name.Contains("Array"))
				{
					Byte[] item = new Byte[tostream.Length];
					tostream.Read(item, 0, (int)tostream.Length);
					p.SetValue(t, item, null);
					tostream.Dispose();
					//tostream.Close();
				}
				else if (p.PropertyType.Name.Contains("String"))
				{
					StreamReader r = new StreamReader(tostream, ((StoredProcAttributes.StreamToMemory)stream).GetEncoding());
					String text = r.ReadToEnd();
					p.SetValue(t, text, null);
					//r.Close();
					r.Dispose();
				}
				else if (p.PropertyType.Name.Contains("Stream"))
				{
					// NOTE: User will have to close the stream if they don't tell us to close file streams!
					if (typeof(StoredProcAttributes.StreamToFile) == stream.GetType() && !((StoredProcAttributes.StreamToFile)stream).LeaveStreamOpen)
					{
						//tostream.Close();
						tostream.Dispose();
					}

					// pass our created stream back to the user since they asked for a stream output
					p.SetValue(t, tostream, null);
				}
				else
				{
					throw new Exception(String.Format("Invalid property type for property {0}. Valid types are Stream, byte or character arrays and String",
						p.Name));
				}
			}
			catch (Exception)
			{
				// always close the stream on exception
				if (null != tostream)
					//tostream.Close();
					tostream.Dispose();

				// pass the exception on
				throw;
			}
		}

		/// <summary>
		/// Read streamed data from SQL Server into a file or memory stream. If the target property for the data in object 't' is not
		/// a stream, then copy the data to an array or String.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="p"></param>
		/// <param name="stream"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private static async Task ReadFromStreamAsync(DbDataReader reader, object t, String name, PropertyInfo p, StoredProcAttributes.StreamOutput stream,
			CancellationToken token)
		{
			// handle streamed values
			Stream tostream = CreateStream(stream, t);
			try
			{
				using (Stream fromstream = reader.GetStream(reader.GetOrdinal(name)))
				{
					await fromstream.CopyToAsync(tostream, (int)fromstream.Length, token);
				}

				// reset our stream position
				tostream.Seek(0, 0);

				// For array output, copy tostream to user's array and close stream since user will never see it
				if (p.PropertyType.Name.Contains("[]") || p.PropertyType.Name.Contains("Array"))
				{
					Byte[] item = new Byte[tostream.Length];
					tostream.Read(item, 0, (int)tostream.Length);
					p.SetValue(t, item, null);
					//tostream.Close();
					tostream.Dispose();
				}
				else if (p.PropertyType.Name.Contains("String"))
				{
					StreamReader r = new StreamReader(tostream, ((StoredProcAttributes.StreamToMemory)stream).GetEncoding());
					String text = r.ReadToEnd();
					p.SetValue(t, text, null);
					//r.Close();
					r.Dispose();
				}
				else
				{
					// NOTE: User will have to close the stream if they don't tell us to close file streams!
					if (typeof(StoredProcAttributes.StreamToFile) == stream.GetType() && !((StoredProcAttributes.StreamToFile)stream).LeaveStreamOpen)
					{
						//tostream.Close();
						tostream.Dispose();
					}

					// pass our created stream back to the user since they asked for a stream output
					p.SetValue(t, tostream, null);
				}
			}
			catch (Exception)
			{
				// always close the stream on an exception
				if (null != tostream)
					//tostream.Close();
					tostream.Dispose();

				// pass on the error
				throw;
			}
		}

		/// <summary>
		/// Create a Stream for saving large object data from the server, use the
		/// stream attribute data
		/// </summary>
		/// <param name="format"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		internal static Stream CreateStream(StoredProcAttributes.StreamOutput format, object t)
		{
			Stream output;

			if (typeof(StoredProcAttributes.StreamToFile) == format.GetType())
			{
				// File stream
				output = ((StoredProcAttributes.StreamToFile)format).CreateStream(t); ;

				// build name from location and name property
			}
			else
			{
				// Memory Stream
				output = ((StoredProcAttributes.StreamToMemory)format).CreateStream();
			}

			// if buffering was requested, overlay bufferedstream on our stream
			//if (format.Buffered)
			//{
			//    output = new BufferedStream(output);
			//}

			return output;
		}

		/// <summary>
		/// Do the work of converting a source data object to SqlDataRecords 
		/// using the parameter attributes to create the table valued parameter definition
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		internal static IEnumerable<SqlDataRecord> TableValuedParameter(IList table)
		{
			// get the object type underlying our table
			Type t = CodeFirstStoredProcHelpers.GetUnderlyingType(table.GetType());

			// list of converted values to be returned to the caller
			List<SqlDataRecord> recordlist = new List<SqlDataRecord>();

			// get all mapped properties
			PropertyInfo[] props = CodeFirstStoredProcHelpers.GetMappedProperties(t);

			// get the column definitions, into an array
			List<SqlMetaData> columnlist = new List<SqlMetaData>();

			// get the propery column name to property name mapping
			// and generate the SqlMetaData for each property/column
			Dictionary<String, String> mapping = new Dictionary<string, string>();
			foreach (PropertyInfo p in props)
			{
				// default name is property name, override of parameter name by attribute
				var attr = p.GetAttribute<StoredProcAttributes.Name>();
				String name = (null == attr) ? p.Name : attr.Value;
				mapping.Add(name, p.Name);

				// get column type
				var ct = p.GetAttribute<StoredProcAttributes.ParameterType>();
				SqlDbType coltype = (null == ct) ? SqlDbType.Int : ct.Value;

				// create metadata column definition
				SqlMetaData column;
				switch (coltype)
				{
					case SqlDbType.Binary:
					case SqlDbType.Char:
					case SqlDbType.NChar:
					case SqlDbType.Image:
					case SqlDbType.VarChar:
					case SqlDbType.NVarChar:
					case SqlDbType.Text:
					case SqlDbType.NText:
					case SqlDbType.VarBinary:
						// get column size
						var sa = p.GetAttribute<StoredProcAttributes.Size>();
						int size = (null == sa) ? 50 : sa.Value;
						column = new SqlMetaData(name, coltype, size);
						break;

					case SqlDbType.Decimal:
						// get column precision and scale
						var pa = p.GetAttribute<StoredProcAttributes.Precision>();
						Byte precision = (null == pa) ? (byte)10 : pa.Value;
						var sca = p.GetAttribute<StoredProcAttributes.Scale>();
						Byte scale = (null == sca) ? (byte)2 : sca.Value;
						column = new SqlMetaData(name, coltype, precision, scale);
						break;

					default:
						column = new SqlMetaData(name, coltype);
						break;
				}

				// Add metadata to column list
				columnlist.Add(column);
			}

			// load each object in the input data table into sql data records
			foreach (object s in table)
			{
				// create the sql data record using the column definition
				SqlDataRecord record = new SqlDataRecord(columnlist.ToArray());
				for (int i = 0; i < columnlist.Count(); i++)
				{
					// locate the value of the matching property
					var value = props.Where(p => p.Name == mapping[columnlist[i].Name])
						.First()
						.GetValue(s, null);

					// set the value
					record.SetValue(i, value);
				}

				// add the sql data record to our output list
				recordlist.Add(record);
			}

			// return our list of data records
			return recordlist;
		}
	}

	/// <summary>
	/// Contains attributes for Stored Procedure processing
	/// </summary>
	public class StoredProcAttributes
	{
		/// <summary>
		/// Parameter name override. Default value for parameter name is the name of the 
		/// property. This overrides that default with a user defined name.
		/// </summary>
		public class Name : Attribute
		{
			public String Value { get; set; }

			public Name(String s)
				: base()
			{
				Value = s;
			}
		}

		/// <summary>
		/// Size in bytes of returned data. Should be used on output and returncode parameters.
		/// </summary>
		public class Size : Attribute
		{
			public Int32 Value { get; set; }

			public Size(Int32 s)
				: base()
			{
				Value = s;
			}
		}

		/// <summary>
		/// Size in bytes of returned data. Should be used on output and returncode parameters.
		/// </summary>
		public class Precision : Attribute
		{
			public Byte Value { get; set; }

			public Precision(Byte s)
				: base()
			{
				Value = s;
			}
		}

		/// <summary>
		/// Size in bytes of returned data. Should be used on output and returncode parameters.
		/// </summary>
		public class Scale : Attribute
		{
			public Byte Value { get; set; }

			public Scale(Byte s)
				: base()
			{
				Value = s;
			}
		}

		/// <summary>
		/// Defines the direction of data flow for the property/parameter.
		/// </summary>
		public class Direction : Attribute
		{
			public ParameterDirection Value { get; set; }

			public Direction(ParameterDirection d)
			{
				Value = d;
			}
		}

		/// <summary>
		/// Define the SqlDbType for the parameter corresponding to this property.
		/// </summary>
		public class ParameterType : Attribute
		{
			public SqlDbType Value { get; set; }

			public ParameterType(SqlDbType t)
			{
				Value = t;
			}
		}

		/// <summary>
		/// Allows the setting of the parameter type name for user defined types in the database
		/// </summary>
		public class TypeName : Attribute
		{
			public String Value { get; set; }

			public TypeName(String t)
			{
				Value = t;
			}
		}

		/// <summary>
		/// Allows the setting of the user defined table type name for table valued parameters
		/// </summary>
		public class TableName : Attribute
		{
			public String Value { get; set; }

			public TableName(String t)
			{
				Value = t;
			}
		}

		/// <summary>
		/// Allows the setting of the user defined table type name for table valued parameters
		/// </summary>
		public class Schema : Attribute
		{
			public String Value { get; set; }

			public Schema(String t)
			{
				Value = t;
			}
		}

		/// <summary>
		/// Allows the setting of the user defined table type name for table valued parameters
		/// </summary>
		public class ReturnTypes : Attribute
		{
			public Type[] Returns { get; set; }

			public ReturnTypes(params Type[] values)
			{
				Returns = values;
			}
		}

		/// <summary>
		/// Allows the setting of the user defined table type name for table valued parameters
		/// </summary>
		public class StreamOutput : Attribute
		{
			public Boolean Buffered { get; set; }
			public Boolean LeaveStreamOpen { get; set; }

			public StreamOutput()
			{
			}
		}

		/// <summary>
		/// Stream to File output
		/// </summary>
		public class StreamToFile : StreamOutput
		{
			public String FileNameField { get; set; }
			public String Location { get; set; }

			/// <summary>
			/// Create the file stream using location attribute data and filename in returned data
			/// </summary>
			/// <param name="t"></param>
			/// <returns></returns>
			internal Stream CreateStream(object t)
			{
				String filename = Location;
				var tp = t.GetType();
				var p = tp.GetProperty(FileNameField);
				if (null != p)
				{
					var name = p.GetValue(t, null);
					if (null != name)
						filename = Path.Combine(filename, name.ToString());
				}

				return new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);

			}

			public StreamToFile()
			{
			}
		}

		/// <summary>
		/// Stream to MemoryStream, Array or String
		/// </summary>
		public class StreamToMemory : StreamOutput
		{
			public String Encoding { get; set; }

			/// <summary>
			/// Create Memory Stream 
			/// </summary>
			/// <returns></returns>
			internal Stream CreateStream()
			{
				return new MemoryStream();
			}

			/// <summary>
			/// Resolve Encoding for conversion of MemoryStream to String
			/// </summary>
			/// <returns></returns>
			internal System.Text.Encoding GetEncoding()
			{
				//MethodInfo method = typeof(System.Text.Encoding).GetMethod(Encoding);

				//return (System.Text.Encoding)typeof(System.Text.Encoding).InvokeMember(Encoding,
				//    BindingFlags.Public | BindingFlags.Static |  BindingFlags.IgnoreCase, null, null, null);


				MethodInfo method = typeof(System.Text.Encoding).GetMethod(Encoding);
				return (System.Text.Encoding)method.Invoke(this, null);
			}

			public StreamToMemory()
			{
				if (String.IsNullOrEmpty(Encoding))
				{
					Encoding = "Default";
				}
			}
		}

	}
}