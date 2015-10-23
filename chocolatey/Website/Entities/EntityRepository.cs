// Copyright 2011 - Present RealDimensions Software, LLC, the original 
// authors/contributors from ChocolateyGallery
// at https://github.com/chocolatey/chocolatey.org,
// and the authors/contributors of NuGetGallery 
// at https://github.com/NuGet/NuGetGallery
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using NugetGallery;

namespace NuGetGallery
{
    public class EntityRepository<T> : IEntityRepository<T>
        where T : class, IEntity, new()
    {
        private readonly IEntitiesContext entities;

        public EntityRepository(IEntitiesContext entities)
        {
            this.entities = entities;
        }

        public void CommitChanges()
        {
            entities.SaveChanges();
        }

        public void DeleteOnCommit(T entity)
        {
            entities.Set<T>()
                    .Remove(entity);
        }

        public T Get(int key)
        {
            return Cache.Get(string.Format("item-{0}-{1}", typeof(T).Name, key), 
                    DateTime.Now.AddMinutes(Cache.DEFAULT_CACHE_TIME_MINUTES), 
                    () => entities.Set<T>().Find(key));
        }

        public IQueryable<T> GetAll()
        {
            return entities.Set<T>();
        }

        public int InsertOnCommit(T entity)
        {
            entities.Set<T>()
                    .Add(entity);

            return entity.Key;
        }
    }
}
