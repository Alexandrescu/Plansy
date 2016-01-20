namespace MaProgramez.Website.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Resources;
    using System.Web.Mvc;

    public class EnforceTrueAttribute : ValidationAttribute, IClientValidatable
    {
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            if (value.GetType() != typeof(bool)) throw new InvalidOperationException("can only be used on boolean properties.");
            return (bool)value == true;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            string errorMessage = String.Empty;
            if (String.IsNullOrWhiteSpace(ErrorMessage))
            {
                // Check if they supplied an error message resource
                if (ErrorMessageResourceType != null && !String.IsNullOrWhiteSpace(ErrorMessageResourceName))
                {
                    var resMan = new ResourceManager(ErrorMessageResourceType.FullName, ErrorMessageResourceType.Assembly);
                    errorMessage = resMan.GetString(ErrorMessageResourceName);
                }
            }
            else
            {
                errorMessage = ErrorMessage;
            }

            yield return new ModelClientValidationRule
            {
                ErrorMessage = errorMessage,
                ValidationType = "enforcetrue"
            };
        }
    }
}