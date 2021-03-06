﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using VaBank.Common.Data.Repositories;
using VaBank.Common.Util;
using VaBank.Core.App.Repositories;

namespace VaBank.Data.EntityFramework.App
{
    public class SettingRepository : IRepository, ISettingRepository
    {
        protected readonly DbContext Context;

        public SettingRepository(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "DbContext should not be empty");
            }
            Context = context;
        }

        public T GetOrDefault<T>(string key)
        {
            try
            {
                var keyParam = new SqlParameter("@Key", key);
                const string sql = "SELECT [Value] FROM [App].[Setting] WHERE [Key] = @Key";
                var json = Context.Database.SqlQuery<string>(sql, keyParam).ToList().FirstOrDefault();
                return Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }

        public object GetOrDefault(string key, Type settingsType)
        {
            try
            {
                var keyParam = new SqlParameter("@Key", key);
                const string sql = "SELECT [Value] FROM [App].[Setting] WHERE [Key] = @Key";
                var json = Context.Database.SqlQuery<string>(sql, keyParam).ToList().FirstOrDefault();
                return Deserialize(json, settingsType);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }

        public T GetOrDefault<T>() where T : class
        {
            var settingsType = typeof (T);
            var settingsAttribute = settingsType.GetCustomAttribute(typeof (SettingsAttribute)) as SettingsAttribute;
            var key = settingsAttribute == null ? settingsType.FullName : settingsAttribute.GetKey(settingsType);
            return GetOrDefault<T>(key);
        }

        public object GetOrDefault(Type settingsType)
        {
            var settingsAttribute = settingsType.GetCustomAttribute(typeof(SettingsAttribute)) as SettingsAttribute;
            var key = settingsAttribute == null ? settingsType.FullName : settingsAttribute.GetKey(settingsType);
            return GetOrDefault(key, settingsType);
        }

        public void Set<T>(string key, T value)
        {
            try
            {
                var json = Serialize(value);
                var keyParam = new SqlParameter("@Key", key);
                var valueParam = new SqlParameter("@Value", SqlDbType.NVarChar) {Value = json};
                const string sql = @"MERGE [App].[Setting] AS target
                                    USING (SELECT @Key, @Value) AS source ([Key], [Value])
                                    ON (target.[Key] = source.[Key])
                                    WHEN MATCHED THEN 
                                        UPDATE SET [Value] = source.[Value]
                                    WHEN NOT MATCHED THEN
                                        INSERT ([Key], [Value])
                                        VALUES (source.[Key], source.[Value]);";
                Context.Database.ExecuteSqlCommand(sql, valueParam, keyParam);
            }
            catch (Exception ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }

        public Dictionary<string, T> BatchGet<T>(params string[] keys)
        {
            Assert.NotNull("keys", keys);
            try
            {
                const string sql = "SELECT [Key], [Value] FROM [App].[Setting] WHERE [Key] IN ({0})";
                var parameters = Enumerable.Range(0, keys.Length)
                    .Select(i => new SqlParameter(string.Format("@p{0}", i), SqlDbType.NVarChar) {Value = keys[i]})
                    .ToArray();
                var dynamicSql = string.Format(sql, string.Join(", ", parameters.Select(x => x.ParameterName)));
                var settings = Context.Database.SqlQuery<KeyValue>(dynamicSql, parameters.Cast<object>().ToArray()).ToList();
                return settings.ToDictionary(x => x.Key, x => Deserialize<T>(x.Value));
            }
            catch (Exception ex)
            {
                throw new RepositoryException(ex.Message, ex);
            }
        }

        private static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static object Deserialize(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return GetDefaultValue(type);
            }
            return JsonConvert.DeserializeObject(json, type);
        }

        private static string Serialize<T>(T obj)
        {
            var @object = obj as object;
            return @object == null ? null : JsonConvert.SerializeObject(obj);
        }

        private static object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        private class KeyValue
        {
            public string Key { get; set; }

            public string Value { get; set; }
        }
    }
}
