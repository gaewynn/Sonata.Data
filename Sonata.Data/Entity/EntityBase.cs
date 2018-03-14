#region Namespace Sonata.Data.Entity
//	The Sonata.Data.Entity namespace contains classes that provides access to the core functionalities related to EntityBase.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using Sonata.Security;

namespace Sonata.Data.Entity
{
	/// <summary>
	/// The base class of all entities used by the model objects.
	/// </summary>
	[DataContract]
	public class EntityBase
	{
		#region Properties

		/// <summary>
		/// Gets or sets the creation date of the current instance.
		/// </summary>
		[Required]
		[DataMember(IsRequired = false)]
		public DateTime CreationDate { get; set; }

		/// <summary>
		/// Gets or sets the name of the creator of the current instance.
		/// </summary>
		[StringLength(255)]
		[Required]
		[DataMember(IsRequired = false)]
		public string CreatedBy { get; set; }

		/// <summary>
		/// Gets or sets the modification date of the current instance.
		/// </summary>
		[DataMember]
		public DateTime? ModificationDate { get; set; }

		/// <summary>
		/// Gets or sets the name of the last modifier of the current instance.
		/// </summary>
		[StringLength(255)]
		[DataMember]
		public string ModifiedBy { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityBase"/> class.
		/// </summary>
		/// <remarks>When initializing, automatically set the <see cref="CreationDate"/> and <see cref="CreatedBy"/> properties with the current date and the current user name.</remarks>
		public EntityBase()
		{
			CreationDate = DateTime.UtcNow;
			CreatedBy = SecurityProvider.WindowsUserProvider.GetCurrentUsername();
			ModificationDate = CreationDate;
			ModifiedBy = CreatedBy;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Updates the modifications information on the <see cref="EntityBase"/>.
		/// </summary>
		public void UpdateProperty()
		{
			ModificationDate = DateTime.UtcNow;
			ModifiedBy = SecurityProvider.WindowsUserProvider.GetCurrentUsername();
		}

		#endregion
	}

	/// <inheritdoc />
	/// <summary>
	/// The generic base class of all entities used by the model objects.
	/// Allows to validate the entity by providing a custom <see cref="T:System.ComponentModel.DataAnnotations.IValidatableObject" />.
	/// </summary>
	/// <typeparam name="TValidator">A type implementing <see cref="T:System.ComponentModel.DataAnnotations.IValidatableObject" /> allowing to validate the current <see cref="T:Sonata.Data.Entity.EntityBase`1" />.</typeparam>
	[DataContract]
	public class EntityBase<TValidator> : EntityBase
		where TValidator : IValidatableObject, new()
	{
		#region Members

		private readonly TValidator _validator = new TValidator();
		private readonly ValidationContext _validationContext;

		#endregion

		#region Properties

		/// <summary>
		/// The list of validation errors on the current <see cref="EntityBase{T}"/>.
		/// If the <see cref="EntityBase{T}"/> is valid, this list is empty.
		/// </summary>
		/// <remarks>The <see cref="IValidatableObject.Validate"/> method has to be called to set this property.</remarks>
		[NotMapped]
		public IEnumerable<ValidationResult> ValidationResults { get; private set; }

		#endregion

		#region Constructors

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sonata.Data.Entity.EntityBase`1" /> class.
		/// </summary>
		/// <remarks>
		/// When initializing, automatically set the <see cref="P:Sonata.Data.Entity.EntityBase.CreationDate" /> and <see cref="P:Sonata.Data.Entity.EntityBase.CreatedBy" /> properties with the current date and the current user name.
		/// </remarks>
		public EntityBase()
		{
			_validationContext = new ValidationContext(this, null, new Dictionary<object, object>());
		}

		#endregion

		#region Methods

		/// <summary>
		/// Runs the validation process on the current <see cref="EntityBase{T}"/> based on the TValidator.
		/// </summary>
		/// <returns>TRUE if the current <see cref="EntityBase{TValidator}"/> is valid; otherwise FALSE.</returns>
		public bool Validate()
		{
			ValidationResults = _validator.Validate(_validationContext);
			return !ValidationResults.Any();
		}

		/// <summary>
		/// Initializes the <see cref="ValidationContext"/>.
		/// </summary>
		/// <param name="values">Values to inject in the <see cref="ValidationContext"/>.</param>
		public void InitializeValidationContextValues(Dictionary<object, object> values)
		{
			if (values == null)
				return;

			foreach (var value in values)
				_validationContext.Items.Add(value.Key, value.Value);
		}

		/// <summary>
		/// Sets a new validation context value that can be use during the validation process of the entity through the IValidatableObject interface.
		/// </summary>
		/// <param name="key">The key to add to the <see cref="ValidationContext"/>.</param>
		/// <param name="value">The value for the <paramref name="key"/> provided.</param>
		/// <remarks>If the key already exists in the <see cref="ValidationContext"/>, its value will be replaced.</remarks>
		public void SetValidationContextValue(object key, object value)
		{
			if (key == null)
				return;

			if (_validationContext.Items.ContainsKey(key))
				_validationContext.Items[key] = value;
			else
				_validationContext.Items.Add(key, value);
		}

		/// <summary>
		/// Removes the specified <paramref name="key"/> of the <see cref="ValidationContext"/>.
		/// </summary>
		/// <param name="key">The key to remove from the <see cref="ValidationContext"/>.</param>
		public void RemoveValidationContextValue(object key)
		{
			if (key == null)
				return;

			if (_validationContext.Items.ContainsKey(key))
				_validationContext.Items.Remove(key);
		}

		/// <summary>
		/// Clears the current <see cref="ValidationContext"/>.
		/// </summary>
		public void ClearValidationContext()
		{
			_validationContext.Items.Clear();
		}

		#endregion
	}
}
