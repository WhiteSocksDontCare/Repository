using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChatCommunication
{
    public static class SerializerHelper
    {
        public static string Serialize<T>(this T toSerialize)
        {
            if (toSerialize == null)
            {
                return string.Empty;
            }
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                serializer.Serialize(stringWriter, toSerialize);
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Une erreur est survenue lors de la sérialisation.", ex);
            }
        }

        public static T Deserialize<T>(this string toDeserialize)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T obj;
                using (TextReader reader = new StringReader(toDeserialize))
                {
                    obj = (T)serializer.Deserialize(reader);
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception("Une erreur est survenue lors de la désérialisation.", ex);
            }
        }
    }
}
