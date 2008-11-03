using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace BlogExtension.OneNote
{
    public class BlogExtensionException: ApplicationException
    {
        public BlogExtensionException() { }
        public BlogExtensionException(string message) : base(message) { }
        public BlogExtensionException(string message, Exception baseException) : base(message, baseException) { }

        protected BlogExtensionException(SerializationInfo info, StreamingContext context) : 
            base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
