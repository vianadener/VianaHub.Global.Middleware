// <copyright file="ConflictException.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a conflict occurs.
    /// This is typically used for scenarios such as unique constraint violations 
    /// or resource state conflicts.
    /// </summary>
    public class ConflictException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        public ConflictException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the conflict.</param>
        public ConflictException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class with a specified 
        /// error message and a reference to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The error message that describes the conflict.</param>
        /// <param name="innerException">The inner exception that caused this exception.</param>
        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}
