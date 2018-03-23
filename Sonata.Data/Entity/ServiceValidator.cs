#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using Sonata.Core.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sonata.Data.Entity
{
	/// <summary>
	/// Represents the base class of all services.
	/// </summary>
	public class ServiceValidator
	{
		#region Members

		private static StringCiAiComparer _stringCiAiComparer;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a <see cref="StringCiAiComparer"/> configured to trim the compared strings during comparison.
		/// </summary>
		protected static StringCiAiComparer StringCiAiComparer => _stringCiAiComparer ?? (_stringCiAiComparer = new StringCiAiComparer(true, false));

		#endregion

		#region Methods

		public bool RunValidation<T>(List<string> logs, Type source, EntityBase<T> entity, CrudOperation? crudOperation = null, string baseMessage = null)
			where T : IValidatableObject, new()
		{
			if (crudOperation.HasValue)
			{
				switch (crudOperation.Value)
				{
					case CrudOperation.Create: entity.InitializeValidationContextValues(BaseValidator.CreationValidationRules); break;
					case CrudOperation.Read: entity.InitializeValidationContextValues(BaseValidator.ReadValidationRules); break;
					case CrudOperation.Update: entity.InitializeValidationContextValues(BaseValidator.UpdateValidationRules); break;
					case CrudOperation.Delete: entity.InitializeValidationContextValues(BaseValidator.DeleteValidationRules); break;
				}
			}

			if (!entity.Validate())
				logs.AddRange(entity.ValidationResults.Select(e => $"{baseMessage ?? String.Empty} {e.ErrorMessage}"));

			return !logs.Any();
		}

		public bool RunValidation<T>(ConcurrentStack<string> logs, Type source, EntityBase<T> entity, CrudOperation? crudOperation = null, string baseMessage = null)
			where T : IValidatableObject, new()
		{
			if (crudOperation.HasValue)
			{
				switch (crudOperation.Value)
				{
					case CrudOperation.Create: entity.InitializeValidationContextValues(BaseValidator.CreationValidationRules); break;
					case CrudOperation.Read: entity.InitializeValidationContextValues(BaseValidator.ReadValidationRules); break;
					case CrudOperation.Update: entity.InitializeValidationContextValues(BaseValidator.UpdateValidationRules); break;
					case CrudOperation.Delete: entity.InitializeValidationContextValues(BaseValidator.DeleteValidationRules); break;
				}
			}

			if (!entity.Validate())
				logs.PushRange(entity.ValidationResults.Select(e => $"{baseMessage ?? String.Empty} {e.ErrorMessage}").ToArray());

			return !logs.Any();
		}
		
		#endregion
	}
}
