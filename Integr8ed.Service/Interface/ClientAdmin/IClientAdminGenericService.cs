using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface.BaseInterface
{
    public interface IClientAdminGenericService<T> where T : class
    {
        T GetSingle(string connectionstring , Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] includeProperties);
        Task<T> GetSingleAsync(string connectionstring,  Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] includeProperties );

        IEnumerable<T> GetAll(string connectionstring,  bool asNoTracking = false);
        IEnumerable<T> GetAllRecords(string connectionstring,  bool asNoTracking = false);
        IEnumerable<T> GetAll(string connectionstring,  Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] includePropertie );

        T GetById(string connectionstring,  object id);
        IEnumerable<T> FindBy(string connectionstring,  Expression<Func<T, bool>> predicate, bool asNoTracking = false);

        int GetCount(string connectionstring,  Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);

        void Add(string connectionstring,  T entity);
        void AddAsync(string connectionstring,  T entity);
        void AddRange(string connectionstring, IEnumerable<T> entity);
        void AddRangeAsync(string connectionstring, IEnumerable<T> entity);
        void Update(string connectionstring, T entity);
        void Delete(string connectionstring, T entity);
        void DeleteRange(string connectionstring, IEnumerable<T> entity);
        void Save(string connectionstring);

        void Detach(string connectionstring, T entity);

        Task<T> InsertAsync(string connectionstring, T entity, IHttpContextAccessor accessor, long? userId = null);
        Task<T> UpdateAsync(string connectionstring, T entity, IHttpContextAccessor accessor, long? userId = null);
        Task<IEnumerable<T>> InsertRangeAsync(string connectionstring, IEnumerable<T> entity, IHttpContextAccessor accessor, long? userId = null);
        void DeleteRange(string connectionstring, IEnumerable<T> entity, IHttpContextAccessor accessor, long? userId = null);
        Task<T> DeleteAsync(string connectionstring, T entity, IHttpContextAccessor accessor, long? userId = null);
        Task SaveAsync(string connectionstring);
    }
}
