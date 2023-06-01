using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SRTPluginBase
{
    public static class Extensions
    {
        public static bool ByteArrayEquals(this byte[] first, byte[] second) => ByteArrayEquals(new ReadOnlySpan<byte>(first), new ReadOnlySpan<byte>(second));

        public static bool ByteArrayEquals(this ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
        {
            // Check to see if the have the same reference.
            if (first == second)
                return true;

            // Check to make sure neither are null.
            if (first == null || second == null)
                return false;

            // Ensure the array lengths match.
            if (first.Length != second.Length)
                return false;

            // Check each element side by side for equality.
            for (int i = 0; i < first.Length; ++i)
                if (first[i] != second[i])
                    return false;

            // We made it past the for loop, we're equal!
            return true;
        }

        public static readonly JsonSerializerOptions JSO = new JsonSerializerOptions() { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, WriteIndented = true };
		public static string GetConfigFile(this Assembly a) => Path.Combine(new FileInfo(a.Location).DirectoryName, string.Format("{0}.cfg", GetPluginNameFromFilename(a)));
		public static string GetPluginNameFromFilename(this Assembly a) => Path.GetFileNameWithoutExtension(new FileInfo(a.Location).Name);

		public static T LoadConfiguration<T>() where T : class => LoadConfiguration<T>(null);
        public static T LoadConfiguration<T>(string? configFile = null) where T : class
        {
            // TODO: ILogger
            if (configFile == null)
                configFile = GetConfigFile(typeof(T).Assembly);

            try
            {
                if (File.Exists(configFile))
                    using (FileStream fs = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                        return JsonSerializer.DeserializeAsync<T>(fs, JSO).Result ?? Activator.CreateInstance<T>();
                else
                    return Activator.CreateInstance<T>(); // File did not exist, just return a new instance.
            }
            catch
            {
                // TODO: ILogger
                return Activator.CreateInstance<T>(); // An exception occurred when reading the file, return a new instance.
            }
        }

        public static void SaveConfiguration<T>(this T configuration) where T : class => SaveConfiguration<T>(configuration, null);
        public static void SaveConfiguration<T>(this T configuration, string? configFile = null) where T : class
        {
            // TODO: ILogger
            if (configFile == null)
                configFile = GetConfigFile(typeof(T).Assembly);

            if (configuration != null) // Only save if configuration is not null.
            {
                try
                {
                    using (FileStream fs = new FileStream(configFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
                        JsonSerializer.SerializeAsync<T>(fs, configuration, JSO).Wait();
                }
                catch
                {
                    // TODO: ILogger
                }
            }
        }

        public enum SqliteObjectType
        {
            Table,
            Index,
            View,
            Trigger,
        }

        private const string SQLITE_QUERY_OBJECT_EXISTS = "SELECT IIF(EXISTS(SELECT 1 FROM [sqlite_schema] WHERE [type] = '{0}' AND [name] = @name), 1, 0);";
		private const string SQLITE_QUERY_COLUMN_EXISTS = "SELECT IIF(EXISTS(SELECT 1 FROM PRAGMA_table_info(@name) WHERE [name] = @columnName), 1, 0);";
        public static bool SqliteObjectExists<T>(this ConfigurationDB<T> plugin, string name, SqliteObjectType type) => (long)(plugin.DbScalar(string.Format(SQLITE_QUERY_OBJECT_EXISTS, type.ToString().ToLowerInvariant()), default, new SqliteParameter("@name", name)) ?? 0L) == 1L;
        public static async Task<bool> SqliteObjectExistsAsync<T>(this ConfigurationDB<T> plugin, string name, SqliteObjectType type, CancellationToken cancellationToken) => (long)(await plugin.DbScalarAsync(string.Format(SQLITE_QUERY_OBJECT_EXISTS, type.ToString().ToLowerInvariant()), default, cancellationToken, new SqliteParameter("@name", name)) ?? 0L) == 1L;

		public static bool SqliteTableExists<T>(this ConfigurationDB<T> plugin, string name) => SqliteObjectExists(plugin, name, SqliteObjectType.Table);
		public static bool SqliteIndexExists<T>(this ConfigurationDB<T> plugin, string name) => SqliteObjectExists(plugin, name, SqliteObjectType.Index);
		public static bool SqliteViewExists<T>(this ConfigurationDB<T> plugin, string name) => SqliteObjectExists(plugin, name, SqliteObjectType.View);
		public static bool SqliteTriggerExists<T>(this ConfigurationDB<T> plugin, string name) => SqliteObjectExists(plugin, name, SqliteObjectType.Trigger);
		public static bool SqliteColumnExists<T>(this ConfigurationDB<T> plugin, string tableOrViewName, string columnName) => (long)(plugin.DbScalar(SQLITE_QUERY_COLUMN_EXISTS, default, new SqliteParameter("@name", tableOrViewName), new SqliteParameter("@columnName", columnName)) ?? 0L) == 1L;

		public static Task<bool> SqliteTableExistsAsync<T>(this ConfigurationDB<T> plugin, string name, CancellationToken cancellationToken) => SqliteObjectExistsAsync(plugin, name, SqliteObjectType.Table, cancellationToken);
		public static Task<bool> SqliteIndexExistsAsync<T>(this ConfigurationDB<T> plugin, string name, CancellationToken cancellationToken) => SqliteObjectExistsAsync(plugin, name, SqliteObjectType.Index, cancellationToken);
		public static Task<bool> SqliteViewExistsAsync<T>(this ConfigurationDB<T> plugin, string name, CancellationToken cancellationToken) => SqliteObjectExistsAsync(plugin, name, SqliteObjectType.View, cancellationToken);
		public static Task<bool> SqliteTriggerExistsAsync<T>(this ConfigurationDB<T> plugin, string name, CancellationToken cancellationToken) => SqliteObjectExistsAsync(plugin, name, SqliteObjectType.Trigger, cancellationToken);
		public static async Task<bool> SqliteColumnExistsAsync<T>(this ConfigurationDB<T> plugin, string tableOrViewName, string columnName, CancellationToken cancellationToken) => (long)(await plugin.DbScalarAsync(SQLITE_QUERY_COLUMN_EXISTS, default, cancellationToken, new SqliteParameter("@name", tableOrViewName), new SqliteParameter("@columnName", columnName)).ConfigureAwait(false) ?? 0L) == 1L;

		public static void AddOrUpdate<K, V>(this IDictionary<K, V> dictionary, K key, V value)
		{
			if (!dictionary.TryAdd(key, value))
				dictionary[key] = value;
		}

		public static T ConfigDictionaryToModel<T>(this IDictionary<string, string?> dictionary) where T : class
		{
			T returnValue = Activator.CreateInstance<T>();

			foreach (PropertyInfo propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (dictionary.ContainsKey(propertyInfo.Name))
                    propertyInfo.SetValue(returnValue, Convert.ChangeType(dictionary[propertyInfo.Name], propertyInfo.PropertyType));

			return returnValue;
		}

		public static IDictionary<string, string?> ModelToConfigDictionary<T>(this T model) where T : class
		{
            IDictionary<string, string?> returnValue = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                returnValue.Add(propertyInfo.Name, propertyInfo.GetValue(model)?.ToString());

			return returnValue;
		}
	}
}
