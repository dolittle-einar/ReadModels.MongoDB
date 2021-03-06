﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dolittle. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 * --------------------------------------------------------------------------------------------*/
using System.Linq;
using System.Reflection;
using Dolittle.Concepts;
using Dolittle.ReadModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.ReadModels.MongoDB
{
    /// <summary>
    /// Represents an implementation of <see cref="IReadModelRepositoryFor{T}"/> for MongoDB
    /// </summary>
    public class ReadModelRepositoryFor<T> : IReadModelRepositoryFor<T> where T : Dolittle.ReadModels.IReadModel
    {
        readonly string _collectionName = RemoveReadNamespace(typeof(T).FullName);
        readonly Configuration _configuration;
        readonly IMongoCollection<T> _collection;

        /// <summary>
        /// Initializes a new instance of <see cref="ReadModelRepositoryFor{T}"/>
        /// </summary>
        /// <param name="configuration"><see cref="Configuration"/> to use</param>
        public ReadModelRepositoryFor(Configuration configuration)
        {
            _configuration = configuration;
            _collection = configuration.Database.GetCollection<T>(_collectionName);
        }

        /// <inheritdoc/>
        public IQueryable<T> Query => _collection.AsQueryable<T>();

        /// <inheritdoc/>
        public void Delete(T readModel)
        {
            var objectId = readModel.GetObjectIdFrom();
            _collection.DeleteOne(Builders<T>.Filter.Eq("_id", objectId));
        }

        /// <inheritdoc/>
        public T GetById(object id)
        {
            var objectId = id.GetIdAsBsonValue();
            return _collection.Find(Builders<T>.Filter.Eq("_id", objectId)).FirstOrDefault();
        }

        /// <inheritdoc/>
        public void Insert(T readModel)
        {
            _collection.InsertOne(readModel);
        }

        /// <inheritdoc/>
        public void Update(T readModel)
        {
            var id = readModel.GetObjectIdFrom();

            var filter = Builders<T>.Filter.Eq("_id", id);
            _collection.ReplaceOne(filter, readModel, new UpdateOptions() { IsUpsert = true });
        }

	    static string RemoveReadNamespace(string source)
        {	
            return source.StartsWith("Read.") ? source.Substring(5) : source;
        }
   }
}