using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Domain.Core.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mix.Common.Helper
{
    public class UnitOfWorkHelper<TDbContext> where TDbContext : DbContext
    {
        /// <summary>
        /// Initializes the context.
        /// </summary>
        /// <returns></returns>
        public static TDbContext InitContext()
        {
            Type classType = typeof(TDbContext);
            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            TDbContext context = (TDbContext)classConstructor.Invoke(new object[] { });
            return context;
        }

        public static void HandleTransaction(bool isSucceed, bool isRoot, IDbContextTransaction transaction)
        {
            if (isSucceed)
            {
                if (isRoot)
                {
                    //if current transaction is root transaction
                    transaction.Commit();
                }
            }
            else
            {
                if (isRoot)
                {
                    //if current transaction is root transaction
                    transaction.Rollback();
                }
            }
        }

        public static RepositoryResponse<TResult> HandleException<TResult>(Exception ex, bool isRoot, IDbContextTransaction transaction)
            
        {
            if (isRoot)
            {
                //if current transaction is root transaction
                transaction.Rollback();
            }
            List<string> errors = new List<string>();
            LogException(ex);
            errors.Add(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            return new RepositoryResponse<TResult>()
            {
                IsSucceed = false,                
                Exception = (ex.InnerException ?? ex),
                Errors = errors
            };
        }

        public static RepositoryResponse<TResult> HandleObjectException<TResult>(Exception ex, bool isRoot, IDbContextTransaction transaction)
        {
            if (isRoot)
            {
                //if current transaction is root transaction
                transaction.Rollback();
            }
            List<string> errors = new List<string>();
            LogException(ex);
            errors.Add(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            return new RepositoryResponse<TResult>()
            {
                IsSucceed = false,
                Data = default,
                Exception = (ex.InnerException ?? ex),
                Errors = errors
            };
        }

        public static void LogException(Exception ex)
        {
            string fullPath = string.Format($"{Environment.CurrentDirectory}/logs");
            if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            string filePath = $"{fullPath}/{DateTime.Now.ToString("YYYYMMDD")}/log_exceptions.json";

            try
            {
                FileInfo file = new FileInfo(filePath);
                string content = "[]";
                if (file.Exists)
                {
                    using (StreamReader s = file.OpenText())
                    {
                        content = s.ReadToEnd();
                    }
                    File.Delete(filePath);
                }

                JArray arrExceptions = JArray.Parse(content);
                JObject jex = new JObject
                {
                    new JProperty("CreatedDateTime", DateTime.UtcNow),
                    new JProperty("Details", JObject.FromObject(ex))
                };
                arrExceptions.Add(jex);
                content = arrExceptions.ToString();

                using (var writer = File.CreateText(filePath))
                {
                    writer.WriteLine(content);
                }
            }
            catch
            {
                // File invalid
            }
        }

        public static void InitTransaction(TDbContext _context, IDbContextTransaction _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot)
        {
            isRoot = _context == null;
            context = _context ?? InitContext();
            transaction = _transaction ?? context.Database.BeginTransaction();
        }
    }
}