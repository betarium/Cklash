using System;
using System.Collections.Generic;

using System.Text.RegularExpressions;

/// zlib License
/// Copyright (c) 2015 Betarium
namespace Library
{
    public class JsonParser
    {

        public class JsonEntity
        {
            public enum JsonType
            {
                Unknown,
                Null,
                Boolean,
                Integer,
                Double,
                String,
                Array,
                Pear,
            }

            public string Name { get; set; }
            public string Value { get; set; }
            public JsonType Type { get; set; }
            public List<JsonEntity> Items { get; protected set; }
            public Dictionary<string, JsonEntity> Entities { get; protected set; }

            public JsonEntity()
            {
                Type = JsonType.Null;
                Name = "";
                Value = null;
                Items = new List<JsonEntity>();
                Entities = new Dictionary<string, JsonEntity>();
            }

            public JsonEntity(JsonType type)
                : this()
            {
                Type = type;
            }

            public JsonEntity(string value)
                : this()
            {
                Type = JsonType.String;
                Value = value;
            }

            public JsonEntity(bool value)
                : this()
            {
                Type = JsonType.Boolean;
                Value = value.ToString();
            }

            public JsonEntity(int value)
                : this()
            {
                Type = JsonType.Integer;
                Value = value.ToString();
            }

            public void Add(string key, JsonEntity entity)
            {
                Items.Add(entity);
                Entities.Add(key, entity);
            }
            public void Add(JsonEntity entity)
            {
                Items.Add(entity);
            }

            public string GetChildText(string key)
            {
                if (!Entities.ContainsKey(key))
                {
                    return null;
                }
                return Entities[key].Value;
            }
        }

        public class JsonException : Exception
        {
            public string SourceText { get; set; }
            public JsonException(string message)
                : base(message)
            {
            }
            public JsonException(string message, string source)
                : base(message)
            {
                SourceText = source;
            }
        }

        public static JsonEntity Parse(string text)
        {
            int index = 0;
            try
            {
                JsonEntity entity = Parse(0, text, ref index);
                return entity;
            }
            catch (IndexOutOfRangeException ex)
            {
                System.Diagnostics.Trace.WriteLine("JsonParser error:" + text);
                System.Diagnostics.Trace.WriteLine(ex);
                throw;
            }
        }

