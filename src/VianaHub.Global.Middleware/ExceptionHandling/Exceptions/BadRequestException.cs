// <copyright file="BadRequestException.cs" company="VianaHub">
// Copyright (c) VianaHub. All rights reserved.
// </copyright>
namespace EBL.FIG.Common.Middleware.Lib.ExceptionHandling.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a bad request occurs.
    /// This typically indicates that the request was malformed or invalid.
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="message">The error message that describes the reason for the exception.</param>
        public BadRequestException(string message) : base(message) { }
    }
}
