using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MyORM.ExpressionTools
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
            return temp;
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
        /// 根据传入的方法生成SQL语句，对应sql语句的参数值
        /// </summary>
        /// <param name="sql">生成的SQL语句</param>
        /// <param name="exp">方法表达式</param>
        /// <returns>表达式中的值(SQL语句中的查询条件值)</returns>
        private static string DealMethodCallExpression(out string sql, MethodCallExpression exp)
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
                 sql = "";
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
                    //value = value.Trim('\"');
                    //value = value.Trim();
                    //value = value.Replace("'","");
                    value = string.Format("{0}{1}{2}","%",value,"%");
                    sql = " " + propname + " like @"+propname;
                    return value;
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
                    //value = value.Trim('\"');
                    //value = value.Trim();
                    //value = value.Replace("'", "");
                    value = string.Format("{0}{1}", value, "%");
                    sql = " " + propname + " like @" + propname;
                    return value;
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
                    //value = value.Trim('\"');
                    //value = value.Trim();
                    //value = value.Replace("'", "");
                    value = string.Format("{0}{1}", "%", value);
                    sql = " " + propname + " like @" + propname;
                    return value;
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
                    //value = value.Trim('\"');
                    //value = value.Trim();
                    //value = value.Replace("'", "");
                    sql = " " + propname + " = @" + propname;
                    return value;
                }
                else
                {
                    throw new ArgumentException("暂不支持您输入的方法：" + methodName);
                }
            }
            #endregion

            //非string 类型的动态方法
            else
            {
                throw new ArgumentException("暂不支持您输入的方法");
            }
        }

        private static string DealBinaryExpression(BinaryExpression be,List<object>paramList,List<string>propNms)
        {
            if (be.NodeType == ExpressionType.And || be.NodeType == ExpressionType.AndAlso)
            {
                return DealExpression(be.Left,paramList,propNms) + " and " + DealExpression(be.Right, paramList, propNms);
            }
            else if (be.NodeType == ExpressionType.Or || be.NodeType == ExpressionType.OrElse)
            {
                return DealExpression(be.Left, paramList, propNms) + " or " + DealExpression(be.Right, paramList, propNms);
            }
            else if (be.NodeType == ExpressionType.Equal)
            {
                return DealExpression(be.Left, paramList, propNms) + " = " + DealExpression(be.Right, paramList, propNms);
            }
            else if (be.NodeType == ExpressionType.LessThan)
            {
                return DealExpression(be.Left, paramList, propNms) + " < " + DealExpression(be.Right, paramList, propNms);
            }
            else if (be.NodeType == ExpressionType.LessThanOrEqual)
            {
                return DealExpression(be.Left, paramList, propNms) + " <= " + DealExpression(be.Right, paramList, propNms);
            }
            else if (be.NodeType == ExpressionType.GreaterThan)
            {
                return DealExpression(be.Left, paramList, propNms) + " > " + DealExpression(be.Right, paramList, propNms);
            }
            else if (be.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return DealExpression(be.Left, paramList, propNms) + " >= " + DealExpression(be.Right, paramList, propNms);
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
                //Date.Now 可能要特殊处理
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
        /// 处理C#Lambda表达式
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static List<object> DealExpression(out string sql,Expression ex)
        {
            List<object> paramList = new List<object>();
            List<string> paramNms = new List<string>();
            sql = DealExpression(ex, paramList, paramNms);
            foreach (string item in paramNms)
            {
                sql = sql.ReplaceFirst("#","@"+item);
            }
            return paramList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex">Lambda表达式</param>
        /// <param name="paramList">参数值集合</param>
        /// <param name="paramNm">参数名集合</param>
        /// <returns></returns>
        private static string DealExpression(Expression ex, List<object>paramList, List<string> paramNms)
        {
            if (ex is MemberExpression)
            {
                object ret = DealMemberExpression(ex as MemberExpression);
                if ((ex as MemberExpression).Expression.NodeType == ExpressionType.Parameter)
                {
                    // property name
                    paramNms.Add(ret.ToString());
                    return ret.ToString();
                }
                else
                {
                    // property value
                    paramList.Add(ret);
                    // 暂时用#占位，最后生成时再替换成@PropertyName
                    return "#";
                }
            }
            else if (ex is ConstantExpression)
            {
                object ret = DealConstantExpression(ex as ConstantExpression);
                paramList.Add(ret);
                return "#";
            }
            else if (ex is MethodCallExpression)
            {
                string sql = "";
                string val = DealMethodCallExpression(out sql,ex as MethodCallExpression);
                paramList.Add(val);
                return sql;
            }
            else if (ex is BinaryExpression)
            {
                return DealBinaryExpression(ex as BinaryExpression,paramList,paramNms);
            }
            else
            {
                throw new ArgumentException("暂不支持您输入的表达式类型：" + ex.NodeType);
            }

        }

        /// <summary>
        /// 替换第一个匹配到的字符串子串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="oldStr"></param>
        /// <param name="newStr"></param>
        /// <returns></returns>
        private static string ReplaceFirst(this string value, string oldStr, string newStr)
        {
            if (string.IsNullOrEmpty(oldStr))
                return value;
            int pos = value.IndexOf(oldStr);
            if (0 < pos)
            {
                value = value.Remove(pos, oldStr.Length);
                value = value.Insert(pos,newStr);
            }
            return value;
        }

    }
}
