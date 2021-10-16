using System.Runtime.Serialization;

namespace CodeCompanion.Extensions.Dapper.Postgres
{
    public class NoRefcursorLeftException : Exception
    {
        public NoRefcursorLeftException() { }

        public NoRefcursorLeftException(string message) : base(message) { }

        public NoRefcursorLeftException(string message, Exception innerException) : base(message, innerException) { }

        protected NoRefcursorLeftException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}