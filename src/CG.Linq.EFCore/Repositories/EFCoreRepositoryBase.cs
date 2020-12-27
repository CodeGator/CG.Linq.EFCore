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
        /// This property contains the data-context associated with the repository.
        /// </summary>
        protected TContext DataContext { get; set; }

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
        /// <param name="dataContext">The data-context to use with the repository.</param>
        protected EFCoreRepositoryBase(
            TOptions options,
            TContext dataContext
            ) 
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(options, nameof(options))
                .ThrowIfNull(dataContext, nameof(dataContext));

            // Save the references.
            DataContext = dataContext;
        }

        #endregion

        // *******************************************************************
        // Protected methods.
        // *******************************************************************

        #region Protected methods

        /// <summary>
        /// This method is called to clean up managed resources.
        /// </summary>
        /// <param name="disposing">True to cleanup managed resources.</param>
        protected override void Dispose(
            bool disposing
            )
        {
            // Should we cleanup managed resources?
            if (disposing)
            {
                DataContext?.Dispose();
            }

            // Give the base class a chance.
            base.Dispose(disposing);
        }

        #endregion
    }



    /// <summary>
    /// This class is a base EFCORE implementation of the <see cref="ILinqRepository{TModel}"/>
    /// interface.
    /// </summary>
    /// <typeparam name="TContext">The data-context type associated with the repository.</typeparam>
    /// <typeparam name="TOptions">The options type associated with the repository.</typeparam>
    /// <typeparam name="TModel">The model type associated with the repository.</typeparam>
    public abstract class EFCoreRepositoryBase<TContext, TOptions, TModel> :
        EFCoreRepositoryBase<TContext, TOptions>,
        ILinqRepository<TModel>
        where TModel : class, IModel
        where TOptions : IOptions<EFCoreRepositoryOptions>
        where TContext : DbContext
    {
        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="EFCoreRepositoryBase{TContext, TOptions, TModel}"/>
        /// class.
        /// </summary>
        /// <param name="options">The options to use with the repository.</param>
        /// <param name="dataContext">The data-context to use with the repository.</param>
        protected EFCoreRepositoryBase(
            TOptions options,
            TContext dataContext
            ) : base(options, dataContext)
        {

        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <inheritdoc />
        public virtual IQueryable<TModel> AsQueryable()
        {
            // Defer to the data-context.
            return DataContext.Set<TModel>().AsQueryable();
        }

        #endregion

        // *******************************************************************
        // Protected methods.
        // *******************************************************************

        #region Protected methods

        /// <summary>
        /// This method is called to clean up managed resources.
        /// </summary>
        /// <param name="disposing">True to cleanup managed resources.</param>
        protected override void Dispose(
            bool disposing
            )
        {
            // Should we cleanup managed resources?
            if (disposing)
            {
                DataContext?.Dispose();
            }

            // Give the base class a chance.
            base.Dispose(disposing);
        }

        #endregion
    }
}
