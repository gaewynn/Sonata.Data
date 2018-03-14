#region Namespace Sonata.Data.Extensions
//	TODO
#endregion

using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Sonata.Data.Entity.Mapping;

namespace Sonata.Data.Extensions
{
	public static class DbDataReaderExtension
	{
		/// <summary>
		/// Read data for the current result row from a reader into a destination object, by the name
		/// of the properties on the destination object.
		/// </summary>
		/// <param name="instance">data reader holding return data</param>
		/// <param name="t">object to populate</param>
		/// <param name="props">properties list to copy from result set row 'reader' to object 't'</param>
		/// <returns></returns>
		internal static object ReadRecord(this DbDataReader instance, object t, PropertyInfo[] props)
		{
			var name = "";

			// copy mapped properties
			foreach (var p in props)
			{
				try
				{
					// default name is property name, override of parameter name by attribute
					name = (!(p.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() is StoredProcedureParameterAttribute attr)) ? p.Name : attr.ParameterName;

					// see if we're being asked to stream this property
					if (p.GetCustomAttributes(typeof(StoredProcedureStreamOutputParamterAttribute), false).FirstOrDefault() is StoredProcedureStreamOutputParamterAttribute stream)
					{
						// if yes, then write to a stream
						instance.ReadFromStream(t, name, p, stream);
					}
					else
					{
						// get the requested value from the returned dataset and handle null values
						var data = instance[name];
						p.SetValue(t, data is DBNull ? null : instance[name], null);
					}
				}
				catch (Exception ex)
				{
					if (ex is IndexOutOfRangeException)
					{
						// if the result set doesn't have this value, intercept the exception
						// and set the property value to null / 0
						p.SetValue(t, null, null);
					}
					else
					{
						// tell the user *where* we had an exception
						var outer = new Exception(String.Format("Exception processing return column {0} in {1}",
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
		/// <param name="instance"></param>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="p"></param>
		/// <param name="stream"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		internal static void ReadFromStream(this DbDataReader instance, object t, string name, PropertyInfo p, StoredProcedureStreamOutputParamterAttribute stream)
		{
			// handle streamed values
			var tostream = CreateStream(stream, t);
			try
			{
				using (var fromstream = instance.GetStream(instance.GetOrdinal(name)))
				{
					fromstream.CopyTo(tostream);
				}

				// reset our stream position
				tostream.Seek(0, 0);

				// For array output, copy tostream to user's array and close stream since user will never see it
				if (p.PropertyType.Name.Contains("[]") || p.PropertyType.Name.Contains("Array"))
				{
					var item = new Byte[tostream.Length];
					tostream.Read(item, 0, (int)tostream.Length);
					p.SetValue(t, item, null);
					tostream.Close();
				}
				else if (p.PropertyType.Name.Contains("String"))
				{
					var r = new StreamReader(tostream, ((StoredProcedureStreamToMemoryParamterAttribute)stream).GetEncoding());
					var text = r.ReadToEnd();
					p.SetValue(t, text, null);
					r.Close();
				}
				else if (p.PropertyType.Name.Contains("Stream"))
				{
					// NOTE: User will have to close the stream if they don't tell us to close file streams!
					if (typeof(StoredProcedureStreamToFileParamterAttribute) == stream.GetType() && !stream.LeaveStreamOpen)
						tostream.Close();

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
				tostream?.Close();

				// pass the exception on
				throw;
			}
		}

		/// <summary>
		/// Read streamed data from SQL Server into a file or memory stream. If the target property for the data in object 't' is not
		/// a stream, then copy the data to an array or String.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="p"></param>
		/// <param name="stream"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		internal static async Task ReadFromStreamAsync(this DbDataReader instance, object t, string name, PropertyInfo p, StoredProcedureStreamOutputParamterAttribute stream, CancellationToken token)
		{
			// handle streamed values
			var tostream = CreateStream(stream, t);
			try
			{
				using (var fromstream = instance.GetStream(instance.GetOrdinal(name)))
				{
					await fromstream.CopyToAsync(tostream, (int)fromstream.Length, token);
				}

				// reset our stream position
				tostream.Seek(0, 0);

				// For array output, copy tostream to user's array and close stream since user will never see it
				if (p.PropertyType.Name.Contains("[]") || p.PropertyType.Name.Contains("Array"))
				{
					var item = new Byte[tostream.Length];
					tostream.Read(item, 0, (int)tostream.Length);
					p.SetValue(t, item, null);
					tostream.Close();
				}
				else if (p.PropertyType.Name.Contains("String"))
				{
					var r = new StreamReader(tostream, ((StoredProcedureStreamToMemoryParamterAttribute)stream).GetEncoding());
					var text = r.ReadToEnd();
					p.SetValue(t, text, null);
					r.Close();
				}
				else
				{
					// NOTE: User will have to close the stream if they don't tell us to close file streams!
					if (typeof(StoredProcedureStreamToFileParamterAttribute) == stream.GetType() && !stream.LeaveStreamOpen)
						tostream.Close();

					// pass our created stream back to the user since they asked for a stream output
					p.SetValue(t, tostream, null);
				}
			}
			catch (Exception)
			{
				// always close the stream on an exception
				tostream?.Close();

				// pass on the error
				throw;
			}
		}

		/// <summary>
		/// Read data for the current result row from a reader into a destination object, by the name
		/// of the properties on the destination object.
		/// </summary>
		/// <param name="instance">data reader holding return data</param>
		/// <param name="t">object to populate</param>
		/// <returns></returns>
		/// <param name="props">properties list to copy from result set row 'reader' to object 't'</param>
		/// <param name="token">Cancellation token for asyc process cancellation</param>
		internal static async Task<object> ReadRecordAsync(this DbDataReader instance, object t, PropertyInfo[] props, CancellationToken token)
		{
			String name = "";

			// copy mapped properties
			foreach (var p in props)
			{
				try
				{
					// default name is property name, override of parameter name by attribute
					name = (!(p.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() is StoredProcedureParameterAttribute attr)) ? p.Name : attr.ParameterName;

					// see if we're being asked to write this property to a stream
					if (p.GetCustomAttributes(typeof(StoredProcedureStreamOutputParamterAttribute), false).FirstOrDefault() is StoredProcedureStreamOutputParamterAttribute stream)
					{
						// if yes, wait on the stream processing
						await instance.ReadFromStreamAsync(t, name, p, stream, token);
					}
					else
					{
						// get the requested value from the returned dataset and handle null values
						var data = instance[name];
						p.SetValue(t, data is DBNull ? null : instance[name], null);
					}
				}
				catch (Exception ex)
				{
					if (ex is IndexOutOfRangeException)
					{
						// if the result set doesn't have this value, intercept the exception
						// and set the property value to null / 0
						p.SetValue(t, null, null);
					}
					else
					{
						// tell the user *where* we had an exception
						var outer = new Exception(String.Format("Exception processing return column {0} in {1}",
							name, t.GetType().Name), ex);

						// something bad happened, pass on the exception
						throw outer;
					}
				}
			}

			return t;
		}

		/// <summary>
		/// Create a Stream for saving large object data from the server, use the
		/// stream attribute data
		/// </summary>
		/// <param name="format"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		internal static Stream CreateStream(StoredProcedureStreamOutputParamterAttribute format, object t)
		{
			var output = typeof(StoredProcedureStreamToFileParamterAttribute) == format.GetType() ?
				((StoredProcedureStreamToFileParamterAttribute)format).CreateStream(t) :
				((StoredProcedureStreamToMemoryParamterAttribute)format).CreateStream();

			// if buffering was requested, overlay bufferedstream on our stream
			if (format.Buffered)
				output = new BufferedStream(output);

			return output;
		}
	}
}