using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azw.FacialOsc
{
    public class UnexpectedEnumValueException : Exception
    {
        public UnexpectedEnumValueException(Enum value)
            : base("Value " + value + " of enum " + typeof(Enum).Name + " is not implemented.")
        {
        }
    }
}
