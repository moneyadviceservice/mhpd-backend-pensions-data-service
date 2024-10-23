using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace MhpdCommon.Repository
{
    internal class MhpdCosmosSerializer(JsonSerializerOptions jsonSerializerOptions) : CosmosSerializer
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions;

        public override T FromStream<T>(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                T? result = JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions).GetAwaiter().GetResult();

                return result ?? throw new InvalidOperationException("Deserialization of data from Cosmos Db resulted in a null object.");
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream stream = new();
            JsonSerializer.SerializeAsync(stream, input, _jsonSerializerOptions).GetAwaiter().GetResult();
            stream.Position = 0;
            return stream;
        }
    }
}
