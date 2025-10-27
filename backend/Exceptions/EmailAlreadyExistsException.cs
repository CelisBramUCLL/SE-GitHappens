using System;

namespace Dotnet_test.Exceptions
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string email) 
            : base($"There is already a user with the email'{email}'.")
        {
        }
    }
}