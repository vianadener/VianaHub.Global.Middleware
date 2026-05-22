// <copyright file="NotFoundException.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace VianaHub.Global.Middleware.Lib.ExceptionHandling.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a requested resource is not found.
    /// This is typically used for scenarios where a lookup operation fails to locate 
    /// the specified entity or resource.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public NotFoundException(string message) : base(message) { }
    }
}
