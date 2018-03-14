#region Namespace Sonata.Data.SqlServer
//	TODO
# endregion

using System.Data.SqlClient;

namespace Sonata.Data.SqlServer
{
	public class SqlServerConnectionProxy
	{
		#region Properties

		public SqlConnection Connection { get; set; }

		public string Database { get; set; }

		#endregion
	}
}