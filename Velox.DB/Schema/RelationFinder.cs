#region License
//=============================================================================
// Velox.DB - Portable .NET ORM 
//
// Copyright (c) 2015 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Velox.DB
{
    internal class LambdaRelationFinder : ExpressionVisitor
    {
        private LambdaRelationFinder(Vx.Context context)
        {
            _context = context;
        }

        private readonly HashSet<OrmSchema.Relation> _relations = new HashSet<OrmSchema.Relation>();
        private readonly Vx.Context _context;

        protected override Expression VisitMember(MemberExpression node)
        {
            Visit(node.Expression);

            var schema = _context.GetSchema(node.Expression.Type, autoCreate: false);
            var relation = schema?.Relations[node.Member.Name];

            if (relation == null) 
                return node;

            _relations.Add(relation);

            return node;
        }

        public static HashSet<OrmSchema.Relation> FindRelations(LambdaExpression expression, OrmSchema schema)
        {
            var finder = new LambdaRelationFinder(schema.Repository.Context);

            finder.Visit(expression.Body);

            return finder._relations;
        }

        public static HashSet<OrmSchema.Relation> FindRelations(IEnumerable<LambdaExpression> expressions, OrmSchema schema)
        {
            if (expressions == null)
                return null;

            HashSet<OrmSchema.Relation> relations = null;

            foreach (var lambdaExpression in expressions)
            {
                if (relations == null)
                    relations = new HashSet<OrmSchema.Relation>(FindRelations(lambdaExpression,schema));
                else
                    relations.UnionWith(FindRelations(lambdaExpression, schema));
            }

            return relations;
        }
    }
}