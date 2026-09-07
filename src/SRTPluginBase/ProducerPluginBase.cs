using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a producer that publishes <typeparamref name="TPayload"/> as JSON. Handles
/// serialisation and change detection; you implement <see cref="RefreshAsync"/>.
/// </summary>
/// <typeparam name="TPayload">
/// The payload type. Declare it in a separate dependency-free contract assembly so consumers can
/// reference the payload without referencing your plugin.
/// </typeparam>
public abstract class ProducerPluginBase<TPayload> : PluginBase, IProducerPlugin
    where TPayload : class
{
    private readonly ArrayBufferWriter<byte> scratch = new(initialCapacity: 4096);
    private byte[] previous = [];
    private int previousLength;

    /// <inheritdoc />
    public abstract PayloadChannelDescriptor Channel { get; }

    /// <inheritdoc />
    public abstract bool IsSourceAvailable { get; }

    /// <summary>
    /// Source-generated serialisation metadata for <typeparamref name="TPayload"/>, from a
    /// <see cref="System.Text.Json.Serialization.JsonSerializerContext"/> in the contract assembly.
    /// Required rather than optional: reflection-based serialisation would run on every tick.
    /// </summary>
    protected abstract JsonTypeInfo<TPayload> PayloadTypeInfo { get; }

    /// <summary>
    /// Whether to skip publishing when the serialised payload is identical to the previous one.
    /// On by default: a paused or menu-bound game otherwise republishes the same bytes 30 times a
    /// second for every subscriber.
    /// </summary>
    protected virtual bool SuppressUnchangedPayloads => true;

    /// <summary>
    /// Read the current state of the source.
    /// </summary>
    /// <returns><see langword="null"/> to skip this tick.</returns>
    protected abstract ValueTask<TPayload?> RefreshAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public async ValueTask<bool> TryProduceAsync(IBufferWriter<byte> destination, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);

        TPayload? payload = await RefreshAsync(cancellationToken).ConfigureAwait(false);
        if (payload is null)
            return false;

        // Serialise into a reusable scratch buffer first so the previous payload can be compared
        // before anything is handed to the transport.
        scratch.ResetWrittenCount();
        using (Utf8JsonWriter writer = new(scratch))
            JsonSerializer.Serialize(writer, payload, PayloadTypeInfo);

        ReadOnlySpan<byte> written = scratch.WrittenSpan;

        if (SuppressUnchangedPayloads && written.SequenceEqual(previous.AsSpan(0, previousLength)))
            return false;

        if (previous.Length < written.Length)
            previous = new byte[written.Length];
        written.CopyTo(previous);
        previousLength = written.Length;

        destination.Write(written);
        return true;
    }
}
