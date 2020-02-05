﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Chsword.Excel2Object.Internal
{
    internal class ExpressionConvert
    {
        public string[] Columns { get; set; }
        public int RowIndex { get; set; }

        public ExpressionConvert(string[] columns, int rowIndex)
        {
            Columns=columns;
            RowIndex=rowIndex;
        }
        public string Convert(Expression expression
          )
        {
            if (expression == null) return string.Empty;
            if (expression.NodeType == ExpressionType.Lambda)
            {
                return InternalConvert((expression as LambdaExpression)?.Body);
            }

            return string.Empty;
        }

        private string InternalConvert(params Expression[] expressions)
        {
                var expression = expressions[0];
                if (expression == null) return "";
                if (expression.NodeType == ExpressionType.Convert)
                {
                    return InternalConvert((expression as UnaryExpression)?.Operand);
                }

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var member = (expression as MemberExpression)?.Member;
                    if (member == null) return string.Empty;
                    if (member.DeclaringType == typeof(DateTime))
                    {
                        if (member.Name == "Now")
                        {
                            return "NOW()";
                        }
                        if (member.Name == "Year")
                        {
                            return "YEAR(NOW())";
                        }
                        if (member.Name == "Month")
                        {
                            return "MONTH(NOW())";
                        }
                        if (member.Name == "Day")
                        {
                            return "DAY(NOW())";
                        }
                    }
                  

                    return "";
                }

                if (expression.NodeType == ExpressionType.Constant)
                {
                    return (expression as ConstantExpression)?.ToString();
                }
                if(expression.NodeType == ExpressionType.Call)
                {
                    var exp = expression as MethodCallExpression;
                    if(exp.Method.Name == "get_Item" && exp.Object.Type ==typeof(Dictionary<string,object>))
                    {
                        var key=(exp.Arguments[0] as ConstantExpression).Value.ToString();
                        var columnIndex = Array.IndexOf(Columns,key);
                        if(columnIndex == -1)
                        {
                            return $"ERROR key:{key}";
                        }
                        return $"{ExcelColumnNameParser.Parse(columnIndex)}{RowIndex+1}";
                    }
                }
                if(expression.NodeType == ExpressionType.Add)
                {
                    var nodes = expression as BinaryExpression;
                    return $"{InternalConvert( nodes.Left)}+{InternalConvert( nodes.Right)}";
                }
            

            return expressions[0].NodeType.ToString();
        }
    }
}