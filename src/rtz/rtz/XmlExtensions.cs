using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace rtz
{
    static class XmlExtensions
    {
        public static string OptionalAttributeString(this XElement element, string attributeName)
        {
            var att = element.Attribute(attributeName);

            if (att is null)
            {
                return null;
            }

            return att.Value;
        }

        public static DateTimeOffset? OptionalAttributeTime(this XElement element, string attributeName)
        {
            var att = element.Attribute(attributeName);

            if (att is null)
            {
                return null;
            }

            return (DateTimeOffset)att;
        }

       
    }
}
