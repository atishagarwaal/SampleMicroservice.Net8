//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Results
{
    using System;

    /// <summary>
    /// Represents the result of an operation that can succeed or fail.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the error message if the operation failed.
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// Prevents a default instance of the <see cref="Result"/> class from being created.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the operation was successful.</param>
        /// <param name="error">The error message if the operation failed.</param>
        protected Result(bool isSuccess, string? error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException("A successful result cannot have an error message.");
            }

            if (!isSuccess && string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException("A failed result must have an error message.");
            }

            this.IsSuccess = isSuccess;
            this.Error = error;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success()
        {
            return new Result(true, null);
        }

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A failed result.</returns>
        public static Result Failure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));
            }

            return new Result(false, error);
        }

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A failed result.</returns>
        public static Result<T> Failure<T>(string error)
        {
            return Result<T>.Failure(error);
        }

        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A successful result with the value.</returns>
        public static Result<T> Success<T>(T value)
        {
            return Result<T>.Success(value);
        }
    }
}

