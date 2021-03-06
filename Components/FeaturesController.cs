// <copyright file="FeaturesController.cs" company="Engage Software">
// Engage: Events - http://www.EngageSoftware.com
// Copyright (c) 2004-2011
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Events.Components
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Search;

    using Engage.Annotations;
    using Engage.Dnn.Framework;
    using Engage.Events;

    using Utility = Engage.Dnn.Events.Utility;

    /// <summary>
    /// Controls which DNN features are available for this module.
    /// </summary>
    [UsedImplicitly, SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection by DNN")]
    internal class FeaturesController : IUpgradeable, ISearchable
    {
        /// <summary>
        /// Performs an action when the module is installed/upgraded, based on the given <paramref name="version"/>.
        /// </summary>
        /// <param name="version">The version to which the module is being upgraded.</param>
        /// <returns>A status message</returns>
        public string UpgradeModule(string version)
        {
            var versionNumber = new Version(version);
            if (versionNumber.Equals(new Version(1, 7, 0)))
            {
                var dataProvider = DataProvider.Instance();
                using (var transaction = dataProvider.GetTransaction())
                using (var timeZoneOffsetsReader = dataProvider.ExecuteReader("Engage_spGetEventTimeZoneOffsetsFor01_07_00Upgrade").AsEnumerable())
                {
                    foreach (var timeZoneOffset in timeZoneOffsetsReader.Select(reader => reader.GetInt32(0)))
                    {
                        var timeZone = Dnn.Utility.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(timeZoneOffset);
                        dataProvider.ExecuteNonQuery(
                            "Engage_spConvertTimeZoneOffsetToTimeZoneFor01_07_00Upgrade",
                            Engage.Utility.CreateIntegerParam("timeZoneOffset", timeZoneOffset),
                            Engage.Utility.CreateVarcharParam("timeZone", timeZone.Id, 50));
                    }

                    dataProvider.CommitTransaction(transaction);
                }

                dataProvider.ExecuteNonQuery("Engage_spCleanup01_07_00Upgrade");

                return "Engage: Events 01.07.00 upgrade complete: time zones converted from offsets to real time zones";
            }

            return "No upgrade action required for version " + version + " of Engage: Events";
        }

        /// <summary>
        /// Gets the search items for the given module.
        /// </summary>
        /// <param name="modInfo">The module for which to get search items.</param>
        /// <returns>A new <see cref="SearchItemInfoCollection"/> for the items in the module</returns>
        public SearchItemInfoCollection GetSearchItems(ModuleInfo modInfo)
        {
            if (modInfo == null)
            {
                throw new ArgumentNullException("modInfo");
            }

            var detailDisplayTabId = ModuleSettings.DetailsDisplayTabId.GetValueAsInt32For(Utility.DesktopModuleName, modInfo, ModuleSettings.DetailsDisplayTabId.DefaultValue);
            var detailDisplayModuleId = ModuleSettings.DetailsDisplayModuleId.GetValueAsInt32For(Utility.DesktopModuleName, modInfo, ModuleSettings.DetailsDisplayModuleId.DefaultValue);
            if ((detailDisplayTabId != null && detailDisplayTabId != modInfo.TabID) || detailDisplayModuleId == null)
            {
                // If it's set to display on another page, just show it in this module; there's no way to link a search result to another page
                detailDisplayModuleId = modInfo.ModuleID;
            }

            var isListingDisplay = ModuleSettings.DisplayType.GetValueAsStringFor(Utility.DesktopModuleName, modInfo, ModuleSettings.DisplayType.DefaultValue).Equals("LIST", StringComparison.OrdinalIgnoreCase);
            var dateRange = isListingDisplay
                                ? ModuleSettings.GetDateRangeFor(new FakeModuleControlBase(Utility.DesktopModuleName, modInfo))
                                : new DateRange(DateRangeBound.CreateUnboundedBound(), DateRangeBound.CreateUnboundedBound());
            var featuredOnly = ModuleSettings.FeaturedOnly.GetValueAsBooleanFor(Utility.DesktopModuleName, modInfo, ModuleSettings.FeaturedOnly.DefaultValue);
            var hideFullEvents = ModuleSettings.HideFullEvents.GetValueAsBooleanFor(Utility.DesktopModuleName, modInfo, ModuleSettings.HideFullEvents.DefaultValue);
            var categoriesSettingValue = ModuleSettings.Categories.GetValueAsStringFor(Utility.DesktopModuleName, modInfo, ModuleSettings.Categories.DefaultValue);
            var categoryIds = string.IsNullOrEmpty(categoriesSettingValue)
                                  ? Enumerable.Empty<int>()
                                  : categoriesSettingValue.Split(',').Select(id => int.Parse(id, CultureInfo.InvariantCulture));

            var querystringParameters = new[] { "modId=" + detailDisplayModuleId.Value.ToString(CultureInfo.InvariantCulture), "key=EventDetail" };

            var events = EventCollection.Load(
                    modInfo.PortalID,
                    dateRange.GetStartDateUtc(),
                    dateRange.GetEndDateUtc(),
                    isListingDisplay,
                    featuredOnly,
                    hideFullEvents,
                    null,
                    categoryIds);
            return new SearchItemInfoCollection(events.Cast<Event>()
                                                      .Select(e => new SearchItemInfo(
                                                          e.Title,
                                                          e.Overview,
                                                          e.CreatedBy,
                                                          e.RevisionDate,
                                                          modInfo.ModuleID,
                                                          e.Id.ToString(CultureInfo.InvariantCulture),
                                                          e.Title + ' ' + e.Overview + ' ' + e.Description,
                                                          string.Join("&", Utility.GetEventParameters(e, querystringParameters).ToArray())))
                                                      .ToArray());
        }
    }
}