﻿using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Semmle.Extraction.Kinds;

namespace Semmle.Extraction.CSharp.Entities.Expressions
{
    internal class InterpolatedString : Expression<InterpolatedStringExpressionSyntax>
    {
        private InterpolatedString(ExpressionNodeInfo info) : base(info.SetKind(ExprKind.INTERPOLATED_STRING)) { }

        public static Expression Create(ExpressionNodeInfo info) => new InterpolatedString(info).TryPopulate();

        protected override void PopulateExpression(TextWriter trapFile)
        {
            var child = 0;
            foreach (var c in Syntax.Contents)
            {
                switch (c.Kind())
                {
                    case SyntaxKind.Interpolation:
                        var interpolation = (InterpolationSyntax)c;
                        new InterpolatedStringInsert(Context, interpolation, this, child++);
                        break;
                    case SyntaxKind.InterpolatedStringText:
                        // Create a string literal
                        var interpolatedText = (InterpolatedStringTextSyntax)c;
                        new Expression(new ExpressionInfo(Context, Type, Context.CreateLocation(c.GetLocation()), ExprKind.UTF16_STRING_LITERAL, this, child++, isCompilerGenerated: false, interpolatedText.TextToken.ValueText));
                        break;
                    default:
                        throw new InternalError(c, $"Unhandled interpolation kind {c.Kind()}");
                }
            }
        }
    }
}
