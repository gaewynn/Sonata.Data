#region Namespace Sonata.Data.Extensions
//	TODO
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.Server;
using Sonata.ComponentModel.DataAnnotations;
using Sonata.Core.Extensions;
using Sonata.Data.Entity.Mapping;

namespace Sonata.Data.Extensions
{
	public static class ListExtension
	{
		/// <summary>
		/// Randomizes elements inside the specified <paramref name="list"/> using the Fisher-Yates algorithm.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the specified <paramref name="list"/>.</typeparam>
		/// <param name="list">The list to randomize.</param>
		public static void Shuffle<T>(this IList<T> list)
		{
			var provider = new System.Security.Cryptography.RNGCryptoServiceProvider();

			var n = list.Count;
			while (n > 1)
			{
				var box = new byte[1];
				do
					provider.GetBytes(box);
				while (!(box[0] < n * (Byte.MaxValue / n)));

				var k = (box[0] % n);
				n--;

				var value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		/// <summary>
		/// Do the work of converting a source data object to SqlDataRecords 
		/// using the parameter attributes to create the table valued parameter definition
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		internal static IEnumerable<SqlDataRecord> TableValuedParameter(this IList instance)
		{
			// get the object type underlying our table
			var t = instance.GetType().GetEnumerableUnderlyingType();

			// list of converted values to be returned to the caller
			var recordlist = new List<SqlDataRecord>();

			// get all mapped properties
			var props = t.GetMappedProperties();

			// get the column definitions, into an array
			var columnlist = new List<SqlMetaData>();

			// get the propery column name to property name mapping
			// and generate the SqlMetaData for each property/column
			var mapping = new Dictionary<string, string>();
			foreach (var p in props)
			{
				// default name is property name, override of parameter name by attribute
				var name = (!(p.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() is StoredProcedureParameterAttribute attr)) ? p.Name : attr.ParameterName;
				mapping.Add(name, p.Name);

				// get column type
				var coltype = (!(p.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() is StoredProcedureParameterAttribute ct)) ? SqlDbType.Int : ct.Type;

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
						var sa = p.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() as StoredProcedureParameterAttribute;
						var size = sa?.Size ?? 50;
						column = new SqlMetaData(name, coltype, size);
						break;

					case SqlDbType.Decimal:
						// get column precision and scale
						var pa = p.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() as StoredProcedureParameterAttribute;
						var precision = pa?.Precision ?? (byte)10;
						var sca = p.GetCustomAttributes(typeof(StoredProcedureParameterAttribute), false).FirstOrDefault() as StoredProcedureParameterAttribute;
						var scale = sca?.Scale ?? (byte)2;
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
			foreach (var s in instance)
			{
				// create the sql data record using the column definition
				var record = new SqlDataRecord(columnlist.ToArray());
				for (var i = 0; i < columnlist.Count; i++)
				{
					// locate the value of the matching property
					var value = props.First(p => p.Name == mapping[columnlist[i].Name])
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
}