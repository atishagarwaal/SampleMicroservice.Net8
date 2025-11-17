// <copyright file="IMessageValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Validation.Interfaces
{
    using Retail.Api.Customers.src.CleanArchitecture.Application.Validation;

    /// <summary>
    /// The interface for validating if a message is valid.
    /// </summary>
    /// <typeparam name="TMessage">The message to be validated.</typeparam>
    public interface IMessageValidator<in TMessage>
    {
        /// <summary>
        /// Validates the given message and returns validation information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The validation data.</returns>
        ValidationData Validate(TMessage message);
    }
}

