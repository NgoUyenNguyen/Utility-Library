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
        /// Propagation mode that delivers messages to subscribers in the child nodes
        /// of the current event bus within the hierarchy.
        /// </summary>
        Downstream,

        /// <summary>
        /// Propagation mode that prevents messages from propagating to any other
        /// event bus, ensuring that messages are delivered only to subscribers
        /// within the current event bus.
        /// </summary>
        Isolated
    }
}