// <copyright file="ErrorDetails.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace VianaHub.Global.Middleware.Lib.ExceptionHandling.Exceptions
{
    using System.Text.Json;

    /// <summary>
    /// Represents detailed information about an error occurrence, including HTTP status code,
    /// error message, correlation ID for tracking, and the machine where the error occurred.
    /// </summary>
    /// <param name="StatusCode">The HTTP status code associated with the error.</param>
    /// <param name="Message">A descriptive message explaining the error.</param>
    /// <param name="CorrelationId">A unique identifier for tracking the error across system components.</param>
    /// <param name="CurrentMachine">The name of the machine where the error occurred. If not provided, defaults to the current machine name.</param>
    public record ErrorDetails(int StatusCode, string Message, string CorrelationId, string? CurrentMachine = null)
    {
        /// <summary>
        /// Gets the name of the machine where the error occurred.
        /// If not explicitly provided in the constructor, defaults to the current machine's name.
        /// </summary>
        /// <value>The machine name where the error occurred.</value>
        public string CurrentMachine { get; init; } = CurrentMachine ?? Environment.MachineName;

        /// <summary>
        /// Returns a JSON string representation of the error details.
        /// This is useful for logging and debugging purposes.
        /// </summary>
        /// <returns>A JSON-formatted string containing all error details.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}