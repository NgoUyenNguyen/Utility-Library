using System;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents an abstract base timer providing functionality for starting, stopping,
    /// pausing, resuming, and tracking the progress of a timer.
    /// </summary>
    public abstract class Timer
    {
        /// <summary>
        /// Represents the initial duration of the timer in seconds.
        /// </summary>
        protected float InitialTime;

        /// <summary>
        /// Represents the current time value of the timer in seconds.
        /// </summary>
        protected float Time { get; set; }

        /// <summary>
        /// Indicates whether the timer is currently running.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Defines an action that is invoked when the timer starts.
        /// </summary>
        public event Action OnTimerStart = () => { };

        /// <summary>
        /// Defines an action that is invoked when the timer stops.
        /// </summary>
        public event Action OnTimerStop = () => { };

        /// <summary>
        /// Represents an abstract base timer providing core functionalities such as starting, stopping,
        /// pausing, resuming, and managing time. This class serves as the foundation
        /// for various types of timers.
        /// </summary>
        protected Timer(float value)
        {
            InitialTime = value;
            IsRunning = true;
        }

        /// <summary>
        /// Starts the timer by setting the current time to the initial duration and activating the timer.
        /// If the timer is already running, no action is performed. Triggers the associated start action.
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;
            Time = InitialTime;
            IsRunning = true;
            OnTimerStart();
        }

        /// <summary>
        /// Stops the timer by deactivating it and triggering the associated stop action.
        /// If the timer is already stopped, no action is performed.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnTimerStop();
        }

        /// <summary>
        /// Resumes the timer by setting its state to active.
        /// If the timer is already running, no action is performed.
        /// </summary>
        public void Resume() => IsRunning = true;

        /// <summary>
        /// Pauses the timer by deactivating it, allowing it to be resumed or restarted later.
        /// If the timer is already paused, no further action is performed.
        /// </summary>
        public void Pause() => IsRunning = false;
        public abstract void Reset();

        /// <summary>
        /// Updates the timer's progress by decreasing or increasing the elapsed time
        /// based on the provided deltaTime. Behavior depends on the implementation
        /// in derived classes (e.g., countdown or stopwatch functionality).
        /// </summary>
        /// <param name="deltaTime">The amount of time, in seconds, to adjust the timer's progress.
        /// For countdown timers, this typically decreases the remaining time, while for
        /// stopwatch timers, it increases the elapsed time.</param>
        public abstract void Tick(float deltaTime);
    }

    /// <summary>
    /// Represents a countdown timer that decrements time from an initial value towards zero.
    /// Provides functionality for managing the timer, such as starting, stopping, pausing,
    /// resuming, resetting with optional new values, and tracking progress or completion.
    /// </summary>
    public class CountdownTimer : Timer
    {
        /// <summary>
        /// Indicates whether the timer has completed, returning true if the timer's time has reached zero or below.
        /// </summary>
        public bool IsFinished => Time <= 0;
        
        /// <summary>
        /// Gets the progress of the timer as a value between 0 and 1,
        /// where 0 represents no progress and 1 represents full completion.
        /// </summary>
        public float Progress => 1 - Time / InitialTime;
        
        public CountdownTimer(float value) : base(value)
        {
        }

        /// <summary>
        /// Resets the timer to its initial starting state.
        /// When implemented, this method will restore the timer's current time
        /// to an appropriate default value, depending on the timer type.
        /// </summary>
        public override void Reset() => Time = InitialTime;

        /// <summary>
        /// Resets the timer to its initial state or a newly provided value.
        /// This method allows the timer to start over with a new duration, if specified,
        /// and resets its progress and state accordingly.
        /// </summary>
        /// <param name="newTime">The new initial time value to set for the timer, replacing the previous value.</param>
        public void Reset(float newTime)
        {
            InitialTime = newTime;
            Reset();
        }

        
        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time > 0)
            {
                Time -= deltaTime;
            }
            if (IsRunning && Time <= 0) Stop();
        }
    }

    /// <summary>
    /// Represents a stopwatch timer that measures elapsed time by incrementing from zero.
    /// Provides functionality for starting, stopping, pausing, resuming, and resetting
    /// the timer. The stopwatch timer tracks the elapsed time in progressive increments
    /// during operation.
    /// </summary>
    public class StopwatchTimer : Timer
    {
        public StopwatchTimer() : base(0)
        {
        }

        /// <summary>
        /// Resets the timer to its initial state. For countdown timers, it sets the time to the initial value.
        /// For stopwatch timers, it sets the time to zero.
        /// </summary>
        public override void Reset() => Time = 0;

        public override void Tick(float deltaTime)
        {
            if (IsRunning)
            {
                Time += deltaTime;
            }
        }
    }
}
