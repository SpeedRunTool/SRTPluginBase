using System.Buffers;

namespace SRTPluginBase.Abstractions;

/// <summary>
/// Describes the channel a producer publishes on. Consumers bind to channels by id, never to a
/// producer type.
/// </summary>
public sealed class PayloadChannelDescriptor
{
    /// <summary>
    /// Globally unique channel identifier, e.g. <c>srt/re4r/gamememory</c>. Consumers match on this.
    /// </summary>
    public required string ChannelId { get; init; }

    /// <summary>
    /// Version of the payload shape. Bump the major when you remove or repurpose a field; consumers
    /// declare the minimum they can read.
    /// </summary>
    public required Version ContractVersion { get; init; }

    /// <summary>How <see cref="PayloadFrame.Payload"/> is encoded.</summary>
    public PayloadCodec Codec { get; init; } = PayloadCodec.Json;

    /// <summary>
    /// Optional JSON Schema (2020-12) for the payload. Purely diagnostic: it lets the host's data
    /// inspector render a typed tree. Nothing depends on it being present or correct.
    /// </summary>
    public string? SchemaJson { get; init; }

    /// <summary>
    /// Hint that this channel would benefit from a direct producer-to-consumer pipe rather than
    /// being relayed through the host. Reserved; the host currently relays everything.
    /// </summary>
    public bool PreferDirect { get; init; }
}

/// <summary>A consumer's declared interest in a channel.</summary>
public sealed class ChannelSubscription
{
    /// <summary>
    /// The channel to subscribe to. <c>"*"</c> subscribes to every channel, which is how a generic
    /// consumer (a JSON writer, a web bridge) works without referencing any payload type.
    /// </summary>
    public required string ChannelId { get; init; }

    /// <summary>
    /// The oldest payload contract this consumer can read. The host refuses to wire up a producer
    /// advertising less than this, rather than letting it fail at deserialisation time.
    /// </summary>
    public Version MinimumContractVersion { get; init; } = new(1, 0);

    /// <summary>
    /// Whether the consumer is useless without this channel. When true, the host holds the consumer
    /// stopped until a matching producer is running.
    /// </summary>
    public bool Required { get; init; } = true;
}

/// <summary>One published payload, as delivered to a consumer.</summary>
public readonly struct PayloadFrame
{
    /// <summary>The channel this came from. Relevant when subscribed to more than one.</summary>
    public required string ChannelId { get; init; }

    /// <summary>
    /// Monotonically increasing per channel. The transport drops stale frames under load rather than
    /// queueing them, so sequence numbers can skip - that is normal, not an error.
    /// </summary>
    public required long Sequence { get; init; }

    /// <summary>Producer-side <see cref="System.Diagnostics.Stopwatch"/> timestamp, for latency measurement.</summary>
    public required long TimestampTicks { get; init; }

    /// <summary>How <see cref="Payload"/> is encoded.</summary>
    public required PayloadCodec Codec { get; init; }

    /// <summary>
    /// The encoded payload.
    /// </summary>
    /// <remarks>
    /// Valid only for the duration of the <see cref="IConsumerPlugin.ConsumeAsync"/> call. The buffer
    /// is pooled and reused immediately afterwards, so copy anything you intend to keep.
    /// </remarks>
    public required ReadOnlyMemory<byte> Payload { get; init; }
}

/// <summary>
/// Publishes payloads on a channel.
/// </summary>
public interface IProducerPlugin : IPlugin
{
    /// <summary>The channel this producer publishes on.</summary>
    PayloadChannelDescriptor Channel { get; }

    /// <summary>
    /// Whether the underlying source is currently readable - typically whether the game process is
    /// attached.
    /// </summary>
    /// <remarks>
    /// When false the host stops polling entirely and tells subscribers the channel has gone idle,
    /// so a producer with no game running costs no CPU. The 3.x host polled unconditionally.
    /// </remarks>
    bool IsSourceAvailable { get; }

    /// <summary>
    /// Write one payload into <paramref name="destination"/>.
    /// </summary>
    /// <returns>
    /// <see langword="false"/> to skip this tick - nothing changed, or the source was not ready.
    /// Skipping is cheaper than publishing a duplicate and is the expected result on a paused game.
    /// </returns>
    ValueTask<bool> TryProduceAsync(IBufferWriter<byte> destination, CancellationToken cancellationToken);
}

/// <summary>
/// Receives payloads from one or more channels.
/// </summary>
public interface IConsumerPlugin : IPlugin
{
    /// <summary>The channels this consumer wants.</summary>
    IReadOnlyList<ChannelSubscription> Subscriptions { get; }

    /// <summary>
    /// Handle one payload. Called on the runner's dispatch loop; return promptly. A consumer that
    /// blocks does not stall the producer - the host drops frames for a slow consumer rather than
    /// applying backpressure - but it will visibly lag.
    /// </summary>
    ValueTask ConsumeAsync(PayloadFrame frame, CancellationToken cancellationToken);

    /// <summary>
    /// The channel went away: the producer stopped, crashed, or its source closed. Clear any
    /// displayed state - a HUD showing the last frame of a closed game is worse than a blank one.
    /// </summary>
    ValueTask OnChannelClosedAsync(string channelId, CancellationToken cancellationToken);
}
