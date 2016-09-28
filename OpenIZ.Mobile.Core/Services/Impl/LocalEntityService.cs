﻿/*
 * Copyright 2015-2016 Mohawk College of Applied Arts and Technology
 * 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: khannan
 * Date: 2016-9-27
 */

using OpenIZ.Core.Model.Entities;
using OpenIZ.Core.Services;
using OpenIZ.Mobile.Core.Synchronization;
using OpenIZ.Mobile.Core.Synchronization.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OpenIZ.Mobile.Core.Services.Impl
{
	/// <summary>
	/// Represents an entity repository service.
	/// </summary>
	public class LocalEntityRepositoryService : IEntityRepositoryService
	{
		/// <summary>
		/// The internal reference to the <see cref="IDataPersistenceService{TData}"/> instance.
		/// </summary>
		private IDataPersistenceService<Entity> persistenceService;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalEntityRepositoryService"/> class.
		/// </summary>
		public LocalEntityRepositoryService()
		{
			ApplicationContext.Current.Started += (o, e) => { this.persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<Entity>>(); };
		}

		/// <summary>
		/// Finds a list of entities.
		/// </summary>
		/// <param name="query">The query to use to find the entities.</param>
		/// <returns>Returns a list of entities.</returns>
		public IEnumerable<Entity> Find(Expression<Func<Entity, bool>> query)
		{
			if (this.persistenceService == null)
			{
				throw new InvalidOperationException(string.Format("Unable to locate persistence service: {0}", nameof(IDataPersistenceService<Entity>)));
			}

			return persistenceService.Query(query);
		}

		/// <summary>
		/// Finds a list of entities.
		/// </summary>
		/// <param name="query">The query to use to find the entities.</param>
		/// <param name="offSet">The offset of the query.</param>
		/// <param name="count">The count of the query.</param>
		/// <param name="totalCount">The total count of the query.</param>
		/// <returns>Returns a list of entities.</returns>
		public IEnumerable<Entity> Find(Expression<Func<Entity, bool>> query, int offSet, int? count, out int totalCount)
		{
			if (this.persistenceService == null)
			{
				throw new InvalidOperationException(string.Format("Unable to locate persistence service: {0}", nameof(IDataPersistenceService<Entity>)));
			}

			return persistenceService.Query(query, offSet, count, out totalCount);
		}

		/// <summary>
		/// Gets an entity.
		/// </summary>
		/// <param name="key">The key of the entity to be retrieved.</param>
		/// <param name="versionKey">The version key of the entity to be retrieved.</param>
		/// <returns>Returns an entity.</returns>
		public Entity Get(Guid key, Guid versionKey)
		{
			if (this.persistenceService == null)
			{
				throw new InvalidOperationException(string.Format("Unable to locate persistence service: {0}", nameof(IDataPersistenceService<Entity>)));
			}

			return persistenceService.Get(key);
		}

		/// <summary>
		/// Inserts an entity.
		/// </summary>
		/// <param name="entity">The entity to be inserted.</param>
		/// <returns>Returns the inserted entity.</returns>
		public Entity Insert(Entity entity)
		{
			if (this.persistenceService == null)
			{
				throw new InvalidOperationException(string.Format("Unable to locate persistence service: {0}", nameof(IDataPersistenceService<Entity>)));
			}

			var result = this.persistenceService.Insert(entity);

			SynchronizationQueue.Outbound.Enqueue(result, DataOperationType.Insert);

			return result;
		}

		/// <summary>
		/// Obsoletes an entity.
		/// </summary>
		/// <param name="key">The key of the entity to be obsoleted.</param>
		/// <returns>Returns the obsoleted entity.</returns>
		public Entity Obsolete(Guid key)
		{
			if (this.persistenceService == null)
			{
				throw new InvalidOperationException(string.Format("Unable to locate persistence service: {0}", nameof(IDataPersistenceService<Entity>)));
			}

			var result = this.persistenceService.Obsolete(new Entity { Key = key });

			SynchronizationQueue.Outbound.Enqueue(result, DataOperationType.Obsolete);

			return result;
		}

		/// <summary>
		/// Saves or inserts an entity.
		/// </summary>
		/// <param name="entity">The entity to be saved.</param>
		/// <returns>Returns the saved entity.</returns>
		public Entity Save(Entity entity)
		{
			if (this.persistenceService == null)
			{
				throw new InvalidOperationException(string.Format("Unable to locate persistence service: {0}", nameof(IDataPersistenceService<Entity>)));
			}

			Entity result = null;

			try
			{
				result = persistenceService.Update(entity);
				SynchronizationQueue.Outbound.Enqueue(result, DataOperationType.Update);
			}
			catch (KeyNotFoundException)
			{
				result = persistenceService.Insert(entity);
				SynchronizationQueue.Outbound.Enqueue(result, DataOperationType.Insert);
			}

			return result;
		}
	}
}