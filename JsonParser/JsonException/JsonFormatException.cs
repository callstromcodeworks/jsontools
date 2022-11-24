using System.Runtime.Serialization;

namespace JsonTools.JsonException
{
    public class JsonFormatException : FormatException
    {
        public JsonFormatException()
        {
        }

        public JsonFormatException(string? message) : base(message)
        {
        }

        public JsonFormatException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected JsonFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
