using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JsonSerializer
{
    [Serializable]
    public class Serializer
    {
        public static string Serialize(object objectToSerialize)
        {
            var builder = new StringBuilder();

            SerializeObject(objectToSerialize, builder);

            return builder.ToString();
        }

        private static void SerializeObject(object objectToSerialize, StringBuilder builder)
        {
            if (null == objectToSerialize)
            {
                builder.Append("null");

                return;
            }

            var atr = Attribute.GetCustomAttributes(objectToSerialize.GetType());

            if (atr.Any(at => at.GetType() != typeof(SerializableAttribute)))
            {
                throw new ArgumentException("Class is not serializable");
            }

            builder.Append("{\n");

            foreach (var field in objectToSerialize.GetType().GetFields()
                .Where(field => !Attribute.GetCustomAttributes(field)
                    .Any(attribute => attribute is NonSerializedAttribute)))
            {
                builder.Append("\t")
                    .Append("\"")
                    .Append(field.Name)
                    .Append("\"")
                    .Append(" : ");


                if (field.FieldType.IsArray)
                {
                    if (SerializeArray(objectToSerialize, builder, field))
                    {
                        continue;
                    }
                }
                else if (field.FieldType.IsPrimitive)
                {
                    SerializePrimitive(objectToSerialize, builder, field);
                }
                else if (field.FieldType == typeof(string))
                {
                    SerializeString(objectToSerialize, builder, field);
                }
                else
                {
                    SerializeObject(field.GetValue(objectToSerialize), builder);
                }

                builder.Append("\n");
            }

            builder.Append("}");
        }

        private static void SerializeString(object objectToSerialize, StringBuilder builder, FieldInfo field)
        {
            builder.Append("\"")
                .Append(field.GetValue(objectToSerialize))
                .Append("\"");
        }

        private static void SerializePrimitive(object objectToSerialize, StringBuilder builder, FieldInfo field)
        {
            if (field.FieldType == typeof(char))
            {
                builder.Append(field.GetValue(objectToSerialize))
                    .Append("\"")
                    .Append("\n");
            }
            else if (field.FieldType == typeof(bool))
            {
                builder.Append(field.GetValue(objectToSerialize)
                    .ToString()
                    .ToLower());
            }
            else
            {
                builder.Append(field.GetValue(objectToSerialize));
            }
        }

        private static bool SerializeArray(object objectToSerialize, StringBuilder builder, FieldInfo field)
        {
            var arr = field.GetValue(objectToSerialize) as Array;

            if (null == arr)
            {
                builder.Append("null")
                    .Append("\n");

                return true;
            }

            builder.Append("[");

            if (0 != arr.Length)
            {
                builder.Append(arr.GetValue(0));

                for (var i = 1; i < arr.Length; i++)
                {
                    builder.Append(", ")
                        .Append(arr.GetValue(i));
                }
            }

            builder.Append("]");
            return false;
        }
    }
}