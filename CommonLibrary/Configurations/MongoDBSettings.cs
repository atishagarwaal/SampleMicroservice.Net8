//-----------------------------------------------------------------------
// <copyright file="MongoDBSettings.cs" company="<Your Company>">
// Copyright (c) <Your Company>. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CommonLibrary.Configuration
{
    /// <summary>
    /// Configuration settings for MongoDB.
    /// </summary>
    public class MongoDBSettings
    {
        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}

