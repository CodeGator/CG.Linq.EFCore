using CG.Linq.EFCore.Repositories.Options;
using CG.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// This delegate type represents a callback to seed a database.
    /// </summary>
    /// <typeparam name="TContext">The type of associated data-context.</typeparam>
    /// <param name="dataContext">The data-context to use for the operation.</param>
    /// <param name="wasDropped">Indicates whether the data-context was recently dropped.</param>
    /// <param name="wasMigrated">Indicates whether the data-context was recently migrated.</param>
    public delegate void SeedAction<in TContext>(
        TContext dataContext,
        bool wasDropped,
        bool wasMigrated
        ) where TContext : DbContext;


    /// <summary>
    /// This class contains extension methods related to the <see cref="IApplicationBuilder"/>
    /// type, for registering types related to the EFCore library.
    /// </summary>
    public static partial class EfCoreApplicationBuilderExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method performs any startup logic required by EFCore, such as 
        /// dropping the underlying database (if needed), or creating the underlying 
        /// database (if needed), applying any pending migrations, or adding seed 
        /// data to an otherwise blank database. 
        /// </summary>
        /// <typeparam name="TContext">The type of assciated data-context</typeparam>
        /// <typeparam name="TOptions">The type of associated options.</typeparam>
        /// <param name="applicationBuilder">The application builder to use for 
        /// the operation.</param>
        /// <param name="seedDelegate">A delegate for seeding the database with 
        /// startup data.</param>
        /// <returns>The value of the <paramref name="applicationBuilder"/>
        /// parameter, for chaining calls together.</returns>
        /// <exception cref="ArgumentException">This exception is thrown whenever one
        /// or more arguments are invalid, or missing.</exception>
        public static IApplicationBuilder UseEFCore<TContext, TOptions>(
            this IApplicationBuilder applicationBuilder,
            SeedAction<TContext> seedDelegate
            ) where TContext : DbContext
              where TOptions : EFCoreRepositoryOptions
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(applicationBuilder, nameof(applicationBuilder))
                .ThrowIfNull(seedDelegate, nameof(seedDelegate));

            // Get the registered options.
            var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<TOptions>>();

            var wasDropped = false;
            var wasMigrated = false;

            // Should we manipulate the database?
            if (options.Value.ApplyMigrations ||
                options.Value.DropDatabase ||
                options.Value.SeedDatabase)
            {
                // Apply any pending migrations.
                using var scope = applicationBuilder.ApplicationServices.CreateScope();

                // Get a data-context instance.
                var dataContext = scope.ServiceProvider.GetService<TContext>();

                // Should we drop the database? 
                if (options.Value.DropDatabase)
                {
                    // Ensure the database exists.
                    dataContext.Database.EnsureDeleted();

                    // Keep track of what we've done.
                    wasDropped = true;
                }

                // Should we create the database and apply migrations?
                if (options.Value.ApplyMigrations || options.Value.DropDatabase)
                {
                    // Apply migrations only.
                    dataContext.Database.Migrate();

                    // Keep track of what we've done.
                    wasMigrated = true;
                }

                // Should we make sure the database has data?
                if (options.Value.SeedDatabase)
                {
                    // Perform the data seeding operation.
                    seedDelegate(
                        dataContext,
                        wasDropped,
                        wasMigrated
                        );
                }
            }

            // Return the application builder.
            return applicationBuilder;
        }

        #endregion
    }
}