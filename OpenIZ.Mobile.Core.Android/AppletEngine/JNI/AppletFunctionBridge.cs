﻿using System;
using Android.Webkit;
using Java.Interop;
using Android.Widget;
using Android.Content;
using Org.Json;
using Android.App;
using Android.Content.Res;
using OpenIZ.Mobile.Core.Diagnostics;

namespace OpenIZ.Mobile.Core.Android.AppletEngine.JNI
{
	
	/// <summary>
	/// Applet functions which are related to javascript
	/// </summary>
	public class AppletFunctionBridge : Java.Lang.Object
	{

		// Context
		private Context m_context;
		private AppletWebView m_view;
        private Tracer m_tracer = Tracer.GetTracer(typeof(AppletFunctionBridge));

		/// <summary>
		/// Gets the context of the function
		/// </summary>
		/// <param name="context">Context.</param>
		public AppletFunctionBridge (Context context, AppletWebView view)
		{
			
			this.m_context = context;
			this.m_view = view;
		}

		/// <summary>
		/// Shows the specified message as a toast
		/// </summary>
		[Export]
		[JavascriptInterface]
		public void ShowToast(String toastText)
		{
			Toast.MakeText (this.m_context, toastText, ToastLength.Short).Show();
		}

		/// <summary>
		/// Navigate the specified appletId and context.
		/// </summary>
		/// <param name="appletId">Applet identifier.</param>
		/// <param name="context">Context.</param>
		[Export]
		[JavascriptInterface]
		public void Navigate(String appletId, String context)
		{
			// TODO: Parameters
			Application.SynchronizationContext.Post (_ => {
				this.m_view.LoadUrl(String.Format("app://openiz.org/applet/{0}/", appletId));
			}, null);
		}

		/// <summary>
		/// Get the specified string
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="stringId">String identifier.</param>
		[Export]
		[JavascriptInterface]
		public String GetString(String stringId)
		{
            try
            {
                return this.m_context.Resources.GetString(this.m_context.Resources.GetIdentifier(stringId, "string", this.m_context.PackageName));
            }
            catch(Exception e)
            {
                this.m_tracer.TraceError("Error retreiving string {0}: {1}", stringId, e);
                return stringId;
            }
		}

		/// <summary>
		/// Go back home
		/// </summary>
		[Export]
		[JavascriptInterface]
		public void Back()
		{
			Application.SynchronizationContext.Post (_ => {
				if (this.m_view.CanGoBack ())
					this.m_view.GoBack ();
				else
					(this.m_context as Activity).Finish ();
			}, null);
		}


		/// <summary>Close the applet</summary>
		[Export]
		[JavascriptInterface]
		public void Close() {
			Application.SynchronizationContext.Post (_ => {
				(this.m_context as Activity).Finish ();
			}, null);
		}

		/// <summary>
		/// Performs a barcode scan
		/// </summary>
		[Export]
		[JavascriptInterface]
		public String BarcodeScan()
		{
			try
			{
				return "XXXXXXXXXXXX";
			}
			catch(Exception e) {
				this.ShowToast (e.Message);
				return null;
			}
		}
	}
}

