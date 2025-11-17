// <copyright file="IConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces
{
    /// <summary>
    /// General purpose contract to convert from 'source' to 'target' type.
    /// </summary>
    /// <typeparam name="TSourceType">The source type to be converted.</typeparam>
    /// <typeparam name="TTargetType">The target type to convert the source type to.</typeparam>
    public interface IConverter<in TSourceType, out TTargetType>
    {
        /// <summary>
        /// Converts the source type to a specific target type object.
        /// </summary>
        /// <param name="sourceType">The source type to convert.</param>
        /// <returns>The converted target type object.</returns>
        TTargetType Convert(TSourceType sourceType);
    }
}

