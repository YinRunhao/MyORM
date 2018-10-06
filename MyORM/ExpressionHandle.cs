using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyORM
{
    /// <summary>
    /// 处理C#表达式并返回SQL字符串的静态类
    /// </summary>
    internal static class ExpressionHandle
    {
        /// <summary>
        /// 递归获取MemberExperssion的值
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        private static object DealMemberExpression(MemberExpression me)
        {
            Stack<MemberInfo> stack = new Stack<MemberInfo>();
            return DealMemberExpression(me, stack);
        }

        /// <summary>
        /// 递归获取MemberExperssion的值
        /// </summary>
        /// <param name="me"></param>
        /// <param name="stack">成员值可能多被多重打包，解开打包时需要一个栈记录成员信息</param>
        /// <returns></returns>
        private static object DealMemberExpression(Expression me, Stack<MemberInfo> stack)
        {
            //还有封装需要继续解
            if (me is MemberExpression)
            {
                var member = me as MemberExpression;
                if (member.Expression.NodeType == ExpressionType.Parameter)
                    return member.Member.Name;
                stack.Push(member.Member);
                return DealMemberExpression(member.Expression, stack);
            }
            //没有封装，开始取值
            else if (me is ConstantExpression)
            {
                var constant = me as ConstantExpression;
                return GetMemberValue(stack, constant.Value);
            }
            //传参不正确
            else
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// 根据栈中的成员信息逐步获取目标值
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object GetMemberValue(Stack<MemberInfo> stack, object value)
        {
            object temp = value;
            foreach (var member in stack)
            {
                if (member is FieldInfo)
                {
                    temp = ((FieldInfo)member).GetValue(temp);
                }
                else if (member is PropertyInfo)
                {
                    temp = ((PropertyInfo)member).GetValue(temp);
                }
            }
            if (temp == null)
            {
                return "'NULL'";
            }
            return "'"+temp.ToString()+"'";
        }

        /// <summary>
        /// 获取ConstantExpression中的值等价于获取该对象的Value属性
        /// </summary>
        /// <param name="ce"></param>
        /// <returns></returns>
        private static object DealConstantExpression(ConstantExpression ce)
        {
            return ce.Value;
        }

        /// <summary>
        /// 根据传入的方法生成SQL语句
        /// </summary>
        /// <param name="mc"></param>
        /// <returns>返回该方法对应的SQL where语句</returns>
        private static string DealMethodCallExpression(MethodCallExpression exp)
        {
            var obj = exp.Object as System.Linq.Expressions.MemberExpression;
            if (obj == null)
            {
                throw new ArgumentException("暂不支持解析静态方法的调用");
            }
            string propname = obj.Member.Name;
            //获取表达式中包含的方法的定义类
            string defineType = exp.Method.DeclaringType.FullName;

            #region 该方法是System.String类的方法
            if (defineType == typeof(string).FullName)
            {
                string sql = "";
                //方法名
                string methodName = exp.Method.Name;
                //按不同方法生成不同的SQL语句
                if (methodName == "Contains")
                {
                    var val = exp.Arguments[0];
                    object valObj = null;
                    if (val is MemberExpression)
                    {
                        valObj = DealMemberExpression(val as MemberExpression);
                    }
                    else if (val is ConstantExpression)
                    {
                        valObj = DealConstantExpression(val as ConstantExpression);
                    }
                    else
                    {
                        throw new ArgumentException("方法中的表达式暂不能识别");
                    }
                    string value = valObj.ToString();
                    value = value.Trim('\"');
                    value = value.Trim();
                    value = value.Replace("'","");
                    sql = " " + propname + " like '%" + value + "%'";
                }
                else if (methodName == "StartsWith")
                {
                    var val = exp.Arguments[0];
                    object valObj = null;
                    if (val is MemberExpression)
                    {
                        valObj = DealMemberExpression(val as MemberExpression);
                    }
                    else if (val is ConstantExpression)
                    {
                        valObj = DealConstantExpression(val as ConstantExpression);
                    }
                    else
                    {
                        throw new ArgumentException("方法中的表达式暂不能识别");
                    }

                    string value = valObj.ToString();
                    value = value.Trim('\"');
                    value = value.Trim();
                    value = value.Replace("'", "");
                    sql = " " + propname + " like '" + value + "%'";
                }
                else if (methodName == "EndsWith")
                {
                    var val = exp.Arguments[0];
                    object valObj = null;
                    if (val is MemberExpression)
                    {
                        valObj = DealMemberExpression(val as MemberExpression);
                    }
                    else if (val is ConstantExpression)
                    {
                        valObj = DealConstantExpression(val as ConstantExpression);
                    }
                    else
                    {
                        throw new ArgumentException("方法中的表达式暂不能识别");
                    }

                    string value = valObj.ToString();
                    value = value.Trim('\"');
                    value = value.Trim();
                    value = value.Replace("'", "");
                    sql = " " + propname + " like '%" + value + "'";
                }
                else if (methodName == "Equals")
                {
                    var val = exp.Arguments[0];
                    object valObj = null;
                    if (val is MemberExpression)
                    {
                        valObj = DealMemberExpression(val as MemberExpression);
                    }
                    else if (val is ConstantExpression)
                    {
                        valObj = DealConstantExpression(val as ConstantExpression);
                    }
                    else
                    {
                        throw new ArgumentException("方法中的表达式暂不能识别");
                    }

                    string value = valObj.ToString();
                    value = value.Trim('\"');
                    value = value.Trim();
                    value = value.Replace("'", "");
                    sql = " " + propname + " = '" + value + "'";
                }
                else
                {
                    throw new ArgumentException("暂不支持您输入的方法：" + methodName);
                }
                return sql;
            }
            #endregion

            //非string 类型的动态方法
            else
            {
                throw new ArgumentException("暂不支持您输入的方法");
            }
        }

        private static string DealBinaryExpression(BinaryExpression be)
        {
            if (be.NodeType == ExpressionType.And || be.NodeType == ExpressionType.AndAlso)
            {
                return DealExpression(be.Left) + " and " + DealExpression(be.Right);
            }
            else if (be.NodeType == ExpressionType.Or || be.NodeType == ExpressionType.OrElse)
            {
                return DealExpression(be.Left) + " or " + DealExpression(be.Right);
            }
            else if (be.NodeType == ExpressionType.Equal)
            {
                return DealExpression(be.Left) + " = " + DealExpression(be.Right);
            }
            else if (be.NodeType == ExpressionType.LessThan)
            {
                return DealExpression(be.Left) + " < " + DealExpression(be.Right);
            }
            else if (be.NodeType == ExpressionType.LessThanOrEqual)
            {
                return DealExpression(be.Left) + " <= " + DealExpression(be.Right);
            }
            else if (be.NodeType == ExpressionType.GreaterThan)
            {
                return DealExpression(be.Left) + " > " + DealExpression(be.Right);
            }
            else if (be.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return DealExpression(be.Left) + " >= " + DealExpression(be.Right);
            }
            else
            {
                throw new ArgumentException("暂时不支持:" + be.NodeType + "表达式");
            }
        }

        public static string DealGetPropertyNameExpression<T>(Expression<Func<T, object>> ex)
        {
            if (ex.NodeType == ExpressionType.Lambda)
            {
                if (ex.Body.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression me = ex.Body as MemberExpression;
                    return me.Member.Name;
                }
                else if (ex.Body.NodeType == ExpressionType.Convert)
                {
                    UnaryExpression ue = ex.Body as UnaryExpression;
                    MemberExpression me = ue.Operand as MemberExpression;
                    return me.Member.Name;
                }
                else if (ex.Body.NodeType == ExpressionType.Parameter)
                {
                    ParameterExpression pe = ex.Body as ParameterExpression;
                    return pe.Name;
                }
            }
            throw new ArgumentException("传入的表达式需要获取一个属性，暂不支持获取嵌套对象的属性");
        }

        /// <summary>
        /// 处理C#表达式返回SQL语句
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string DealExpression(Expression ex)
        {
            if (ex is MemberExpression)
            {
                return DealMemberExpression(ex as MemberExpression).ToString();
            }
            else if (ex is ConstantExpression)
            {
                return "'" + DealConstantExpression(ex as ConstantExpression).ToString() + "'";
            }
            else if (ex is MethodCallExpression)
            {
                return DealMethodCallExpression(ex as MethodCallExpression);
            }
            else if (ex is BinaryExpression)
            {
                return DealBinaryExpression(ex as BinaryExpression);
            }
            else
            {
                throw new ArgumentException("暂不支持您输入的表达式类型：" + ex.NodeType);
            }

        }

    }
}
