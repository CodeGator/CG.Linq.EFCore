using CG.Business.Models;
using CG.Business.Repositories;
using CG.Linq.EFCore.Properties;
using CG.Linq.EFCore.Repositories.Options;
using CG.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CG.Linq.EFCore.Repositories
{
    /// <summary>
    /// This class is a base EFCORE implementation of the <see cref="ICrudRepository{TModel, TKey}"/>
    /// interface.
    /// </summary>
    /// <typeparam name="TContext">The data-context type associated with the repository.</typeparam>
    /// <typeparam name="TOptions">The options type associated with the repository.</typeparam>
    /// <typeparam name="TModel">The type of associated model.</typeparam>
    /// <typeparam name="TKey">The key type associated with the model.</typeparam>
    public abstract class EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey> :
        CrudRepositoryBase<TOptions, TModel, TKey>,
        ICrudRepository<TModel, TKey>
        where TModel : class, IModel<TKey>
        where TOptions : IOptions<EFCoreRepositoryOptions>
        where TContext : DbContext
        where TKey : new()
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
        /// This constructor creates a new instance of the <see cref="EFCoreCrudRepositoryBase{TContext, TOptions, TModel, TKey}"/>
        /// class.
        /// </summary>
        /// <param name="options">The options to use with the repository.</param>
        /// <param name="dataContext">The data-context to use with the repository.</param>
        protected EFCoreCrudRepositoryBase(
            TOptions options,
            TContext dataContext
            ) : base(options)
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(options, nameof(options))
                .ThrowIfNull(dataContext, nameof(dataContext));

            // Save the references.
            DataContext = dataContext;
        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <inheritdoc />
        public override IQueryable<TModel> AsQueryable()
        {
            // Defer to the efcore container.
            return DataContext.Set<TModel>().AsQueryable();
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task<TModel> AddAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var entityEntry = await DataContext.AddAsync(
                    model,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);

                // Return the newly added entity.
                return entityEntry.Entity;
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_AddAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task<TModel> UpdateAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));
            
            try
            {
                // Defer to the data-context.
                var originalEntity = await DataContext.FindAsync<TModel>(
                    new[] { model.Key },
                    cancellationToken
                    ).ConfigureAwait(false);

                // Did we fail to find the model?
                if (null == originalEntity)
                {
                    // Panic!
                    throw new KeyNotFoundException(
                        message: string.Format(
                            Resources.EfCoreCrudRepository_KeyNotFound,
                            model.Key
                            )
                        );
                }

                // Update the model properties.
                DataContext.Entry(originalEntity)
                    .CurrentValues
                    .SetValues(model);

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);

                // Read the model again.
                var updatedEntity = await DataContext.FindAsync<TModel>(
                    new[] { model.Key },
                    cancellationToken
                    ).ConfigureAwait(false);

                // Return the updated entity.
                return updatedEntity;
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_UpdateAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task DeleteAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var originalEntity = await DataContext.FindAsync<TModel>(
                    new[] { model.Key },
                    cancellationToken
                    ).ConfigureAwait(false);

                // Did we fail to find the model?
                if (null == originalEntity)
                {
                    // Panic!
                    throw new KeyNotFoundException(
                        message: string.Format(
                            Resources.EfCoreCrudRepository_KeyNotFound,
                            model.Key
                            )
                        );
                }

                // Remove the entity.
                DataContext.Remove(
                    originalEntity
                    );

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_DeleteAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
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
    /// This class is a base EFCORE implementation of the <see cref="ICrudRepository{TModel, TKey1, TKey2}"/>
    /// interface.
    /// </summary>
    /// <typeparam name="TContext">The data-context type associated with the repository.</typeparam>
    /// <typeparam name="TOptions">The options type associated with the repository.</typeparam>
    /// <typeparam name="TModel">The type of associated model.</typeparam>
    /// <typeparam name="TKey1">The key 1 type associated with the model.</typeparam>
    /// <typeparam name="TKey2">The key 2 type associated with the model.</typeparam>
    public abstract class EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2> :
        CrudRepositoryBase<TOptions, TModel, TKey1, TKey2>,
        ICrudRepository<TModel, TKey1, TKey2>
        where TModel : class, IModel<TKey1, TKey2>
        where TOptions : IOptions<EFCoreRepositoryOptions>
        where TContext : DbContext
        where TKey1 : new()
        where TKey2 : new()
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
        /// This constructor creates a new instance of the <see cref="EFCoreCrudRepositoryBase{TContext, TOptions, TModel, TKey1, TKey2}"/>
        /// class.
        /// </summary>
        /// <param name="options">The options to use with the repository.</param>
        /// <param name="dataContext">The data-context to use with the repository.</param>
        protected EFCoreCrudRepositoryBase(
            TOptions options,
            TContext dataContext
            ) : base(options)
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(options, nameof(options))
                .ThrowIfNull(dataContext, nameof(dataContext));

            // Save the references.
            DataContext = dataContext;
        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <inheritdoc />
        public override IQueryable<TModel> AsQueryable()
        {
            // Defer to the efcore container.
            return DataContext.Set<TModel>().AsQueryable();
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task<TModel> AddAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var entityEntry = await DataContext.AddAsync(
                    model,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);

                // Return the newly added entity.
                return entityEntry.Entity;
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_AddAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task<TModel> UpdateAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var originalEntity = await DataContext.FindAsync<TModel>(
                    model.Key1, 
                    model.Key2,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Did we fail to find the model?
                if (null == originalEntity)
                {
                    // Panic!
                    throw new KeyNotFoundException(
                        message: string.Format(
                            Resources.EfCoreCrudRepository_KeyNotFound2,
                            model.Key1,
                            model.Key2
                            )
                        );
                }

                // Update the model properties.
                DataContext.Entry(originalEntity)
                    .CurrentValues
                    .SetValues(model);

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);

                // Read the model again.
                var updatedEntity = await DataContext.FindAsync<TModel>(
                    model.Key1,
                    model.Key2,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Return the updated entity.
                return updatedEntity;
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_UpdateAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task DeleteAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var originalEntity = await DataContext.FindAsync<TModel>(
                    model.Key1, 
                    model.Key2,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Did we fail to find the model?
                if (null == originalEntity)
                {
                    // Panic!
                    throw new KeyNotFoundException(
                        message: string.Format(
                            Resources.EfCoreCrudRepository_KeyNotFound2,
                            model.Key1,
                            model.Key2
                            )
                        );
                }

                // Remove the entity.
                DataContext.Remove(
                    originalEntity
                    );

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_DeleteAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
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
    /// This class is a base EFCORE implementation of the <see cref="ICrudRepository{TModel, TKey1, TKey2, TKey3}"/>
    /// interface.
    /// </summary>
    /// <typeparam name="TContext">The data-context type associated with the repository.</typeparam>
    /// <typeparam name="TOptions">The options type associated with the repository.</typeparam>
    /// <typeparam name="TModel">The type of associated model.</typeparam>
    /// <typeparam name="TKey1">The key 1 type associated with the model.</typeparam>
    /// <typeparam name="TKey2">The key 2 type associated with the model.</typeparam>
    /// <typeparam name="TKey3">The key 3 type associated with the model.</typeparam>
    public abstract class EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2, TKey3> :
        CrudRepositoryBase<TOptions, TModel, TKey1, TKey2, TKey3>,
        ICrudRepository<TModel, TKey1, TKey2, TKey3>
        where TModel : class, IModel<TKey1, TKey2, TKey3>
        where TOptions : IOptions<EFCoreRepositoryOptions>
        where TContext : DbContext
        where TKey1 : new()
        where TKey2 : new()
        where TKey3 : new()
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
        /// This constructor creates a new instance of the <see cref="EFCoreCrudRepositoryBase{TContext, TOptions, TModel, TKey1, TKey2, TKey3}"/>
        /// class.
        /// </summary>
        /// <param name="options">The options to use with the repository.</param>
        /// <param name="dataContext">The data-context to use with the repository.</param>
        protected EFCoreCrudRepositoryBase(
            TOptions options,
            TContext dataContext
            ) : base(options)
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(options, nameof(options))
                .ThrowIfNull(dataContext, nameof(dataContext));

            // Save the references.
            DataContext = dataContext;
        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <inheritdoc />
        public override IQueryable<TModel> AsQueryable()
        {
            // Defer to the efcore container.
            return DataContext.Set<TModel>().AsQueryable();
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task<TModel> AddAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var entityEntry = await DataContext.AddAsync(
                    model,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);

                // Return the newly added entity.
                return entityEntry.Entity;
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_AddAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2, TKey3>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task<TModel> UpdateAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var originalEntity = await DataContext.FindAsync<TModel>(
                    model.Key1,
                    model.Key2,
                    model.Key3,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Did we fail to find the model?
                if (null == originalEntity)
                {
                    // Panic!
                    throw new KeyNotFoundException(
                        message: string.Format(
                            Resources.EfCoreCrudRepository_KeyNotFound3,
                            model.Key1,
                            model.Key2,
                            model.Key3
                            )
                        );
                }

                // Update the model properties.
                DataContext.Entry(originalEntity)
                    .CurrentValues
                    .SetValues(model);

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);

                // Read the model again.
                var updatedEntity = await DataContext.FindAsync<TModel>(
                    model.Key1,
                    model.Key2,
                    model.Key3,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Return the updated entity.
                return updatedEntity;
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_UpdateAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2, TKey3>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
        }

        // *******************************************************************

        /// <inheritdoc />
        public override async Task DeleteAsync(
            TModel model,
            CancellationToken cancellationToken = default
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(model, nameof(model));

            try
            {
                // Defer to the data-context.
                var originalEntity = await DataContext.FindAsync<TModel>(
                    model.Key1,
                    model.Key2,
                    model.Key3,
                    cancellationToken
                    ).ConfigureAwait(false);

                // Did we fail to find the model?
                if (null == originalEntity)
                {
                    // Panic!
                    throw new KeyNotFoundException(
                        message: string.Format(
                            Resources.EfCoreCrudRepository_KeyNotFound3,
                            model.Key1,
                            model.Key2,
                            model.Key3
                            )
                        );
                }

                // Remove the entity.
                DataContext.Remove(
                    originalEntity
                    );

                // Save the changes.
                await DataContext.SaveChangesAsync(
                    cancellationToken
                    ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Add better context to the error.
                throw new RepositoryException(
                    message: string.Format(
                        Resources.EfCoreCrudRepository_DeleteAsync,
                        nameof(EFCoreCrudRepositoryBase<TContext, TOptions, TModel, TKey1, TKey2, TKey3>),
                        typeof(TModel).Name,
                        JsonSerializer.Serialize(model)
                        ),
                    innerException: ex
                    );
            }
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
