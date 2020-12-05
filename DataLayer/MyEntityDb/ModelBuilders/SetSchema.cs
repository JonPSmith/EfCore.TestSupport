// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb.ModelBuilders
{
    public static class SetSchema
    {
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity>().ToTable("MyEntities", "MySchema");
        }
    }
}