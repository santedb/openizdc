﻿using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Data.Model.Acts
{
    /// <summary>
    /// Represents an act protocol
    /// </summary>
    [Table("act_protocol")]
    public class DbActProtocol : DbIdentified
    {

        /// <summary>
        /// Gets or sets the act UUID
        /// </summary>
        [Column("act_uuid"), MaxLength(16), Indexed]
        public byte[] ActUuid { get; set; }

        /// <summary>
        /// Gets or sets the protocol uuid
        /// </summary>
        [Column("proto_uuid"), MaxLength(16), Indexed]
        public byte[] ProtocolUuid { get; set; }

    }
}