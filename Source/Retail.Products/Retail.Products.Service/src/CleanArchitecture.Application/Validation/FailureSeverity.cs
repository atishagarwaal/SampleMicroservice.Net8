// <copyright file="FailureSeverity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Products.src.CleanArchitecture.Application.Validation
{
    /// <summary>
    /// Severity levels for errors raised during validation.
    /// </summary>
    public enum FailureSeverity
    {
        /// <summary>
        /// High severity. Exception raised, Error logged.
        /// </summary>
        Error,

        /// <summary>
        /// Medium severity. No Exception raised, Warning logged.
        /// </summary>
        Warn,

        /// <summary>
        /// Low severity. No Exception raised, Information logging not required.
        /// </summary>
        Info,
    }
}

