using System;

namespace MigrationSolution.CustomException
{
    class LengthMoreThan127Exception : Exception
    {
        public LengthMoreThan127Exception()
        {
        }

        public LengthMoreThan127Exception(string message)
            : base(message)
        {
        }

        public LengthMoreThan127Exception(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
