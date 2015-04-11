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
                Console.Out.WriteLine("An error occurred during serialization.\n" + ex.Message);
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
                Console.Out.WriteLine("An error occurred during deserialization.\n" + ex.Message);
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

                XmlWriterSettings closeOutput = new XmlWriterSettings();
                closeOutput.CloseOutput = true;

                writer = XmlWriter.Create(fileName, closeOutput);
                serializer.Serialize(writer, toSerialize);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("An error occurred during serialization to XML file " + fileName + ".\n" + ex.Message);
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

                XmlReaderSettings closeInput = new XmlReaderSettings();
                closeInput.CloseInput = true;

                reader = XmlReader.Create(fileName, closeInput);
                obj = (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("An error occurred during deserialization from XML file " + fileName + ".\n" + ex.Message);
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
