﻿// <copyright file="RsvpCollection.cs" company="Engage Software">
// Engage: Events - http://www.engagemodules.com
// Copyright (c) 2004-2008
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
namespace Engage.Events
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using Data;

    /// <summary>
    /// The collection of <see cref="Rsvp"/> objects for a given event.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="BindingList{T}"/> for future support.
    /// </remarks>
    public class RsvpCollection : BindingList<Rsvp>
    {
        /// <summary>
        /// Backing field for <see cref="TotalRecords"/>.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int totalRecords;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsvpCollection"/> class.
        /// </summary>
        /// <param name="totalRecords">The total number of records returned by the query that filled this collection.</param>
        private RsvpCollection(int totalRecords)
        {
            this.totalRecords = totalRecords;
        }

        /// <summary>
        /// Gets the total records.
        /// </summary>
        /// <value>The total records.</value>
        public int TotalRecords
        {
            [DebuggerStepThrough]
            get { return this.totalRecords; }
        }

        /// <summary>
        /// Loads the collection of <see cref="Rsvp"/> objects for the specified event.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="status">The status.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="index">The page index.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>The collection of <see cref="Rsvp"/> objects for the specified event.</returns>
        /// <exception cref="DBException">If an error occurs while loading the collection from the database</exception>
        public static RsvpCollection Load(int eventId, string status, string sortColumn, int index, int pageSize)
        {
            IDataProvider dp = DataProvider.Instance;
            try
            {
                using (
                    IDataReader dataReader = dp.ExecuteReader(
                        CommandType.StoredProcedure,
                        dp.NamePrefix + "spGetRsvps",
                        Utility.CreateIntegerParam("@EventId", eventId),
                        Utility.CreateVarcharParam("@Status", status),
                        Utility.CreateVarcharParam("@sortColumn", sortColumn, 200),
                        Utility.CreateIntegerParam("@index", index),
                        Utility.CreateIntegerParam("@pageSize", pageSize)))
                {
                    return FillRsvps(dataReader);
                }
            }
            catch (Exception se)
            {
                throw new DBException("spGetRsvps", se);
            }
        }

        /// <summary>
        /// Fills the RSVPS.
        /// </summary>
        /// <param name="dataReader">A data reader representing the record of an RSVP.</param>
        /// <returns>The collection of <see cref="Rsvp"/> objects for the given event.</returns>
        private static RsvpCollection FillRsvps(IDataReader dataReader)
        {
            if (dataReader.Read())
            {
                RsvpCollection rsvps = new RsvpCollection((int)dataReader["TotalRecords"]);

                if (dataReader.NextResult())
                {
                    while (dataReader.Read())
                    {
                        rsvps.Add(Rsvp.Fill(dataReader));
                    }
                }

                return rsvps;
            }

            throw new DBException("Data reader did not have the expected structure.  An error must have occurred in the query.");
        }
    }
}