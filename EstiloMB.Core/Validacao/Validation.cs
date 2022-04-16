using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EstiloMB.Core
{
    public class Validation
    {
        public bool IsValid { get; set; }
        public int Count { get { return Errors.Count; } }
        public Error this[int index] { get { return Errors[index]; } }
        public string this[string property]
        {
            get
            {
                Error error = Errors.FirstOrDefault(e => e.Property == property);
                return error != null ? error.Message : null;
            }
        }

        public IList<Error> Errors { get; private set; }

        public Validation()
        {
            Errors = new List<Error>();
        }

        public void Add(string property, string message)
        {
            Errors.Add(new Error()
            {
                Property = property,
                Message = message
            });
        }

        public void Add(Error error)
        {
            Errors.Add(error);
        }

        public void Add<T>(string message, Expression<Func<T, object>> property)
        {
            Errors.Add(new Error()
            {
                Property = Utility.ParsePath(property),
                Message = message
            });
        }

        public JsonElement ToJsonElement()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                JsonWriterOptions options = new JsonWriterOptions
                {
                    Indented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };

                using (Utf8JsonWriter writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();

                    for (int i = 0; i < Errors.Count; i++)
                    {
                        string[] properties = Errors[i].Property.Split('.');
                        for (int j = 0; j < properties.Length - 1; j++)
                        {
                            writer.WriteStartObject(properties[i]);
                        }

                        writer.WriteString(properties[properties.Length - 1], Errors[i].Message);

                        for (int j = 0; j < properties.Length - 1; j++)
                        {
                            writer.WriteEndObject();
                        }
                    }

                    writer.WriteEndObject();
                }

                return JsonDocument.Parse(Encoding.UTF8.GetString(stream.ToArray())).RootElement;
            }
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();

            for (int i = 0; i < Errors.Count; i++)
            {
                JToken node = jObject;

                string[] properties = Errors[i].Property.Split('.');
                for (int j = 0; j < properties.Length; j++)
                {
                    JToken current = new JObject();

                    if (j + 1 < properties.Length)
                    {
                        ((JObject)node).Add(properties[j], current);
                    }
                    else
                    {
                        ((JObject)node).Add(properties[j], Errors[i].Message);
                    }

                    node = current;
                }
            }

            return jObject;
        }

        public static Validation ValidateProperty(object entity, string property)
        {
            IList<ValidationResult> results = new List<ValidationResult>();
            ValidationContext validationContext = new ValidationContext(entity);
            validationContext.MemberName = property;
            //validationContext.Items.Add("Values", new Dictionary<object, object>());
            //if (context != null) { validationContext.Items.Add("Database", context); }

            Validation validation = new Validation();
            validation.IsValid = Validator.TryValidateProperty(entity.GetType().GetProperty(property).GetValue(entity), validationContext, results);

            for (int i = 0; i < results.Count; i++)
            {
                IEnumerator<string> enumerator = results[i].MemberNames.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Error error = new Error();
                    error.Message = results[i].ErrorMessage;
                    error.Property = enumerator.Current;

                    validation.Errors.Add(error);
                }
            }

            return validation;
        }

        public static Validation Validate(object entity)
        {
            Validation validation = new Validation();
            Validate(entity, null, validation);
            validation.IsValid = validation.Errors.Count == 0;

            return validation;
        }

        private static void Validate(object entity, string path, Validation validation)
        {
            IList<ValidationResult> results = new List<ValidationResult>();
            ValidationContext context = new ValidationContext(entity);
            //context.Items.Add("Values", new Dictionary<object, object>());

            if (path != null) { context.Items.Add("Prefix", path); }
            //if (databaseContext != null) { context.Items.Add("Database", databaseContext); }
            //if (localContext != null) { context.Items.Add("Context", localContext); }
            Validator.TryValidateObject(entity, context, results, true);

            for (int i = 0; i < results.Count; i++)
            {
                IEnumerator<string> enumerator = results[i].MemberNames.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Error error = new Error();
                    error.Message = results[i].ErrorMessage;
                    error.Property = path + enumerator.Current;

                    validation.Errors.Add(error);
                }
            }

            PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].PropertyType.Equals(typeof(String)) || properties[i].PropertyType.Equals(typeof(Object))) { continue; }
                //if (properties[i].GetCustomAttribute<IgnoreValidation>() != null) { continue; }

                if (typeof(IEnumerable).IsAssignableFrom(properties[i].PropertyType) || properties[i].PropertyType.IsArray)
                {
                    //bool isCascade = properties[i].GetCustomAttribute<Cascade>() != null;
                    IList list = properties[i].GetValue(entity) as IList;
                    if (list == null || list.Count == 0) { continue; }

                    for (int j = 0; j < list.Count; j++)
                    {
                        Validate(list[j], path + properties[i].Name + "[" + j.ToString() + "].", validation);
                    }
                }
                else if (properties[i].PropertyType.GetTypeInfo().IsClass)
                {
                    object property = properties[i].GetValue(entity);
                    if (property == null) { continue; }

                    Validate(property, path + properties[i].Name + ".", validation);
                }
            }
        }
    }
}