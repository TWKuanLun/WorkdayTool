using System;

namespace MigrationSolution.CustomException
{
    class CustomValueNullException : Exception
    {
        public CustomValueNullException()
        {
        }

        public CustomValueNullException(string message)
            : base(message)
        {
        }

        public CustomValueNullException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
