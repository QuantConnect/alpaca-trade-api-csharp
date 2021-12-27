﻿using System;
using JetBrains.Annotations;

namespace Alpaca.Markets
{
    /// <summary>
    /// Encapsulates the basic trading status update information from Alpaca APIs.
    /// </summary>
    public interface IStatus
    {
        /// <summary>
        /// Gets asset name.
        /// </summary>
        [UsedImplicitly]
        String Symbol { get; }

        /// <summary>
        /// Gets status timestamp in UTC time zone.
        /// </summary>
        [UsedImplicitly]
        [Obsolete("This property will be removed in the next major version of SDK. Use the TimestampUtc property instead.", false)]
        DateTime TimeUtc { get; }

        /// <summary>
        /// Gets status timestamp in UTC time zone.
        /// </summary>
        [UsedImplicitly]
        DateTime TimestampUtc { get; }

        /// <summary>
        /// Gets status code.
        /// </summary>
        [UsedImplicitly]
        String StatusCode { get; }

        /// <summary>
        /// Gets status message.
        /// </summary>
        [UsedImplicitly]
        String StatusMessage { get; }

        /// <summary>
        /// Gets reason code.
        /// </summary>
        [UsedImplicitly]
        String ReasonCode { get; }

        /// <summary>
        /// Gets reason message.
        /// </summary>
        [UsedImplicitly]
        String ReasonMessage { get; }

        /// <summary>
        /// Gets tape.
        /// </summary>
        [UsedImplicitly]
        String Tape { get; }
    }
}
