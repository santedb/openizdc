﻿using System;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

namespace OpenIZ.Mobile.Core.Applets
{
	/// <summary>
	/// The applet manifest is responsible for storing data related to a JavaScript applet
	/// </summary>
	[XmlType(nameof(AppletManifest), Namespace = "http://openiz.org/mobile/applet")]
	[XmlRoot(nameof(AppletManifest), Namespace = "http://openiz.org/mobile/applet")]
	public class AppletManifest
	{

		/// <summary>
		/// Load the specified manifest name
		/// </summary>
		public static AppletManifest Load(Stream resourceStream)
		{
			XmlSerializer xsz = new XmlSerializer (typeof(AppletManifest));
			return xsz.Deserialize (resourceStream) as AppletManifest;

		}

		/// <summary>
		/// Gets or sets the tile sizes the applet can have
		/// </summary>
		[XmlElement("tile")]
		public List<AppletTile> Tiles {
			get;
			set;
		}

		/// <summary>
		/// Applet information itself
		/// </summary>
		[XmlElement("info")]
		public AppletInfo Info {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the signature which can be used to validate the file
		/// </summary>
		[XmlElement("dsig")]
		public byte[] Signature {
			get;
			set;
		}

		/// <summary>
		/// Gets the thumbprint of the key that signed the signature
		/// </summary>
		[XmlElement("dsig_key")]
		public String SignatureKey {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the assets which are to be used in the applet
		/// </summary>
		[XmlElement("asset")]
		public List<AppletAsset> Assets {
			get;
			set;
		}
	}
}

