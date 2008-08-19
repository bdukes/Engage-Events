// <copyright file="MainContainer.ascx.cs" company="Engage Software">
// Engage: Events - http://www.engagemodules.com
// Copyright (c) 2004-2008
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using Utility = Engage.Utility;

    /// <summary>
    /// The main container that is used by the Engage: Events module.  
    /// This control is registered with DNN, and is in charge of loading other requested control.
    /// </summary>
    public partial class MainContainer : ModuleBase
    {
        /// <summary>
        /// A dictionary mapping control keys to user controls.
        /// </summary>
        private static readonly IDictionary<string, string> ControlKeys = FillControlKeys();

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.LoadChildControl(this.GetControlToLoad());
        }

        /// <summary>
        /// Fills <see cref="ControlKeys"/>.
        /// </summary>
        /// <returns>A dictionary mapping control keys to user controls.</returns>
        private static IDictionary<string, string> FillControlKeys()
        {
            IDictionary<string, string> keyDictionary = new Dictionary<string, string>(11, StringComparer.OrdinalIgnoreCase);

            keyDictionary.Add("EmailEdit", "EmailEdit.ascx");
            keyDictionary.Add("EventEdit", "EventEdit.ascx");
            keyDictionary.Add("EventListing", "Display/EventListingTemplate.ascx");
            keyDictionary.Add("EventListingAdmin", "Display/EventListingAdmin.ascx");
            keyDictionary.Add("RsvpSummary", "RsvpSummary.ascx");
            keyDictionary.Add("RsvpDetail", "RsvpDetail.ascx");
            keyDictionary.Add("Rsvp", "Rsvp.ascx");
            keyDictionary.Add("EmailAFriend", "EmailAFriend.ascx");
            keyDictionary.Add("Register", "Register.ascx");
            keyDictionary.Add("EventDetail", "Display/EventDetail.ascx");

            return keyDictionary;
        }

        /// <summary>
        /// Gets the control to load, based on the key (or lack thereof) that is passed on the querystring.
        /// </summary>
        /// <returns>A relative path to the control that should be loaded into this container</returns>
        private string GetControlToLoad()
        {
            if (!IsConfigured)
            {
                return "Admin/NotConfigured.ascx";
            }

            string keyParam = this.GetCurrentControlKey();
            return Engage.Utility.HasValue(keyParam) ? ControlKeys[keyParam] : "Display/EventListingTemplate.ascx";
        }

        /// <summary>
        /// Loads the child control to be displayed in this container.
        /// </summary>
        /// <param name="controlToLoad">The control to load.</param>
        private void LoadChildControl(string controlToLoad)
        {
            try
            {
                if (controlToLoad == null)
                {
                    return;
                }

                PortalModuleBase mb = (PortalModuleBase)this.LoadControl(controlToLoad);
                mb.ModuleConfiguration = this.ModuleConfiguration;
                mb.ID = Path.GetFileNameWithoutExtension(controlToLoad);
                this.phControls.Controls.Add(mb);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}