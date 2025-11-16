//-----------------------------------------------------------------------
// <copyright file="Result{T}.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Results
{
    using System;

    /// <summary>
    /// Represents the result of an operation that can succeed or fail, with an optional value.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success.</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// Gets the value if the operation was successful.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Result{T}"/> class from being created.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the operation was successful.</param>
        /// <param name="value">The value if the operation was successful.</param>
        /// <param name="error">The error message if the operation failed.</param>
        private Result(bool isSuccess, T value, string? error)
            : base(isSuccess, error)
        {
            this.Value = value;
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A successful result with the value.</returns>
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A failed result.</returns>
        public static new Result<T> Failure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));
            }

            return new Result<T>(false, default!, error);
        }

        /// <summary>
        /// Implicitly converts a value to a successful result.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator Result<T>(T value)
        {
            return Success(value);
        }
    }
}

