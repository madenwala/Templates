using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace AppFramework.Core.Extensions
{
    public static class IStorageFileExtensions
    {
        public static async Task<string> ReadAllTextAsync(this IStorageFile file)
        {
            // Read the whole file as a string.
            string contents = null;
            using (IRandomAccessStream textStream = await file.OpenReadAsync())
            {
                using (DataReader textReader = new DataReader(textStream))
                {
                    uint textLength = (uint)textStream.Size;
                    await textReader.LoadAsync(textLength);
                    contents = textReader.ReadString(textLength);
                }
            }
            return contents;
        }
    }
}