// <copyright file="GenericValidator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper methods to validate the response from other services.
    /// </summary>
    public static class GenericValidator
    {
        /// <summary>
        /// Validates that the entity is not null.
        /// </summary>
        /// <typeparam name="T">The type of the resources.</typeparam>
        /// <param name="response">The data to be validated.</param>
        /// <param name="validatorName">Name of Validator.</param>
        /// <param name="validationData">The validation data to return.</param>
        /// <param name="failureSeverity">The severity level of the validation if it fails.</param>
        /// <returns>True if the response is null, false otherwise.</returns>
        public static bool IsResponseNull<T>(T response, string validatorName, out ValidationData validationData, FailureSeverity failureSeverity = FailureSeverity.Error)
        {
            if (response == null)
            {
                validationData = new ValidationData(
                    validatorName: validatorName,
                    failureReason: $"The {nameof(T)} message being validated is null.",
                    failureSeverity: failureSeverity);
                return true;
            }

            validationData = new ValidationData();
            return false;
        }

        /// <summary>
        /// Validates if the identifier value is valid.
        /// </summary>
        /// <typeparam name="T">Type of the identifier.</typeparam>
        /// <param name="identifier">The identifier value to validate.</param>
        /// <param name="validatorName">Name of Validator.</param>
        /// <param name="validationData">The result of the validation.</param>
        /// <returns>True, if the identifier has valid value. False otherwise.</returns>
        public static bool HasInvalidIdentifier<T>(T identifier, string validatorName, out ValidationData validationData)
        {
            if ((identifier is int id && id <= 0) ||
                (identifier is long longId && longId <= 0) ||
                (identifier is string str && string.IsNullOrWhiteSpace(str)) ||
                (identifier is Guid guid && guid == Guid.Empty))
            {
                validationData = new ValidationData(
                    validatorName: validatorName,
                    failureReason: $"The given identifier does not have a valid value.");
                return true;
            }

            validationData = new ValidationData();
            return false;
        }

        /// <summary>
        /// Checks that a collection of resources is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the resources.</typeparam>
        /// <param name="resources">The resources provided.</param>
        /// <param name="validatorName">Name of Validator.</param>
        /// <param name="nameOfResources">The string representation of the resources.</param>
        /// <param name="validationData">The validation data that needs to be populated if validation fails.</param>
        /// <param name="failureSeverity">The severity of the validation error.</param>
        /// <returns>True if resources list is null or empty with lesser validation failure severity 'Warn', false otherwise.</returns>
        public static bool AreResourcesNullOrEmpty<T>(IList<T> resources, string validatorName, string nameOfResources, out ValidationData validationData, FailureSeverity failureSeverity = FailureSeverity.Error)
        {
            if (resources == null || !resources.Any())
            {
                validationData = new ValidationData(
                    validatorName: validatorName,
                    failureReason: $"The {nameOfResources} in the provided input is null or empty",
                    failureSeverity: failureSeverity);
                return true;
            }

            validationData = new ValidationData();
            return false;
        }

        /// <summary>
        /// Validates if a string is null or whitespace.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="validatorName">Name of Validator.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="validationData">The result of the validation.</param>
        /// <param name="failureSeverity">The severity level of the validation if it fails.</param>
        /// <returns>True if the string is null or whitespace, false otherwise.</returns>
        public static bool IsStringNullOrWhitespace(string value, string validatorName, string fieldName, out ValidationData validationData, FailureSeverity failureSeverity = FailureSeverity.Error)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                validationData = new ValidationData(
                    validatorName: validatorName,
                    failureReason: $"The {fieldName} field is null or whitespace.",
                    failureSeverity: failureSeverity);
                return true;
            }

            validationData = new ValidationData();
            return false;
        }

        /// <summary>
        /// Validates if a string exceeds maximum length.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="maxLength">The maximum allowed length.</param>
        /// <param name="validatorName">Name of Validator.</param>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="validationData">The result of the validation.</param>
        /// <param name="failureSeverity">The severity level of the validation if it fails.</param>
        /// <returns>True if the string exceeds maximum length, false otherwise.</returns>
        public static bool ExceedsMaxLength(string value, int maxLength, string validatorName, string fieldName, out ValidationData validationData, FailureSeverity failureSeverity = FailureSeverity.Error)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Length > maxLength)
            {
                validationData = new ValidationData(
                    validatorName: validatorName,
                    failureReason: $"The {fieldName} field exceeds the maximum length of {maxLength} characters.",
                    failureSeverity: failureSeverity);
                return true;
            }

            validationData = new ValidationData();
            return false;
        }
    }
}

