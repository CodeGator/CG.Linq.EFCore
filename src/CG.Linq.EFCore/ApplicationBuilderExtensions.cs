using CG.Linq.EFCore.Repositories.Options;
using CG.Validations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="IApplicationBuilder"/>
    /// type.
    /// </summary>
    public static partial class ApplicationBuilderExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method performs any startup logic required by EFCore, such as 
        /// creating the underlying database (if needed), applying migrations,
        /// or adding seed data to a blank database. 
        /// </summary>
        /// <typeparam name="TContext">The type of assciated data-context</typeparam>
        /// <typeparam name="TOptions">The type of associated options.</typeparam>
        /// <param name="applicationBuilder">The application builder to use for 
        /// the operation.</param>
        /// <param name="hostEnvironment">The hosting environment to use for the
        /// operation.</param>
        /// <param name="seedDelegate">A delegate for seeding the database with 
        /// startup data.</param>
        /// <returns>The value of the <paramref name="applicationBuilder"/>
        /// parameter, for chaining calls together.</returns>
        /// <exception cref="ArgumentException">This exception is thrown whenever one
        /// or more arguments are invalid, or missing.</exception>
        public static IApplicationBuilder UseEFCoreStartup<TContext, TOptions>(
            this IApplicationBuilder applicationBuilder,
            IWebHostEnvironment hostEnvironment,
            Action<TContext> seedDelegate
            ) where TContext : DbContext
              where TOptions : EFCoreRepositoryOptions
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(applicationBuilder, nameof(applicationBuilder))
                .ThrowIfNull(hostEnvironment, nameof(hostEnvironment))
                .ThrowIfNull(seedDelegate, nameof(seedDelegate));

            // Get the registered options.
            var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<TOptions>>();

            // Should we manipulate the database?
            if (options.Value.ApplyMigrations ||
                options.Value.EnsureCreated ||
                options.Value.SeedDatabase)
            {
                // Apply any pending migrations.
                using (var scope = applicationBuilder.ApplicationServices.CreateScope())
                {
                    // Get a data-context instance.
                    var dataContext = scope.ServiceProvider.GetService<TContext>();

                    // Should we make sure the database exists?
                    if (options.Value.EnsureCreated)
                    {
                        // Ensure the database exists.
                        dataContext.Database.EnsureCreated();
                    }

                    // Should we make sure the migrations are up to date?
                    if (options.Value.ApplyMigrations)
                    {
                        // Apply migrations.
                        dataContext.Database.Migrate();
                    }

                    // Should we make the database has seed data?
                    if (options.Value.SeedDatabase)
                    {
                        // Perform the data seeding operation.
                        seedDelegate(dataContext);
                    }
                }
            }

            // Return the application builder.
            return applicationBuilder;
        }

        #endregion
    }
}