using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ChatCommunication
{
    public static class SerializerHelper
    {
        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer serializer = null;
            StringWriter writer = null;
            string serializedObject = String.Empty;

            if (toSerialize == null)
            {
                return string.Empty;
            }
            try
            {
                serializer = new XmlSerializer(typeof(T));
                writer = new StringWriter();
                serializer.Serialize(writer, toSerialize);
                serializedObject = writer.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during serialization.", ex);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return serializedObject;
        }

        public static T Deserialize<T>(this string toDeserialize)
        {
            XmlSerializer serializer = null;
            StringReader reader = null;
            T obj = default(T);

            try
            {
                serializer = new XmlSerializer(typeof(T));
                reader = new StringReader(toDeserialize);
                obj = (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during deserialization.", ex);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return obj;
        }
        public static void SerializeToXML<T>(this T toSerialize, string fileName)
        {
            XmlSerializer serializer = null;
            XmlWriter writer = null;

            try
            {
                serializer = new XmlSerializer(typeof(T));
                writer = XmlWriter.Create(fileName);
                serializer.Serialize(writer, toSerialize);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during serialization into file" + fileName + ".", ex);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T DeserializeFromXML<T>(string fileName)
        {
            XmlSerializer serializer = null;
            XmlReader reader = null;
            T obj = default(T);

            try
            {
                serializer = new XmlSerializer(typeof(T));
                reader = XmlReader.Create(fileName);
                obj = (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during deserialization from file" + fileName + ".", ex);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return obj;
        }
    }
}
