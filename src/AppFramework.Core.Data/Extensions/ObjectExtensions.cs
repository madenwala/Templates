namespace AppFramework.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static string SerializeToJson(this object obj)
        {
            // TODO implement JSON
            return null;

            //    var settings = new JsonSerializerSettings
            //    {
            //        NullValueHandling = NullValueHandling.Ignore,
            //        MissingMemberHandling = MissingMemberHandling.Ignore
            //    };
            //    return JsonConvert.DeserializeObject<T>(data, settings);
        }
    }
}