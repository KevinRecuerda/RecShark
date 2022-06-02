using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RecShark.Extensions;

namespace RecShark.AspNetCore.Extensions
{
    public static class ValidationHelper
    {
        public static ValidationResult Range(DateTime? from, DateTime? to)
        {
            if (to < from)
                return new ValidationResult("'from' must be before 'to'", new[] { nameof(from), nameof(to) });

            return null;
        }

        public static ValidationResult Scope(string param, Type type, bool optional = false, string paramName = "")
        {
            if (optional && string.IsNullOrEmpty(param))
                return null;

            var values = type.GetConstValues();
            if (!param.In(values))
                return new ValidationResult($"expected values: [{values.ToString(",")}]", new[] { paramName });

            return null;
        }

        public static ValidationResult Scope(string[] param, Type type, bool optional = false, string paramName = "")
        {
            if (optional && param.IsNullOrEmpty())
                return null;

            var values = type.GetConstValues();
            if (param.Except(values).Any())
                return new ValidationResult($"expected values: [{values.ToString(",")}]", new[] { paramName });

            return null;
        }

        public static void Ensure(this ValidationResult result)
        {
            if (result != null)
                throw new ArgumentException(result.ErrorMessage, result.MemberNames.ToString(","));
        }
    }
}
