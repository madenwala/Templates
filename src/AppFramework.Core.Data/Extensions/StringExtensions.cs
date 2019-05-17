namespace AppFramework.Core.Extensions
{
    public static class StringExtensions
    {
        public static T Deserialize<T>(this string data)
        {
            // TODO deserialize to Newtonsoft
            return default(T);
        }
    }
}