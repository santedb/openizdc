﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OpenIZ.Mobile.Core.Resources {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OpenIZ.Mobile.Core.Resources.Strings", typeof(Strings).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Importing Data....
        /// </summary>
        internal static string locale_import {
            get {
                return ResourceManager.GetString("locale_import", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The import of remote data is complete.
        /// </summary>
        internal static string locale_importDoneBody {
            get {
                return ResourceManager.GetString("locale_importDoneBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remote Data Imported.
        /// </summary>
        internal static string locale_importDoneSubject {
            get {
                return ResourceManager.GetString("locale_importDoneSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deploying Data.
        /// </summary>
        internal static string locale_setting_deploy {
            get {
                return ResourceManager.GetString("locale_setting_deploy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Migrating Data.
        /// </summary>
        internal static string locale_setting_migration {
            get {
                return ResourceManager.GetString("locale_setting_migration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Installing Tables.
        /// </summary>
        internal static string locale_setting_table {
            get {
                return ResourceManager.GetString("locale_setting_table", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Downloading {0}s....
        /// </summary>
        internal static string locale_sync {
            get {
                return ResourceManager.GetString("locale_sync", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your application has been installed successfully. The application is currently downloading remote data it needs to operate from the back-end system (known as the IMS). During this time, you may notice the application operates very slowly or some screens don&apos;t have data. That is normal until the sycnrhonization is complete. We&apos;ll let you know when the synchronization operation is done!
        ///
        ///Sincerely,
        ///- OpenIZ Community.
        /// </summary>
        internal static string locale_welcomeMessageBody {
            get {
                return ResourceManager.GetString("locale_welcomeMessageBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Welcome to OpenIZ Disconnected Client Core!.
        /// </summary>
        internal static string locale_welcomeMessageSubject {
            get {
                return ResourceManager.GetString("locale_welcomeMessageSubject", resourceCulture);
            }
        }
    }
}
