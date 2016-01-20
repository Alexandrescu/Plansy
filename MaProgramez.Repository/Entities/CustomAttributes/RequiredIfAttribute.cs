using System.ComponentModel.DataAnnotations;

namespace MaProgramez.Repository.Entities.CustomAttributes
{
    public class RequiredIfAttribute : ValidationAttribute
    {
        /// <summary>
        /// http://anthonyvscode.com/2011/07/14/mvc-3-requiredif-validator-for-multiple-values/
        /// </summary>
        private RequiredAttribute _innerAttribute = new RequiredAttribute();

        private string _dependentProperty;
        private object[] _targetValue;

        public RequiredIfAttribute(string dependentProperty, params object[] targetValue)
        {
            this._dependentProperty = dependentProperty;
            this._targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // get a reference to the property this validation depends upon
            var containerType = validationContext.ObjectInstance.GetType();
            var field = containerType.GetProperty(this._dependentProperty);

            if (field != null)
            {
                // get the value of the dependent property
                var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);

                foreach (var obj in _targetValue)
                {
                    // compare the value against the target value
                    if ((dependentvalue == null && this._targetValue == null) ||
                        (dependentvalue != null && dependentvalue.Equals(obj)))
                    {
                        // match => means we should try validating this field
                        if (!_innerAttribute.IsValid(value))
                            // validation failed - return an error
                            return new ValidationResult(this.ErrorMessage, new[] { validationContext.MemberName });
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}