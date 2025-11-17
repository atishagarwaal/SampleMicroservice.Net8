namespace Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces
{
    using CommonLibrary.Results;
    using InventoryUpdatedEventNameSpace;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;

    /// <summary>
    /// Interface definition for customer service.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Method to fetch all customers asynchronously.
        /// </summary>
        /// <returns>List of customers.</returns>
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();

        /// <summary>
        /// Method to fetch customer record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Result containing the customer object if found, or an error message if not found.</returns>
        Task<Result<CustomerDto>> GetCustomerByIdAsync(long id);

        /// <summary>
        /// Method to add a new customer record asynchronously.
        /// </summary>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Result containing the created customer object if successful, or an error message if validation fails.</returns>
        Task<Result<CustomerDto>> AddCustomerAsync(CustomerDto custDto);

        /// <summary>
        /// Method to update customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Result containing the updated customer object if successful, or an error message if validation fails or customer not found.</returns>
        Task<Result<CustomerDto>> UpdateCustomerAsync(long id, CustomerDto custDto);

        /// <summary>
        /// Method to delete customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>True if customer was deleted, false if not found.</returns>
        Task<bool> DeleteCustomerAsync(long id);

        Task HandleOrderCreatedEvent(InventoryUpdatedEvent inventoryUpdatedEvent);
    }
}
