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
    public class DbContextOptionsDisposable<T> : DbContextOptions<T>, IDisposable where T : DbContext
    {
        private bool _stopNextDispose;
        public DbConnection Connection { get; private set; }

        public DbContextOptionsDisposable(DbContextOptions<T> baseOptions)
             : base(new ReadOnlyDictionary<Type, IDbContextOptionsExtension>(
                 baseOptions.Extensions.ToDictionary(x => x.GetType())))
        {
            Connection = RelationalOptionsExtension.Extract(baseOptions).Connection;
        }

        /// <summary>
        /// Use this to stop the Dispose if you want to create a second context to the same 
        /// </summary>
        public void StopNextDispose()
        {
            _stopNextDispose = true;
        }

        public void Dispose()
        {
            if (!_stopNextDispose)
                Connection.Dispose();
            _stopNextDispose = false;
        }
    }
}