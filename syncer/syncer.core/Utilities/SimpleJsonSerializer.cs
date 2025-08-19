using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace syncer.core.Utilities
{
    /// <summary>
    /// Simple JSON serializer/deserializer for .NET 3.5 compatibility
    /// Handles basic types, objects, arrays, and nested structures
    /// </summary>
    public static class SimpleJsonSerializer
    {
        /// <summary>
        /// Serialize an object to JSON string
        /// </summary>
        public static string Serialize(object obj)
        {
            if (obj == null) return "null";
            
            var sb = new StringBuilder();
            SerializeValue(obj, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Deserialize JSON string to specified type
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            
            var reader = new JsonReader(json);
            return (T)DeserializeValue(typeof(T), reader);
        }

        /// <summary>
        /// Try to deserialize with error handling
        /// </summary>
        public static bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        private static void SerializeValue(object obj, StringBuilder sb)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            var type = obj.GetType();

            if (type == typeof(string))
            {
                SerializeString((string)obj, sb);
            }
            else if (type == typeof(bool))
            {
                sb.Append(((bool)obj) ? "true" : "false");
            }
            else if (IsNumericType(type))
            {
                sb.Append(obj.ToString());
            }
            else if (type == typeof(DateTime))
            {
                SerializeString(((DateTime)obj).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), sb);
            }
            else if (type == typeof(DateTime?))
            {
                var dt = (DateTime?)obj;
                if (dt.HasValue)
                    SerializeString(dt.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), sb);
                else
                    sb.Append("null");
            }
            else if (type == typeof(TimeSpan))
            {
                SerializeString(((TimeSpan)obj).ToString(), sb);
            }
            else if (type.IsEnum)
            {
                SerializeString(obj.ToString(), sb);
            }
            else if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                SerializeArray(obj, sb);
            }
            else
            {
                SerializeObject(obj, sb);
            }
        }

        private static void SerializeString(string str, StringBuilder sb)
        {
            sb.Append('"');
            foreach (char c in str)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        private static void SerializeArray(object array, StringBuilder sb)
        {
            sb.Append('[');
            
            var enumerable = array as System.Collections.IEnumerable;
            bool first = true;
            
            foreach (object item in enumerable)
            {
                if (!first) sb.Append(',');
                SerializeValue(item, sb);
                first = false;
            }
            
            sb.Append(']');
        }

        private static void SerializeObject(object obj, StringBuilder sb)
        {
            sb.Append('{');
            
            var properties = obj.GetType().GetProperties();
            bool first = true;
            
            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;
                
                if (!first) sb.Append(',');
                
                SerializeString(prop.Name, sb);
                sb.Append(':');
                SerializeValue(prop.GetValue(obj, null), sb);
                
                first = false;
            }
            
            sb.Append('}');
        }

        private static bool IsNumericType(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) ||
                   type == typeof(short) || type == typeof(ushort) ||
                   type == typeof(int) || type == typeof(uint) ||
                   type == typeof(long) || type == typeof(ulong) ||
                   type == typeof(float) || type == typeof(double) ||
                   type == typeof(decimal);
        }

        private static object DeserializeValue(Type targetType, JsonReader reader)
        {
            reader.SkipWhitespace();
            
            if (reader.Peek() == 'n') // null
            {
                reader.ReadString(4); // "null"
                return null;
            }
            
            if (targetType == typeof(string))
            {
                return reader.ReadString();
            }
            else if (targetType == typeof(bool))
            {
                return reader.ReadBoolean();
            }
            else if (IsNumericType(targetType))
            {
                return reader.ReadNumber(targetType);
            }
            else if (targetType == typeof(DateTime))
            {
                var str = reader.ReadString();
                return DateTime.Parse(str);
            }
            else if (targetType == typeof(DateTime?))
            {
                if (reader.Peek() == 'n')
                {
                    reader.ReadString(4); // "null"
                    return null;
                }
                var str = reader.ReadString();
                return DateTime.Parse(str);
            }
            else if (targetType == typeof(TimeSpan))
            {
                var str = reader.ReadString();
                return TimeSpan.Parse(str);
            }
            else if (targetType.IsEnum)
            {
                var str = reader.ReadString();
                return Enum.Parse(targetType, str);
            }
            else if (targetType.IsArray)
            {
                return DeserializeArray(targetType, reader);
            }
            else if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return DeserializeList(targetType, reader);
            }
            else
            {
                return DeserializeObject(targetType, reader);
            }
        }

        private static object DeserializeArray(Type arrayType, JsonReader reader)
        {
            var elementType = arrayType.GetElementType();
            var list = new List<object>();
            
            reader.ReadChar(); // '['
            reader.SkipWhitespace();
            
            if (reader.Peek() == ']')
            {
                reader.ReadChar();
                var array = Array.CreateInstance(elementType, 0);
                return array;
            }
            
            while (true)
            {
                var item = DeserializeValue(elementType, reader);
                list.Add(item);
                
                reader.SkipWhitespace();
                var next = reader.Peek();
                
                if (next == ']')
                {
                    reader.ReadChar();
                    break;
                }
                else if (next == ',')
                {
                    reader.ReadChar();
                    reader.SkipWhitespace();
                }
                else
                {
                    throw new InvalidOperationException("Invalid JSON array format");
                }
            }
            
            var result = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                result.SetValue(list[i], i);
            }
            
            return result;
        }

        private static object DeserializeList(Type listType, JsonReader reader)
        {
            var elementType = listType.GetGenericArguments()[0];
            var listInstance = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");
            
            reader.ReadChar(); // '['
            reader.SkipWhitespace();
            
            if (reader.Peek() == ']')
            {
                reader.ReadChar();
                return listInstance;
            }
            
            while (true)
            {
                var item = DeserializeValue(elementType, reader);
                addMethod.Invoke(listInstance, new object[] { item });
                
                reader.SkipWhitespace();
                var next = reader.Peek();
                
                if (next == ']')
                {
                    reader.ReadChar();
                    break;
                }
                else if (next == ',')
                {
                    reader.ReadChar();
                    reader.SkipWhitespace();
                }
                else
                {
                    throw new InvalidOperationException("Invalid JSON array format");
                }
            }
            
            return listInstance;
        }

        private static object DeserializeObject(Type objectType, JsonReader reader)
        {
            var instance = Activator.CreateInstance(objectType);
            var properties = objectType.GetProperties();
            
            reader.ReadChar(); // '{'
            reader.SkipWhitespace();
            
            if (reader.Peek() == '}')
            {
                reader.ReadChar();
                return instance;
            }
            
            while (true)
            {
                var propertyName = reader.ReadString();
                reader.SkipWhitespace();
                reader.ReadChar(); // ':'
                reader.SkipWhitespace();
                
                // Find matching property
                foreach (var prop in properties)
                {
                    if (prop.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase) && prop.CanWrite)
                    {
                        var value = DeserializeValue(prop.PropertyType, reader);
                        prop.SetValue(instance, value, null);
                        break;
                    }
                }
                
                reader.SkipWhitespace();
                var next = reader.Peek();
                
                if (next == '}')
                {
                    reader.ReadChar();
                    break;
                }
                else if (next == ',')
                {
                    reader.ReadChar();
                    reader.SkipWhitespace();
                }
                else
                {
                    throw new InvalidOperationException("Invalid JSON object format");
                }
            }
            
            return instance;
        }
    }

    /// <summary>
    /// Simple JSON reader for parsing JSON strings
    /// </summary>
    internal class JsonReader
    {
        private readonly string _json;
        private int _position;

        public JsonReader(string json)
        {
            _json = json ?? throw new ArgumentNullException("json");
            _position = 0;
        }

        public char Peek()
        {
            SkipWhitespace();
            return _position < _json.Length ? _json[_position] : '\0';
        }

        public char ReadChar()
        {
            return _position < _json.Length ? _json[_position++] : '\0';
        }

        public string ReadString(int length)
        {
            var result = _json.Substring(_position, Math.Min(length, _json.Length - _position));
            _position += length;
            return result;
        }

        public string ReadString()
        {
            SkipWhitespace();
            
            if (ReadChar() != '"')
                throw new InvalidOperationException("Expected string start quote");
            
            var sb = new StringBuilder();
            
            while (_position < _json.Length)
            {
                var c = ReadChar();
                
                if (c == '"')
                    break;
                    
                if (c == '\\')
                {
                    var escaped = ReadChar();
                    switch (escaped)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            var hex = ReadString(4);
                            var unicode = int.Parse(hex, NumberStyles.HexNumber);
                            sb.Append((char)unicode);
                            break;
                        default:
                            sb.Append(escaped);
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            
            return sb.ToString();
        }

        public bool ReadBoolean()
        {
            SkipWhitespace();
            
            if (Peek() == 't')
            {
                ReadString(4); // "true"
                return true;
            }
            else if (Peek() == 'f')
            {
                ReadString(5); // "false"
                return false;
            }
            else
            {
                throw new InvalidOperationException("Expected boolean value");
            }
        }

        public object ReadNumber(Type targetType)
        {
            SkipWhitespace();
            
            var sb = new StringBuilder();
            
            while (_position < _json.Length)
            {
                var c = Peek();
                if (char.IsDigit(c) || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                {
                    sb.Append(ReadChar());
                }
                else
                {
                    break;
                }
            }
            
            var numberStr = sb.ToString();
            
            if (targetType == typeof(byte)) return byte.Parse(numberStr);
            if (targetType == typeof(sbyte)) return sbyte.Parse(numberStr);
            if (targetType == typeof(short)) return short.Parse(numberStr);
            if (targetType == typeof(ushort)) return ushort.Parse(numberStr);
            if (targetType == typeof(int)) return int.Parse(numberStr);
            if (targetType == typeof(uint)) return uint.Parse(numberStr);
            if (targetType == typeof(long)) return long.Parse(numberStr);
            if (targetType == typeof(ulong)) return ulong.Parse(numberStr);
            if (targetType == typeof(float)) return float.Parse(numberStr);
            if (targetType == typeof(double)) return double.Parse(numberStr);
            if (targetType == typeof(decimal)) return decimal.Parse(numberStr);
            
            throw new InvalidOperationException("Unsupported numeric type: " + targetType);
        }

        public void SkipWhitespace()
        {
            while (_position < _json.Length && char.IsWhiteSpace(_json[_position]))
            {
                _position++;
            }
        }
    }
}
