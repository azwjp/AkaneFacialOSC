using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Azw.FacialOsc.Properties;

namespace Azw.FacialOsc.View
{
    public class PositiveNumberValidation : ValidationRule
    {
        public PositiveNumberValidation() { }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string str || string.IsNullOrWhiteSpace(str)) return new ValidationResult(false, Resources.ValidationNoValue);
            //str = str.Trim();
            double inputNum;
            if (!double.TryParse(str, out inputNum))
            {
                return new ValidationResult(false, Resources.ValidationShouldBePositive);
            }
            else if (inputNum <= 0)
            {
                return new ValidationResult(false, Resources.ValidationShouldBePositive);
            }

            return ValidationResult.ValidResult;
        }
    }
}
