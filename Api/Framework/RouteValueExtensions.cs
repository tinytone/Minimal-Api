﻿namespace Api.Framework
{
    public static class RouteValueExtensions
    {
        public static int GetInt(this RouteValueDictionary routeValues, string key)
        {
            if (routeValues.TryGetValue(key, out var objectValue))
            {
                if (objectValue is string strValue)
                {
                    if (int.TryParse(strValue, out var number))
                    {
                        return number;
                    }
                }
            }

            return default;
        }
    }
}
