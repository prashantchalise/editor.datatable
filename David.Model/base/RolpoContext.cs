using David.Model.ChaliseStoredProc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace David.Model
{

	public interface IContext
	{
         DbSet<TEntity> Set<TEntity>() where TEntity : class;
		DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

		ResultsList CallSP(StoredProc procedure, IEnumerable<SqlParameter> parms = null);
		ResultsList CallSP<T>(StoredProc<T> procedure, T data);

		int SaveChanges();
	}


	public class RolpoContext : DbContext, IContext
	{
 
        public RolpoContext()
			: base("Name=RolpoContext")
		{
			//this.Configuration.LazyLoadingEnabled = false; 
		}


		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			base.OnModelCreating(modelBuilder);
		}

		public override int SaveChanges()
		{
			var modifiedEntries = ChangeTracker.Entries()
				.Where(x => x.Entity is IAuditableEntity
					&& (x.State == System.Data.Entity.EntityState.Added || x.State == System.Data.Entity.EntityState.Modified || x.State == System.Data.Entity.EntityState.Deleted));

			foreach (var entry in modifiedEntries)
			{
				IAuditableEntity entity = entry.Entity as IAuditableEntity;
				if (entity != null)
				{
					string identityName = Thread.CurrentPrincipal.Identity.Name;
					DateTime now = DateTime.UtcNow;

					if (entry.State == System.Data.Entity.EntityState.Added)
					{
						entity.CreatedBy = identityName;
						entity.CreatedDate = now;

						entity.UpdatedDate = null;
						entity.DeletedDate = null;

					}
					else if (entry.State == System.Data.Entity.EntityState.Modified)
					{
						entity.UpdatedBy = identityName;
						entity.UpdatedDate = now;

						entity.DeletedDate = null;

					}
					else
					{
						entry.State = EntityState.Modified;

						entity.DeletedBy = identityName;
						entity.DeletedDate = now;
					}


				}
			}

			return base.SaveChanges();
		}


		public ResultsList CallSP(StoredProc procedure, IEnumerable<SqlParameter> parms = null)
		{
			return  this.CallStoredProc(procedure, parms);
		}

		public ResultsList CallSP<T>(StoredProc<T> procedure, T data)
		{
			return this.CallStoredProc<T>(procedure, data);
		}
	}

	
}