        protected static JsonEntity Parse(int startIndex, string text, ref int index)
        {
            int beforeIndex = index;
            while (index < text.Length)
            {
                char ch = text[index];
                if (ch == 'n')
                {
                    //Regex pattern = new Regex("(null)");
                    //Match match = pattern.Match(text, index);
                    //string part = match.Groups[1].Value;
                    //index += match.Length;
                    string part = text.Substring(index, 4);
                    if (part != "null")
                    {
                        throw new JsonException("Bad data", text);
                    }
                    index += 4;
                    JsonEntity entity = new JsonEntity();
                    return entity;
                }
                else if (ch == 't' || ch == 'f')
                {
                    Regex pattern = new Regex("(true|false)");
                    Match match = pattern.Match(text, index);
                    string part = match.Groups[1].Value;
                    index += match.Length;
                    JsonEntity entity = new JsonEntity(part);
                    return entity;
                }
                else if (ch >= '0' && ch <= '9' || ch == '-')
                {
                    Regex pattern = new Regex("(-?[0-9]+[\\.]?[0-9]*)");
                    Match match = pattern.Match(text, index);
                    if (!match.Success)
                    {
                        throw new JsonException("Bad format in number", text);
                    }
                    string part = match.Groups[1].Value;
                    index += match.Length;
                    JsonEntity entity = new JsonEntity(part);
                    return entity;
                }
                else if (ch == '\"')
                {
                    index++;
                    string textValue = "";
                    while (index < text.Length)
                    {
                        Regex pattern = new Regex("([^\"\\\\]*)");
                        Match match = pattern.Match(text, index);
                        string part = match.Groups[1].Value;
                        textValue += part;
                        index += match.Length;
                        if (index >= text.Length)
                        {
                            throw new JsonException("Bad format in text", text);
                        }
                        if (text[index] == '\"')
                        {
                            index++;
                            break;
                        }

                        if (text[index] != '\\')
                        {
                            throw new JsonException("Bad format in text", text);
                        }

                        if (text[index + 1] == '\"')
                        {
                            textValue += "\"";
                            index += 2;
                        }
                        else if (text[index + 1] == '/')
                        {
                            textValue += "/";
                            index += 2;
                        }
                        else if (text[index + 1] == 'r')
                        {
                            textValue += "\r";
                            index += 2;
                        }
                        else if (text[index + 1] == 'n')
                        {
                            textValue += "\n";
                            index += 2;
                        }
                        else if (text[index + 1] == 't')
                        {
                            textValue += "\t";
                            index += 2;
                        }
                        else if (text[index + 1] == 'u')
                        {
                            string part2 = text.Substring(index + 2, 4);
                            char char2 = (char)Convert.ToUInt32(part2, 16);
                            textValue += char2;
                            index += 6;
                        }
                        else
                        {
                            textValue += text.Substring(index, 2);
                            index += 2;
                        }
                    }


                    JsonEntity entity = new JsonEntity(textValue);
                    return entity;
                }
                else if (ch == '{')
                {
                    index++;

                    JsonEntity array = new JsonEntity(JsonEntity.JsonType.Pear);
                    while (index < text.Length)
                    {
                        char ch2 = text[index];
                        if (array.Items.Count > 0)
                        {
                            if (ch2 != ',')
                            {
                                break;
                            }
                            index++;
                        }
                        if (ch2 == '}')
                        {
                            break;
                        }
                        Regex pattern1 = new Regex("\"([^\"]*)\":");
                        Match match1 = pattern1.Match(text, index);
                        string key = match1.Groups[1].Value;
                        index += match1.Length;

                        startIndex = index;
                        JsonEntity entity = Parse(startIndex, text, ref index);
                        entity.Name = key;
                        array.Add(key, entity);
                    }

                    char ch3 = text[index];
                    if (ch3 != '}')
                    {
                        throw new JsonException("Invalid character after pear:" + ch3, text);
                    }
                    index++;

                    return array;
                }
                else if (ch == '[')
                {
                    index++;

                    JsonEntity array = new JsonEntity(JsonEntity.JsonType.Array);
                    while (index < text.Length)
                    {
                        if (array.Items.Count > 0)
                        {
                            char ch2 = text[index];
                            if (ch2 != ',')
                            {
                                break;
                            }
                            index++;
                        }
                        startIndex = index;
                        JsonEntity entity = Parse(startIndex, text, ref index);
                        if (entity == null)
                        {
                            break;
                        }
                        array.Add(entity);
                    }

                    char ch3 = text[index];
                    if (ch3 != ']')
                    {
                        throw new JsonException("Invalid character after array:" + ch3, text);
                    }
                    index++;

                    return array;
                }
                break;
            }
            if (index >= text.Length)
            {
                return null;
            }
            char ch4 = text[index];
            if (ch4 == ']' || ch4 == '}' || ch4 == ',')
            {
                return null;
            }
            throw new JsonException("Invalid format", text);
        }

        public static void Dump(JsonEntity entity)
        {
            Dump(entity, 0);
        }

        protected static void Dump(JsonEntity entity, int indent)
        {
            string tab = "";
            for (int i = 0; i < indent; i++)
            {
                tab += "\t";
            }
            System.Diagnostics.Debug.WriteLine(tab + "name:" + entity.Name + " item:" + entity.Value);
            if (entity.Type == JsonEntity.JsonType.Array || entity.Type == JsonParser.JsonEntity.JsonType.Pear)
            {
                foreach (var item in entity.Items)
                {
                    Dump(item, indent + 1);
                }
            }
        }
    }
}
