using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SRTPluginBase;

/// <summary>
/// Serialises a producer's payload and decides whether it is worth publishing.
/// </summary>
/// <remarks>
/// A composed helper for the same reason <see cref="PluginConfiguration{TConfiguration}"/> is one: a
/// producer may or may not also be configurable, and the two base classes that result must not each
/// carry their own copy of the change-detection buffers. Getting that duplicated wrong is silent -
/// one of the two would republish unchanged bytes 30 times a second and nothing would look broken.
/// </remarks>
/// <typeparam name="TPayload">The payload type.</typeparam>
public sealed class PayloadPublisher<TPayload>
    where TPayload : class
{
    private readonly ArrayBufferWriter<byte> scratch = new(initialCapacity: 4096);
    private byte[] previous = [];
    private int previousLength;

    /// <summary>
    /// Serialises <paramref name="payload"/> into <paramref name="destination"/>.
    /// </summary>
    /// <param name="payload">The payload, or null to skip this tick.</param>
    /// <param name="typeInfo">Source-generated metadata for the payload type.</param>
    /// <param name="destination">Where the bytes go when there is something to publish.</param>
    /// <param name="suppressUnchanged">
    /// Skip the tick when the bytes are identical to the previous ones. A paused or menu-bound game
    /// otherwise republishes the same payload to every subscriber, every tick.
    /// </param>
    /// <returns>Whether anything was written.</returns>
    public bool TryWrite(
        TPayload? payload,
        JsonTypeInfo<TPayload> typeInfo,
        IBufferWriter<byte> destination,
        bool suppressUnchanged)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        ArgumentNullException.ThrowIfNull(destination);

        if (payload is null)
            return false;

        // Serialised into a reusable scratch buffer first so the previous payload can be compared
        // before anything is handed to the transport.
        scratch.ResetWrittenCount();

        using (Utf8JsonWriter writer = new(scratch))
            JsonSerializer.Serialize(writer, payload, typeInfo);

        ReadOnlySpan<byte> written = scratch.WrittenSpan;

        if (suppressUnchanged && written.SequenceEqual(previous.AsSpan(0, previousLength)))
            return false;

        if (previous.Length < written.Length)
            previous = new byte[written.Length];

        written.CopyTo(previous);
        previousLength = written.Length;

        destination.Write(written);
        return true;
    }
}
