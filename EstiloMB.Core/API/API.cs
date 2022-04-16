using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Text.Json;

namespace EstiloMB.Core
{
    public static class API
    {
        public static Response Redirect(string type, string function, int userID, string culture, JObject request)
        {
            Type requestedType = Type.GetType("EstiloMB.Core." + type + ", EstiloMB.Core");
            MethodInfo method = requestedType?.GetMethod(function);

            if (requestedType == null || method == null)
            {
                return new Response()
                {
                    Code = ResponseCode.BadRequest,
                    Message = "O tipo \"" + type + "\" não foi encontrado ou implementado corretamente."
                };
            }

            request["UserID"] = userID;

            Response response = (Response)method.Invoke(null, new[] { request.ToObject(typeof(Request<>).MakeGenericType(requestedType)) });

            if (response.Code == ResponseCode.Sucess)
            {
                return response;
            }

            Response<JObject> error = new Response<JObject>();
            error.Data = response.Validation?.ToJObject();
            error.Code = response.Code;
            error.Message = response.Message;

            return error;
        }

        public static Response Redirect(string type, string function, int userID, string culture, JsonElement request)
        {
            Type requestedType = Type.GetType("EstiloMB.Core." + type + ", EstiloMB.Core");
            MethodInfo method = requestedType?.GetMethod(function);

            if (requestedType == null || method == null)
            {
                return new Response()
                {
                    Code = ResponseCode.BadRequest,
                    Message = "O tipo \"" + type + "\" não foi encontrado ou implementado corretamente."
                };
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new StringToInt32Converter());
            options.Converters.Add(new StringToDecimalConverter());

            Request oRequest = (Request)JsonSerializer.Deserialize(request.GetRawText(), typeof(Request<>).MakeGenericType(requestedType), options);
            oRequest.UserID = userID;

            Response response = (Response)method.Invoke(null, new[] { oRequest });

            if (response.Code == ResponseCode.Sucess)
            {
                return response;
            }

            if (response.Validation != null)
            {
                Response<JsonElement> error = new Response<JsonElement>();
                error.Data = response.Validation.ToJsonElement();
                error.Code = response.Code;
                error.Message = response.Message;

                response = error;
            }

            return response;
        }
    }
}