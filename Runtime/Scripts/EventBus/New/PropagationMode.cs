namespace NgoUyenNguyen
{
    /// <summary>
    /// Specifies the propagation behavior of messages within the event bus hierarchy.
    /// </summary>
    public enum PropagationMode
    {
        /// <summary>
        /// Propagation mode that delivers messages to subscribers in the parent nodes
        /// of the current event bus within the hierarchy.
        /// </summary>
        Upstream,

        /// <summary>
        /// Propagation mode that delivers messages to subscribers in the parent nodes
        /// of the current event bus within the hierarchy, excluding the current event bus itself.
        /// </summary>
        UpstreamIgnoreSelf,

        /// <summary>
        /// Propagation mode that delivers messages to subscribers in the child nodes
        /// of the current event bus within the hierarchy.
        /// </summary>
        Downstream,

        /// <summary>
        /// Propagation mode that delivers messages to subscribers in the child nodes
        /// of the current event bus within the hierarchy, excluding the current node itself.
        /// </summary>
        DownstreamIgnoreSelf,

        /// <summary>
        /// Propagation mode where messages remain confined to the current event bus
        /// without being delivered to any parent or child nodes within the hierarchy.
        /// </summary>
        Isolated
    }
}