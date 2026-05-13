namespace WahadiniCryptoQuest.Core.Enums;

/// <summary>
/// Represents the completion status of a course enrollment
/// </summary>
public enum CompletionStatus
{
    /// <summary>
    /// Course not yet started (0% progress)
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Course in progress (1-99% progress)
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Course completed (100% progress)
    /// </summary>
    Completed = 2
}
