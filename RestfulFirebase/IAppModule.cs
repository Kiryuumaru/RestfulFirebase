using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase
{
    /// <summary>
    /// The app module interface for the <see cref="RestfulFirebaseApp"/> app uses.
    /// </summary>
    public interface IAppModule
    {
        /// <summary>
        /// Gets the underlying <see cref="RestfulFirebaseApp"/> this module uses.
        /// </summary>
        RestfulFirebaseApp App { get; }
    }
}
