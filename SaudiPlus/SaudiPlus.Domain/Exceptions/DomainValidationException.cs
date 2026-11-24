using System;

namespace SaudiPlus.Domain.Exceptions
{
    public sealed class DomainValidationException : Exception
    {
        public DomainValidationException(string message)
        : base(message)
        {
        }
    }
}
