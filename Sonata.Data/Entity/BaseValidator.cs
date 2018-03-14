#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Sonata.Data.Entity
{
	public enum CrudOperation
	{
		Create = 1,
		Read = 2,
		Update = 3,
		Delete = 4
	}

	/// <inheritdoc />
	/// <summary>
	/// The base class of all entities used by the model objects.
	/// </summary>
	[DataContract]
	public abstract class BaseValidator : IValidatableObject
	{
		#region Constants

		public const string CrudKey = "CRUD";

		#endregion

		#region Properties

		public static Dictionary<object, object> CreationValidationRules = new Dictionary<object, object> { { CrudKey, CrudOperation.Create } };
		public static Dictionary<object, object> ReadValidationRules = new Dictionary<object, object> { { CrudKey, CrudOperation.Read } };
		public static Dictionary<object, object> UpdateValidationRules = new Dictionary<object, object> { { CrudKey, CrudOperation.Update } };
		public static Dictionary<object, object> DeleteValidationRules = new Dictionary<object, object> { { CrudKey, CrudOperation.Delete } };

		#endregion

		#region Methods

		public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);

		#endregion
	}
}
