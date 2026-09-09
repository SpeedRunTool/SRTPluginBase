using System.Buffers;
using System.Text.Json.Serialization.Metadata;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a producer that publishes <typeparamref name="TPayload"/> as JSON. Handles
/// serialisation and change detection; you implement <see cref="RefreshAsync"/>.
/// </summary>
/// <remarks>
/// If your producer also has user-editable settings, derive from
/// <see cref="ConfigurableProducerPluginBase{TPayload, TConfiguration}"/> instead - a type gets one
/// base class, so the combination has to be its own.
/// </remarks>
/// <typeparam name="TPayload">
/// The payload type. Declare it in a separate dependency-free contract assembly so consumers can
/// reference the payload without referencing your plugin.
/// </typeparam>
public abstract class ProducerPluginBase<TPayload> : PluginBase, IProducerPlugin
    where TPayload : class
{
    private readonly PayloadPublisher<TPayload> publisher = new();

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

        return publisher.TryWrite(payload, PayloadTypeInfo, destination, SuppressUnchangedPayloads);
    }
}
