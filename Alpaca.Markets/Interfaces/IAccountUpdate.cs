﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Alpaca.Markets
{
    /// <summary>
    /// Encapsulates account update information from Alpaca streaming API.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [Obsolete("This interface will be removed in the next major SDK release", false)]
    public interface IAccountUpdate : IAccountBase
    {
        /// <summary>
        /// Gets timestamp of last account update event in the UTC.
        /// </summary>
        DateTime UpdatedAtUtc { get; }

        /// <summary>
        /// Gets timestamp of account deletion event in the UTC.
        /// </summary>
        DateTime? DeletedAtUtc { get; }
    }
}
