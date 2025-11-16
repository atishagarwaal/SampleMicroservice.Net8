// <copyright file="ValidationData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Validation
{
    /// <summary>
    /// Validation data returned by message validators.
    /// </summary>
    public class ValidationData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationData" /> class for validation success.
        /// </summary>
        public ValidationData()
        {
            this.IsValid = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationData" /> class for validation failure.
        /// </summary>
        /// <param name="validatorName">The name of the validator.</param>
        /// <param name="failureReason">The reason the validator failed.</param>
        /// <param name="failureSeverity">The severity of the validator failure.</param>
        public ValidationData(string validatorName, string failureReason, FailureSeverity failureSeverity = FailureSeverity.Error)
        {
            if (string.IsNullOrWhiteSpace(validatorName))
            {
                throw new ArgumentNullException(nameof(validatorName));
            }

            if (string.IsNullOrWhiteSpace(failureReason))
            {
                throw new ArgumentNullException(nameof(failureReason));
            }

            this.ValidatorName = validatorName;
            this.IsValid = false;
            this.FailureReason = failureReason;
            this.FailureSeverity = failureSeverity;
        }

        /// <summary>
        /// Gets a value indicating whether validation was successful.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets name of the validator that was unsuccessful.
        /// </summary>
        public string? ValidatorName { get; }

        /// <summary>
        /// Gets reason validation failed.
        /// </summary>
        public string? FailureReason { get; }

        /// <summary>
        /// Gets severity of the validation error.
        /// </summary>
        public FailureSeverity FailureSeverity { get; }
    }
}

