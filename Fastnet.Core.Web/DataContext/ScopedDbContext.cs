using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fastnet.Core.Web
{
    /// <summary>
    /// A disposable dbcontext for use outside a web request (e.g.  a scheduled/real time task)
    /// </summary>
    /// <typeparam name="T">type of DbContext</typeparam>
    public class ScopedDbContext<T> : IDisposable where T : DbContext
    {
        private DbContext db;
        private IServiceScope scope;
        private T _db;
        private readonly IServiceProvider serviceProvider;
        /// <summary>
        /// The required db context
        /// </summary>
        public T Db
        {
            get
            {
                if (_db == null)
                {
                    _db = GetDBContext();
                }
                return _db;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp"></param>
        public ScopedDbContext(IServiceProvider sp)
        {
            this.serviceProvider = sp;
        }
        /// <summary>
        /// 
        /// </summary>
        ~ScopedDbContext()
        {
            Dispose();
        }
        private T GetDBContext()
        {
            scope = this.serviceProvider.CreateScope();
            db = scope.ServiceProvider.GetService<T>();
            return (T)db;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            db?.Dispose();
            scope?.Dispose();
        }
    }
}
