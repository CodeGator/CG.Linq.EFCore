using CG.Business.Models;
using CG.Business.Repositories;
using CG.Linq.EFCore.Repositories.Options;
using CG.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;

namespace CG.Linq.EFCore.Repositories
{
    /// <summary>
    /// This class is a base EFCORE implementation of the <see cref="IRepository"/>
    /// interface.
    /// </summary>
    /// <typeparam name="TContext">The data-context type associated with the repository.</typeparam>
    /// <typeparam name="TOptions">The options type associated with the repository.</typeparam>
    public abstract class EFCoreRepositoryBase<TContext, TOptions> :
        RepositoryBase,
        IRepository
        where TOptions : IOptions<EFCoreRepositoryOptions>
        where TContext : DbContext
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property contains a factory for creating <typeparamref name="TContext"/>
        /// instances.
        /// </summary>
        protected DbContextFactory<TContext> Factory { get; }

        #endregion

        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="EFCoreRepositoryBase{TContext, TOptions}"/>
        /// class. 
        /// </summary>
        /// <param name="options">The options to use with the repository.</param>
        /// <param name="dbContextFactory">The data-context factory to use with 
        /// the repository.</param>
        /// <remarks>
        /// Use this constructor when the repostitory requires a transient 
        /// data-context instance that should be created, used, and disposed
        /// of, for each operation.
        /// </remarks>
        protected EFCoreRepositoryBase(
            TOptions options,
            DbContextFactory<TContext> dbContextFactory
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(options, nameof(options))
                .ThrowIfNull(dbContextFactory, nameof(dbContextFactory));

            // Save the references.
            Factory = dbContextFactory;
        }

        #endregion
    }
}
