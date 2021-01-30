using CG.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CG.Linq.EFCore
{
    /// <summary>
    /// This class represents a factory for creating EFCore data-context instances.
    /// </summary>
    /// <typeparam name="TContext">The type of associated data-context</typeparam>
    /// <remarks>The idea, with this class, is to create a factory for making transitory 
    /// data-context instances, at runtime. The class is a better alternative to 
    /// passing a raw <see cref="IServiceProvider"/> instance around, or using a 
    /// service locator pattern.</remarks>
    public class DbContextFactory<TContext> 
        where TContext : DbContext
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property contains a reference to a service provider.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        #endregion

        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="DbContextFactory{TContext}"/>
        /// type.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use with the factory.</param>
        public DbContextFactory(
            IServiceProvider serviceProvider
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(serviceProvider, nameof(serviceProvider));

            // Save the references.
            ServiceProvider = serviceProvider;
        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method creates a new <typeparamref name="TContext"/> instance
        /// and returns it.
        /// </summary>
        /// <returns>A new <typeparamref name="TContext"/> instance.</returns>
        public virtual TContext Create()
        {
            // Create the data-context instance.
            var dbContext = ServiceProvider.GetRequiredService<TContext>();

            // Return the results.
            return dbContext;
        }

        #endregion
    }
}
