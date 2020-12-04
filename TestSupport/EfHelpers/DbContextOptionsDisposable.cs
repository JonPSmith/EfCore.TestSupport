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
        private readonly DbConnection _connection;

        public DbContextOptionsDisposable(DbContextOptions<T> baseOptions)
             : base(new ReadOnlyDictionary<Type, IDbContextOptionsExtension>(
                 baseOptions.Extensions.ToDictionary(x => x.GetType())))
        {
            _connection = RelationalOptionsExtension.Extract(baseOptions).Connection;
        }

        public void Dispose() => _connection.Dispose();
    }
}