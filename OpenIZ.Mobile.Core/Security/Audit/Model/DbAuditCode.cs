﻿using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Security.Audit.Model
{
    /// <summary>
    /// Audit codes
    /// </summary>
    [Table("audit_code")]
    public class DbAuditCode
    {
        /// <summary>
        /// Identifier of the audit code instance
        /// </summary>
        [Column("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [Column("code")]
        public string Code { get; set; }

        /// <summary>
        /// Code system
        /// </summary>
        [Column("code_system")]
        public String CodeSystem { get; set; }
    }
}
