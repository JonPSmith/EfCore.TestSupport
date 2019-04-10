// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TestSupport.EfHelpers.Internal
{
    internal class MemberAnonymiseData
    {
        public MemberAnonymiseData(Type classType, PropertyInfo propertyToAnonymise, string replaceRequest)
        {
            ClassType = classType;
            PropertyToAnonymise = propertyToAnonymise;
            ReplaceRequest = replaceRequest;
            AnonymiserData = new AnonymiserData(ReplaceRequest);
        }

        public Type ClassType { get; private set; }
        public PropertyInfo PropertyToAnonymise { get; private set; }
        public string ReplaceRequest { get; private set; }

        public AnonymiserData AnonymiserData { get; private set; }

        public void AnonymiseMember(object entityToUpdate, DataResetterConfig config)
        {
            PropertyToAnonymise.SetValue(entityToUpdate, config.AnonymiserFunc(AnonymiserData, entityToUpdate));
        }

        //thanks to https://www.codeproject.com/Tips/301274/How-to-get-property-name-using-Expression-2
        public static PropertyInfo GetPropertyViaLambda<T>(Expression<Func<T, string>> expression)
        {
            var body = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;

            return (PropertyInfo)body?.Member ?? throw new ArgumentException("You must call this with ...<MyEntity>(p => p.PropertyInMyEntity)");
        }
    }
}