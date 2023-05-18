using Integr8ed.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Integr8ed.Service.BaseInterface;
using Integr8ed.Data.DbContext.ClientAdminContext;
using Microsoft.Data.SqlClient;
using Integr8ed.Service.Interface.BaseInterface;
//using Microsoft.AspNetCore.Http;
//using IHttpContextAccessor HttpContextAccessor;

namespace Integr8ed.Service.Implementation.BaseService
{
  public  class ClientAdminGenericRepository <T> : IClientAdminGenericService<T> where T : class
    {

        private  ClientAdminDbContext db;
        private  DbSet<T> _table;
     //   public string connectionstring = "SERVER = C187\\SQLEXPRESS2016; DATABASE = ClientAdmin_0; User ID=sa;Password= admin123!@#";

        protected ClientAdminGenericRepository(ClientAdminDbContext context)
        {
            db = context;
            _table = context.Set<T>();

            //_assessor = accessor;
        }

        public T GetSingle(string connectionstring, Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                    IQueryable<T> query = db.Set<T>();
                    query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
                    return asNoTracking ? query.AsNoTracking().SingleOrDefault(predicate) : query.SingleOrDefault(predicate);
                }
                catch (Exception e) { throw e; }
            }
        }
        public virtual IEnumerable<T> GetAll(string connectionstring, bool asNoTracking = false)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                    return asNoTracking ? _table.AsNoTracking() : _table.AsEnumerable();
                }
                catch (Exception e) { throw e; }
            }
        }

        public IEnumerable<T> GetAllRecords(string cnnstr, bool asNoTracking = false)
        {
            var dbConnection = new SqlConnection(cnnstr);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                    return asNoTracking ? _table.AsNoTracking() : _table.AsEnumerable();
                }
                catch (Exception e) { throw e; }
            }
        }

        public async Task<T> GetSingleAsync(string connectionstring, Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                    IQueryable<T> query = db.Set<T>();
                    if (asNoTracking)
                        return await query.AsNoTracking().SingleOrDefaultAsync(predicate);
                    return await query.SingleOrDefaultAsync(predicate);
                }
                catch (Exception e) { throw e; }
            }
        }
        public IEnumerable<T> GetAll(string connectionstring, Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
              IQueryable<T> query = db.Set<T>();
            query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            return asNoTracking ? query.AsNoTracking().Where(predicate) : query.Where(predicate).AsEnumerable();
                }
                catch (Exception e) { throw e; }
            }
        }

        public T GetById(string connectionstring, object id)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    return _table.Find(id);
                }
                catch (Exception e) { throw e; }
            }
        }

        public int GetCount(string connectionstring, Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    IQueryable<T> query = db.Set<T>();
                    query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
                    return query.Count(predicate);
                }
                catch (Exception e) { throw e; }
            }
        }

        public virtual IEnumerable<T> FindBy(string connectionstring, Expression<Func<T, bool>> predicate, bool asNoTracking = false)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    return asNoTracking ? _table.AsNoTracking().Where(predicate) : _table.Where(predicate).AsEnumerable();
                } catch (Exception e) { throw e; } }
        }

        public virtual void Add(string connectionstring, T entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    _table.Add(entity);
                }
                catch (Exception e) { throw e; }
            }
        }

        public virtual void AddRange(string connectionstring, IEnumerable<T> entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    _table.AddRange(entity);
                }
                catch (Exception e) { throw e; }
            }
        }

        public virtual void Update(string connectionstring, T entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    _table.Attach(entity);
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
                }
                catch (Exception e) { throw e; }
            }
        }

        public virtual void Delete(string connectionstring, T entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    _table.Remove(entity);
            Save(connectionstring);
                }
                catch (Exception e) { throw e; }
            }
        }

        public void DeleteRange(string connectionstring, IEnumerable<T> entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    _table.RemoveRange(entity);
            Save(connectionstring);
                }
                catch (Exception e) { throw e; }
            }
        }

        public virtual void DeleteWhere(string connectionstring, Expression<Func<T, bool>> predicate)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    var entities = _table.Where(predicate);

            foreach (var entity in entities)
            {
                db.Entry<T>(entity).State = EntityState.Deleted;
            }
            Save(connectionstring);
                }
                catch (Exception e) { throw e; }
            }
        }

        public virtual void Save( string connectionstring)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                db.SaveChanges();
            }
        }

        public virtual void Detach(string connectionstring, T entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    EntityEntry dbEntityEntry = db.Entry<T>(entity);
            switch (dbEntityEntry.State)
            {
                case EntityState.Modified:
                    dbEntityEntry.State = EntityState.Unchanged;
                    break;
                case EntityState.Added:
                    dbEntityEntry.State = EntityState.Detached;
                    break;
                case EntityState.Deleted:
                    dbEntityEntry.Reload();
                    break;
                    }
                }
                catch (Exception e) { throw e; }
            }
        }

        /* add With Ranage Async*/
        public virtual void AddAsync(string connectionstring, T entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    _table.AddAsync(entity);
                }
                catch (Exception e) { throw e; }
            }
        }

        public virtual void AddRangeAsync(string connectionstring, IEnumerable<T> entity)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    _table.AddRangeAsync(entity);
                }
                catch (Exception e) { throw e; }
            }
        }

        public async Task<IEnumerable<T>> InsertRangeAsync(string connectionstring, IEnumerable<T> entity, IHttpContextAccessor accessor, long? userId)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    _table = db.Set<T>();
                    if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var insertRangeAsync = entity.ToList();
            for (var i = 0; i < insertRangeAsync.Count; i++)
            {
                //Update default fields for addition and updation
                insertRangeAsync[i] = UpdateDefaultFieldsForAddAndUpdate(connectionstring, insertRangeAsync[i], userId);
            }
            await Task.FromResult(_table.AddRangeAsync(insertRangeAsync));
            await db.SaveChangesAsync();
                
            return insertRangeAsync;
                }
                catch (Exception e) { throw e; }
            }
        }

        /// <summary>
        /// Insert Async Data and Save it to database
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <param name="accessor"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<T> InsertAsync(string connectionstring, T entity, IHttpContextAccessor accessor, long? userId)
        {
            //Update default fields for addition and updation
           
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                    entity = UpdateDefaultFieldsForAddAndUpdate(connectionstring, entity, userId);

                    if (entity == null)
                    {
                        throw new ArgumentNullException(nameof(entity));
                    }
                    await Task.FromResult(_table.AddAsync(entity));

                    await db.SaveChangesAsync();


                }catch(Exception e) { throw e; }
            }
            return entity;
        }

        /* Update With Async*/
        public async Task<T> UpdateAsync(string connectionstring, T entity, IHttpContextAccessor accessor, long? userId)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                    if (entity == null)
                    {
                        throw new ArgumentNullException(nameof(entity));
                    }

                    entity = UpdateDefaultFieldsForAddAndUpdate(connectionstring, entity, userId, true);
                    await Task.FromResult(db.Entry<T>(entity).State = EntityState.Modified);
                    await db.SaveChangesAsync();
                }
                catch (Exception e) { throw e; }
            }
            return entity;
        }

        /* Delete Range*/
        public void DeleteRange(string connectionstring, IEnumerable<T> entity, IHttpContextAccessor accessor, long? userId)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                   

                        if (entity == null)
                        {
                            throw new ArgumentNullException(nameof(entity));
                        }

                        var enumerable = entity.ToList();
                        db.Set<T>().RemoveRange(enumerable);
                        db.SaveChanges();
                    
                }

                catch (Exception e) { throw e; } 
            }
        }

        /* Delete With Async*/
        public async Task<T> DeleteAsync(string connectionstring, T entity, IHttpContextAccessor accessor, long? userId)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                _table = db.Set<T>();
                try
                {
                   
                        if (entity == null)
                        {
                            throw new ArgumentNullException(nameof(entity));
                        }
                        //Update default fields for addition and updation
                        entity = UpdateDefaultFieldsForAddAndUpdate(connectionstring, entity, userId, true);

                        await Task.FromResult(db.Entry<T>(entity).State = EntityState.Deleted);
                        await db.SaveChangesAsync();
                    
                }
                catch (Exception e) { throw e; }
            }
            return entity;
                }

        public virtual async Task SaveAsync(string connectionstring)
        {
            var dbConnection = new SqlConnection(connectionstring);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception e) { throw e; }
            }
        }


        public T UpdateDefaultFieldsForAddAndUpdate(string connectionstring, T entity, long? userId, bool isEdit = false)
        {
            //Add createdBy and CreadetDate
            if (!isEdit)
            {
                if (entity.GetType().GetProperty("CreatedBy") != null)
                {
                    entity.GetType().GetProperty("CreatedBy")?.SetValue(entity, userId);
                }
                if (entity.GetType().GetProperty("CreatedDate") != null)
                {
                    //entity.GetType().GetProperty("CreatedDate")?.SetValue(entity, DateTime.UtcNow);
                    entity.GetType().GetProperty("CreatedDate")?.SetValue(entity, entity.GetType().GetProperty("CreatedDate").GetValue(entity) ?? DateTime.UtcNow);
                }

            }
            //Add updatedby and updatedDate
            else
            {
                if (entity.GetType().GetProperty("ModifiedBy") != null)
                {
                    entity.GetType().GetProperty("ModifiedBy")?.SetValue(entity, userId);
                }
                if (entity.GetType().GetProperty("ModifiedDate") != null)
                {
                    entity.GetType().GetProperty("ModifiedDate")?.SetValue(entity, DateTime.UtcNow);
                }
            }

            return entity;
        }
    }
}
