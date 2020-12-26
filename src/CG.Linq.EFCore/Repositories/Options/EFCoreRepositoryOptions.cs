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
        /// needed, at startup (or not).
        /// </summary>
        public bool EnsureCreated { get; set; }

        /// <summary>
        /// This property indicates whether the database should be seeded with 
        /// data, if needed, at startup (or not).
        /// </summary>
        public bool SeedDatabase { get; set; }

        #endregion
    }
}
