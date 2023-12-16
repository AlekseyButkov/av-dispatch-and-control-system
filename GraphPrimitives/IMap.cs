using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVGraphPrimitives
{
    public interface IMap
    {
        /// <summary>
        /// Returns a list of all locations.
        /// </summary>
        /// <returns></returns>
        List<long> GetLocations();

        /// <summary>
        /// Answers to the question: Where can I get, following one of the connections starting at the specified location?
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        List<long> GetPossibleNextLocations(long location);

        /// <summary>
        /// Answers to the question: From where can I reach a specified location, following one of the map connections?
        /// </summary>
        List<long> GetPossiblePrevLocations(long location);

        /// <summary>
        /// Returns the travel distance between the two specified locations if they are linked by a connection, positive infinity otherwise. 
        /// </summary>
        double GetDistanceMetric(long fromLocation, long toLocation);

        /// <summary>
        /// Returns the travel time between the two specified locations if they are linked by a connection, considering the speed limits
        /// positive infinity otherwise.
        /// </summary>
        double GetTimeMetric(long fromLocation, long toLocation);

        /// <summary>
        /// Returns the position of the specified location. The position is represented by two coordinates, e.g. latitude and longitude values. 
        /// </summary>
        Point2D GetPosition(long loc);

        /// <summary>
        /// Returns a location which is selected by random.
        /// </summary>
        /// <returns></returns>
        long RandomlyGenerateDestination();
    }
}
