namespace NuGetGallery
{
    using System.ComponentModel.DataAnnotations;

    public class PackageFile : IEntity
    {
        public int Key { get; set; }

        public Package Package { get; set; }
        public int PackageKey { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [StringLength(500), Required]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        /// <remarks>
        /// Has a max length of 4000. Is not indexed and not used for searches. Db column is nvarchar(max).
        /// </remarks>
        [Required]
        public string FileContent { get; set; }
    }
}