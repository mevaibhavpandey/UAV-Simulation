using System;
using UnityEngine;

namespace ASTRA.UAV.Utilities
{
    /// <summary>
    /// Geographic transformation utilities for converting between WGS84 GPS (Latitude, Longitude, Altitude)
    /// coordinates and local Unity Cartesian (ENU) world coordinates.
    /// Includes geodesic formulas (Haversine, Vincenty/Spherical Bearing, Tangent Plane ENU).
    /// </summary>
    public static class GeoUtilities
    {
        /// <summary>WGS84 ellipsoid semi-major axis radius in meters.</summary>
        public const double EarthRadiusEquatorial = 6378137.0;

        /// <summary>WGS84 Earth flattening ratio.</summary>
        public const double EarthFlattening = 1.0 / 298.257223563;

        /// <summary>Average spherical Earth radius in meters.</summary>
        public const double EarthMeanRadius = 6371000.0;

        /// <summary>
        /// Converts WGS84 Geodetic coordinates (Latitude, Longitude, Altitude) to local Unity Cartesian ENU coordinates
        /// relative to a reference origin point (refLat, refLon, refAlt).
        /// </summary>
        /// <param name="latitude">Target latitude in degrees.</param>
        /// <param name="longitude">Target longitude in degrees.</param>
        /// <param name="altitude">Target altitude in meters.</param>
        /// <param name="refLat">Reference origin latitude in degrees.</param>
        /// <param name="refLon">Reference origin longitude in degrees.</param>
        /// <param name="refAlt">Reference origin altitude in meters.</param>
        /// <returns>Local Unity position vector (X = East, Y = Up, Z = North).</returns>
        public static Vector3 LatLonAltToUnityWorld(double latitude, double longitude, double altitude, double refLat, double refLon, double refAlt)
        {
            double dLat = (latitude - refLat) * Math.PI / 180.0;
            double dLon = (longitude - refLon) * Math.PI / 180.0;
            double refLatRad = refLat * Math.PI / 180.0;

            // East offset (X axis in Unity ENU)
            double east = dLon * Math.Cos(refLatRad) * EarthMeanRadius;

            // North offset (Z axis in Unity ENU)
            double north = dLat * EarthMeanRadius;

            // Up offset (Y axis in Unity ENU)
            double up = altitude - refAlt;

            return new Vector3((float)east, (float)up, (float)north);
        }

        /// <summary>
        /// Converts local Unity Cartesian ENU world position back into WGS84 Geodetic coordinates (Lat, Lon, Alt)
        /// relative to a reference origin point.
        /// </summary>
        /// <param name="unityPosition">Local Unity position vector.</param>
        /// <param name="refLat">Reference origin latitude in degrees.</param>
        /// <param name="refLon">Reference origin longitude in degrees.</param>
        /// <param name="refAlt">Reference origin altitude in meters.</param>
        /// <param name="latitude">Extracted target latitude in degrees.</param>
        /// <param name="longitude">Extracted target longitude in degrees.</param>
        /// <param name="altitude">Extracted target altitude in meters.</param>
        public static void UnityWorldToLatLonAlt(Vector3 unityPosition, double refLat, double refLon, double refAlt, out double latitude, out double longitude, out double altitude)
        {
            double refLatRad = refLat * Math.PI / 180.0;

            double dLat = (unityPosition.z / EarthMeanRadius) * (180.0 / Math.PI);
            double dLon = (unityPosition.x / (EarthMeanRadius * Math.Cos(refLatRad))) * (180.0 / Math.PI);

            latitude = refLat + dLat;
            longitude = refLon + dLon;
            altitude = refAlt + unityPosition.y;
        }

        /// <summary>
        /// Computes the great-circle Haversine distance in meters between two WGS84 latitude/longitude points.
        /// </summary>
        /// <param name="lat1">Latitude of point 1 in degrees.</param>
        /// <param name="lon1">Longitude of point 1 in degrees.</param>
        /// <param name="lat2">Latitude of point 2 in degrees.</param>
        /// <param name="lon2">Longitude of point 2 in degrees.</param>
        /// <returns>Distance in meters.</returns>
        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            double rLat1 = lat1 * Math.PI / 180.0;
            double rLat2 = lat2 * Math.PI / 180.0;

            double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                       Math.Cos(rLat1) * Math.Cos(rLat2) *
                       Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return EarthMeanRadius * c;
        }

        /// <summary>
        /// Calculates initial bearing in degrees (0 to 360) from point 1 to point 2.
        /// </summary>
        /// <param name="lat1">Latitude 1 in degrees.</param>
        /// <param name="lon1">Longitude 1 in degrees.</param>
        /// <param name="lat2">Latitude 2 in degrees.</param>
        /// <param name="lon2">Longitude 2 in degrees.</param>
        /// <returns>Bearing angle in degrees clockwise from True North.</returns>
        public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
        {
            double rLat1 = lat1 * Math.PI / 180.0;
            double rLat2 = lat2 * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            double y = Math.Sin(dLon) * Math.Cos(rLat2);
            double x = Math.Cos(rLat1) * Math.Sin(rLat2) -
                       Math.Sin(rLat1) * Math.Cos(rLat2) * Math.Cos(dLon);

            double bearingRad = Math.Atan2(y, x);
            double bearingDeg = bearingRad * (180.0 / Math.PI);
            return (bearingDeg + 360.0) % 360.0;
        }

        /// <summary>
        /// Calculates destination latitude and longitude given start point, bearing, and distance.
        /// </summary>
        /// <param name="lat1">Start latitude in degrees.</param>
        /// <param name="lon1">Start longitude in degrees.</param>
        /// <param name="bearingDeg">Bearing angle in degrees from True North.</param>
        /// <param name="distanceMeters">Distance to travel in meters.</param>
        /// <param name="destLat">Output destination latitude in degrees.</param>
        /// <param name="destLon">Output destination longitude in degrees.</param>
        public static void CalculateDestinationPoint(double lat1, double lon1, double bearingDeg, double distanceMeters, out double destLat, out double destLon)
        {
            double angularDistance = distanceMeters / EarthMeanRadius;
            double bearingRad = bearingDeg * Math.PI / 180.0;

            double rLat1 = lat1 * Math.PI / 180.0;
            double rLon1 = lon1 * Math.PI / 180.0;

            double rLat2 = Math.Asin(Math.Sin(rLat1) * Math.Cos(angularDistance) +
                           Math.Cos(rLat1) * Math.Sin(angularDistance) * Math.Cos(bearingRad));

            double rLon2 = rLon1 + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(angularDistance) * Math.Cos(rLat1),
                                             Math.Cos(angularDistance) - Math.Sin(rLat1) * Math.Sin(rLat2));

            destLat = rLat2 * (180.0 / Math.PI);
            destLon = rLon2 * (180.0 / Math.PI);
        }
    }
}


