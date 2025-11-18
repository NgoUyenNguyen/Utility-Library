using System.Threading;
using System.Threading.Tasks;

namespace NgoUyenNguyen.Behaviour
{
    /// <summary>
    /// Represents the different states or modes of an activity in the lifecycle of a behavior or process.
    /// </summary>
    public enum ActivityMode
    {
        Inactive,
        Activating,
        Active,
        Deactivating,
    }

    /// <summary>
    /// Defines the contract for an activity that can transition between different states in its lifecycle.
    /// Activities implementing this interface should manage transitions such as activation and deactivation,
    /// encapsulating specific behavior or logic.
    /// </summary>
    public interface IActivity
    {
        ActivityMode Mode { get; }
        Task ActivateAsync(CancellationToken ct);
        Task DeactivateAsync(CancellationToken ct);
    }

    /// <summary>
    /// Provides an abstract base implementation for activities capable of transitioning through
    /// various lifecycle states such as activation and deactivation.
    /// Derived classes represent specific activities with their respective behavior during these transitions.
    /// </summary>
    public abstract class Activity : IActivity
    {
        /// <summary>
        /// Gets the current operational state or mode of the activity represented by this property.
        /// </summary>
        /// <remarks>
        /// This property reflects the lifecycle phase of an activity, indicating whether it is inactive, activating, active, or deactivating.
        /// The possible states are defined by the <see cref="ActivityMode"/> enumeration.
        /// Transitions between these states occur when methods such as <c>ActivateAsync</c> or <c>DeactivateAsync</c> are invoked.
        /// </remarks>
        public ActivityMode Mode { get; protected set; } = ActivityMode.Inactive;

        /// <summary>
        /// Asynchronously activates the activity, transitioning its mode from Inactive to Active.
        /// </summary>
        /// <param name="ct">A CancellationToken that is used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public virtual async Task ActivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Inactive) return;

            Mode = ActivityMode.Activating;
            await Task.CompletedTask;
            Mode = ActivityMode.Active;
        }

        /// <summary>
        /// Asynchronously deactivates the activity, transitioning its mode from Active to Inactive.
        /// </summary>
        /// <param name="ct">A CancellationToken that is used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public virtual async Task DeactivateAsync(CancellationToken ct)
        {
            if (Mode != ActivityMode.Active) return;
            
            Mode = ActivityMode.Deactivating;
            await Task.CompletedTask;
            Mode = ActivityMode.Inactive;
        }
    }
}
