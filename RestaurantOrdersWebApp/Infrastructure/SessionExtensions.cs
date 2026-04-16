using System.Text.Json;

namespace RestaurantOrdersWebApp.Infrastructure
{
    public static class SessionExtensions
    {
        //метод расширения, чтобы добавлять списки в сессии
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}