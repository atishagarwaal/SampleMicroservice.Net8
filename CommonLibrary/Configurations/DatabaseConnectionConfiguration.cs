//-----------------------------------------------------------------------
// <copyright file="DatabaseConnectionConfiguration.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Configuration
{
    /// <summary>
    /// Configuration for database connection strings.
    /// </summary>
    public class DatabaseConnectionConfiguration
    {
        /// <summary>
        /// Gets or sets the default connection string.
        /// </summary>
        public string DefaultConnection { get; set; } = string.Empty;
    }
}

