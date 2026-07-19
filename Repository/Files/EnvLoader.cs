using System;
using System.IO;

namespace Repository.Files
{
    public static class EnvLoader
    {
        public static void Load(string path = ".env")
        {
            if (!File.Exists(path))
                return;

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim(); // remove whitespace

                // skip comment or empty line
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // find index of =
                int separatorIndex = line.IndexOf('=');

                // skip invalid lines
                if (separatorIndex <= 0) continue;

                string key = line.Substring(0, separatorIndex).Trim();  // get the key
                string value = line.Substring(separatorIndex + 1).Trim(); // get the value
                Environment.SetEnvironmentVariable(key, value);  // set them as an enviroment variable
            }
        }
    }
}