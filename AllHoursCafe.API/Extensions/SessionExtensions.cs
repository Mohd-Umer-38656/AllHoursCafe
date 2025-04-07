using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AllHoursCafe.API.Extensions
{
    public static class SessionExtensions
    {
        // No need to redefine SetString as it's already an extension method in Microsoft.AspNetCore.Http

        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
