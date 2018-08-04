using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{

    /// <summary>
    /// Database association
    /// </summary>
    public interface IDbAssociation
    {
        /// <summary>
        /// Gets or sets the source of the association
        /// </summary>
        Guid SourceKey { get; set; }
    }

    /// <summary>
    /// Represents the databased associated entity
    /// </summary>
    public abstract class DbAssociation : DbIdentified, IDbAssociation
    {
        /// <summary>
        /// Gets or sets the key of the item associated with this object
        /// </summary>
        public abstract Guid SourceKey { get; set; }

    }
}
