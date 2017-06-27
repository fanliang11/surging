// Copyright 2007-2010 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.Reflection
{
    using System;
    using System.Linq.Expressions;


    public class CurryExpressionVisitor<T1, T2, TResult> :
        ExpressionVisitor
    {
        ConstantExpression _replace;
        ParameterExpression _search;

        public Expression<Func<T1, TResult>> Curry(Expression<Func<T1, T2, TResult>> expression, T2 value)
        {
            _replace = Expression.Constant(value, typeof(int));
            _search = expression.Parameters[1];

            var result = Visit(expression) as LambdaExpression;
            if (result == null)
                throw new InvalidOperationException("Unable to curry expression: " + expression);

            Expression<Func<T1, TResult>> response = Expression.Lambda<Func<T1, TResult>>(result.Body,
                                                                                          result.Parameters[0]);

            return response;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (p == _search)
                return _replace;

            return base.VisitParameter(p);
        }
    }
}