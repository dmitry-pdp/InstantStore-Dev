using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantStore.Domain.Exceptions
{
    public class ModelValidationException : ApplicationException
    {
        public ModelValidationException(string message)
            : base(message)
        {
        }
    }
}
