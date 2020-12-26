using CG.Business.Repositories.Options;
using System;

namespace CG.Linq.EFCore.Repositories.Options
{
    /// <summary>
    /// This class contains configuration settings for an EFCore repository.
    /// </summary>
    public class EFCoreRepositoryOptions : LinqRepositoryOptions
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property indicates whether migrations should be applied, at
        /// startup (or not).
        /// </summary>
        public bool ApplyMigrations { get; set; }

        /// <summary>
        /// This property indicates whether the database should be created, if
        /// needed, at startup (or not). Note, this step is only ever performed
        /// when running in the <c>Development</c> environment, in order to 
        /// prevent horrible accidents in production.
        /// </summary>
        public bool EnsureCreated { get; set; }

        /// <summary>
        /// This property indicates whether the database should be dropped, if 
        /// it already exists (or not). Note, this step is only ever performed
        /// when running in the <c>Development</c> environment, in order to 
        /// prevent horrible accidents in production.
        /// </summary>
        public bool DropDatabase { get; set; }

        /// <summary>
        /// This property indicates whether the database should be seeded with 
        /// data, if needed, at startup (or not). Note, this step is only ever 
        /// performed when running in the <c>Development</c> environment, in order 
        /// to prevent horrible accidents in production.
        /// </summary>
        public bool SeedDatabase { get; set; }

        #endregion
    }
}
