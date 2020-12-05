// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This is used to return a class that implements <see cref="DbContextOptions{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbContextOptionsDisposable<T> : DbContextOptions<T>, IDisposable where T : DbContext
    {
        private bool _stopNextDispose;
        private bool _turnOffDispose;
        private readonly DbConnection _connection;

        /// <summary>
        /// This creates the class and sets up the <see cref="DbContextOptions{T}"/> part and getting a reference to the connection
        /// </summary>
        /// <param name="baseOptions"></param>
        public DbContextOptionsDisposable(DbContextOptions<T> baseOptions)
             : base(new ReadOnlyDictionary<Type, IDbContextOptionsExtension>(
                 baseOptions.Extensions.ToDictionary(x => x.GetType())))
        {
            _connection = RelationalOptionsExtension.Extract(baseOptions).Connection;
        }

        /// <summary>
        /// Use this to stop the Dispose if you want to create a second context to the same connection.
        /// You should call this BEFORE you create the DbContext
        /// </summary>
        public void StopNextDispose()
        {
            _stopNextDispose = true;
        }

        /// <summary>
        /// If you have lots of separate application DbContext's then use this to stop the connection from being removed.
        /// But remember to call <see cref="ManualDispose"/> at the end of your unit test
        /// </summary>
        public void TurnOffDispose()
        {
            _turnOffDispose = true;
        }

        /// <summary>
        /// If you used <see cref="TurnOffDispose"/>, then you should call this at the end of your unit test
        /// That will dispose the connection.
        /// </summary>
        public void ManualDispose()
        {
            _turnOffDispose = false;
            _stopNextDispose = false;
            Dispose();
        }

        /// <summary>
        /// This disposes the Sqlite connection with holds the in-memory data 
        /// </summary>
        public void Dispose()
        {
            if (!_stopNextDispose && !_turnOffDispose)
                _connection.Dispose();
            _stopNextDispose = false;
        }
    }
}