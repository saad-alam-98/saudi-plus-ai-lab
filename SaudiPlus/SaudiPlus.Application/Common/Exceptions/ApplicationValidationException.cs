using System;
using System.Collections.Generic;
using System.Text;

namespace SaudiPlus.Application.Common.Exceptions
{
    public sealed class ApplicationValidationException : Exception
    {
        public ApplicationValidationException(string message)
        : base(message)
        {
        }
    }
}
