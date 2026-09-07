using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SRTPluginBase.Abstractions;

namespace SRTPluginBase;

/// <summary>
/// Base class for a consumer that reads a single JSON payload type. Handles deserialisation; you
/// implement <see cref="OnPayloadAsync"/>.
/// </summary>
/// <typeparam name="TPayload">
/// The payload type, from the producer's contract assembly. Note this is a reference to the payload
/// contract only - never to the producer plugin itself.
/// </typeparam>
public abstract class ConsumerPluginBase<TPayload> : PluginBase, IConsumerPlugin
    where TPayload : class
{
    /// <inheritdoc />
    public abstract IReadOnlyList<ChannelSubscription> Subscriptions { get; }

    /// <summary>
    /// Source-generated serialisation metadata for <typeparamref name="TPayload"/>, from the
    /// contract assembly's <see cref="System.Text.Json.Serialization.JsonSerializerContext"/>.
    /// </summary>
    protected abstract JsonTypeInfo<TPayload> PayloadTypeInfo { get; }

    /// <summary>Handle one decoded payload.</summary>
    protected abstract ValueTask OnPayloadAsync(TPayload payload, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual ValueTask OnChannelClosedAsync(string channelId, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public async ValueTask ConsumeAsync(PayloadFrame frame, CancellationToken cancellationToken)
    {
        // frame.Payload points into a pooled buffer that is recycled as soon as this returns, so
        // deserialise before awaiting anything.
        TPayload? payload = JsonSerializer.Deserialize(frame.Payload.Span, PayloadTypeInfo);
        if (payload is null)
            return;

        await OnPayloadAsync(payload, cancellationToken).ConfigureAwait(false);
    }
}
